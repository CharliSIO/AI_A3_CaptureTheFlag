using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentController : CharacterController
{

    #region Weighting Variables

    [SerializeField] private float m_SeekPrisonWeight = 1.0f;
    [SerializeField] private float m_SeekFlagsWeight = 1.0f;

    [SerializeField] private float m_DefendPrisonWeight = 1.0f;
    [SerializeField] private float m_DefendFlagsWeight = 1.0f;

    [SerializeField] private float m_ChaseEnemyWeight = 1.0f; // might not be needed?

    [SerializeField] private Team TeamToSeekFlags;
    [SerializeField] private Team TeamToRescueFrom;


    public int RandomiserNumber;
    #endregion

    #region Locomotion Variables
    [SerializeField] private Vector2 m_TargetPos;
    [SerializeField] private Vector2 m_FleeTargetPos;
    [SerializeField] private Character m_TargetCharacter; // change to purse
    [SerializeField] private Character m_FleeCharacter; // change to evade later
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    override protected void Start()
    {
        base.Start();
        StartCoroutine(UpdateStates(1.0f)); // update states only once every 2 seconds
    }

    private void Update()
    {
        switch (m_CurrentState)
        {
            case BehaviourState.EBS_NEUTRAL:
                break;
            case BehaviourState.EBS_SEEK_PRISON:
                SeekPrison();
                break;
            case BehaviourState.EBS_SEEK_FLAGS:
                SeekFlags();
                break;
            case BehaviourState.EBS_DEFEND_PRISON:
                DefendPrison();
                break;
            case BehaviourState.EBS_DEFEND_FLAGS:
                DefendFlags();
                break;
            case BehaviourState.EBS_IN_PRISON:
                InPrison();
                break;
            case BehaviourState.EBS_RETURNING_FROM_PRISON:
                ReturnPrison();
                break;
            case BehaviourState.EBS_RETURNING_WITH_FLAG:
                ReturnFlag();
                break;
            case BehaviourState.EBS_CHASE_ENEMY:
                ChaseEnemy();
                break;
            default:
                break;
        }

        m_SteeringForce = Vector2.zero;
        m_SteeringForce += Seek(m_TargetPos);
        m_SteeringForce += Flee(m_FleeTargetPos);
        if (m_TargetPos != Vector2.zero) m_SteeringForce += Arrive(m_TargetPos);

        Vector2.ClampMagnitude(m_SteeringForce, m_MaxSteeringForce);
        m_Velocity += m_SteeringForce;
        MoveCharacter();
    }

    // coroutine that calls the update states - so not every frame
    private IEnumerator UpdateStates(float _time)
    {
        while (true) // TODO: change this to be a while match in progress or something
        {
            UnityEngine.Random.InitState(RandomiserNumber + (DateTime.Now).Millisecond);
            DecideCurrentState();

            yield return new WaitForSeconds(_time + UnityEngine.Random.Range(0.1f, 0.5f));
        }
    }

    #region Locomotion
    Vector2 CalculateSteeringForce(Vector2 _desiredVel, float _forceweight)
    {
        Vector2 steerForce = _desiredVel - m_Velocity;
        return _forceweight * Time.deltaTime * Vector2.ClampMagnitude(steerForce, m_MaxSteeringForce);
    }

    Vector2 Seek(Vector2 _targetPos)
    {
        return CalculateSteeringForce((_targetPos - m_Position).normalized * m_MaxSpeed, m_SeekForceWeighting);
    }

    Vector2 Flee(Vector2 _targetPos)
    {
        return CalculateSteeringForce((m_Position - _targetPos).normalized * m_MaxSpeed, m_FleeForceWeighting);
    }

    Vector2 Arrive(Vector2 _targetPos)
    {
        Vector2 desVol = m_TargetPos - m_Position;
        float distance = desVol.magnitude;
        if (distance < m_DetectionRadius.radius)
        {
            desVol = (distance / m_DetectionRadius.radius) * m_MaxSpeed * desVol.normalized;
            return CalculateSteeringForce(desVol, m_ArriveForceWeighting);
        }
        return Vector2.zero;
    }

    // TODO
    Vector2 Separation()
    {
        return new();
    }

    // TODO
    Vector2 Cohesion()
    {
        return new();
    }

    // TODO
    Vector2 Alignment()
    {
        return new();
    }

    #endregion

    // update weightings based on rules
    void UpdateDecisionWeightings()
    {
        m_SeekPrisonWeight = UnityEngine.Random.Range(0.5f, 2.0f) + (m_CurrentState == BehaviourState.EBS_SEEK_PRISON ? 1.0f : 0.0f );
        m_SeekFlagsWeight = UnityEngine.Random.Range(0.5f, 2.0f) + (m_CurrentState == BehaviourState.EBS_SEEK_FLAGS ? 1.0f : 0.0f);
        m_DefendPrisonWeight = UnityEngine.Random.Range(0.1f, 0.2f) + (m_CurrentState == BehaviourState.EBS_DEFEND_PRISON ? 1.0f : 0.0f);
        m_DefendFlagsWeight = UnityEngine.Random.Range(0.1f, 1.0f) + (m_CurrentState == BehaviourState.EBS_DEFEND_FLAGS ? 1.0f : 0.0f); 
        m_ChaseEnemyWeight = 0.0f;

        // check shared memory to see what other team members are doing
        // increase/decrease weightings
        foreach (var agent in AgentSharedMemory.Instance.AttackingAgents)
        {
            if (agent.m_Team != m_Team)
            {
                if (GameManager.Instance.Field.GetTeamZone(m_Team).IsPosInField(agent.GetPosition()))
                {
                    m_DefendFlagsWeight += 2.0f;
                    m_DefendPrisonWeight += 0.5f;
                }
                else
                {
                    m_SeekFlagsWeight += 1.0f;
                    m_SeekPrisonWeight += 1.0f;
                }
            }
            else
            {
                m_DefendFlagsWeight += 0.25f;
                m_DefendPrisonWeight += 0.25f;
            }
        }

        Team bestTeamAttack = GameManager.Instance.m_Teams[0];
        int highestImprisoned = 0;
        float seekFWeight = 0;
        foreach (var t in GameManager.Instance.m_Teams)
        {
            if (t == m_Team) continue;
            if (t.m_Flags.Count == 0) continue; // dont attack if no flags
            if (t.MembersInPrison > highestImprisoned)
            {
                bestTeamAttack = t;
                highestImprisoned = t.MembersInPrison;
                seekFWeight = highestImprisoned * 1.5f + t.m_Flags.Count;
            }
        }
        if (bestTeamAttack == m_Team) bestTeamAttack = GameManager.Instance.m_Teams[1];
        m_SeekFlagsWeight += seekFWeight;

        Dictionary<Team, int> prisonsHoldingTeammates = new();
        foreach (var a in m_Team.m_TeamMembers)
        {
            if (a.GetComponent<Character>().m_Controller.m_CurrentState == BehaviourState.EBS_IN_PRISON)
            {
                if (prisonsHoldingTeammates.ContainsKey(a.GetComponent<Character>().m_ImprisonedBy))
                    prisonsHoldingTeammates[a.GetComponent<Character>().m_ImprisonedBy] += 1;
                else 
                    prisonsHoldingTeammates[a.GetComponent<Character>().m_ImprisonedBy] = 1;
            }
        }
        if (prisonsHoldingTeammates.Count > 0)
        {
            Team bestRescueMission = prisonsHoldingTeammates.First().Key;
            int mostPrisoners = 0;
            foreach (var t in prisonsHoldingTeammates)
            {
                if (t.Value > mostPrisoners)
                {
                    bestRescueMission = t.Key;
                    mostPrisoners = t.Value;
                    if (bestRescueMission == bestTeamAttack) { m_SeekPrisonWeight += 2.0f; break; }
                }
            }
            m_SeekPrisonWeight += mostPrisoners * 2.0f + prisonsHoldingTeammates.Count;
            TeamToRescueFrom = bestRescueMission;
        }
        else m_SeekPrisonWeight = 0.0f;

        TeamToSeekFlags = bestTeamAttack;

        if (((Vector2)TeamToSeekFlags.m_Zone.m_FlagZone.transform.position - m_Position).sqrMagnitude <= 72.0f)
        {
            m_SeekFlagsWeight += 8.0f;
        }
        if (m_SeekPrisonWeight > 0.1f &&((Vector2)TeamToRescueFrom.m_Zone.m_Prison.transform.position - m_Position).sqrMagnitude <= 49.0f 
            && prisonsHoldingTeammates.ContainsKey(TeamToRescueFrom))
        {
            m_SeekPrisonWeight += 8.0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Character>(out var character))
        {
            if (character.m_Team == m_Team) return;
            if (character.m_Controller.m_CurrentState == BehaviourState.EBS_RETURNING_FROM_PRISON) return;

            if (m_Team.m_Zone.ZoneBoundary.Contains((Vector3)m_Position))
            {
                if (m_Team.m_Zone.ZoneBoundary.Contains((Vector2)character.GetPosition()))
                {
                    m_TargetCharacter = character;
                    m_CurrentState = BehaviourState.EBS_CHASE_ENEMY;
                }
            }
            else if (character.m_Team.m_Zone.ZoneBoundary.Contains((Vector3)m_Position))
            {
                if (character.m_Team.m_Zone.ZoneBoundary.Contains(character.GetPosition()))
                {
                    m_FleeCharacter = character;
                    m_FleeForceWeighting = 2.0f;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Character>(out var character))
        {
            if (character == m_FleeCharacter)
            {
                m_FleeForceWeighting = 0.0f;
                m_FleeCharacter = null;
            }
            else if (character == m_TargetCharacter)
            {
                m_TargetCharacter = null;
                DecideCurrentState();
            }
        }
    }

    // choose which state based on the rules - mix of weighting and random element
    public void DecideCurrentState()
    {
        UpdateDecisionWeightings();

        if (m_CurrentState == BehaviourState.EBS_RETURNING_FROM_PRISON || m_CurrentState == BehaviourState.EBS_RETURNING_WITH_FLAG || 
            m_CurrentState == BehaviourState.EBS_IN_PRISON)
            return;

        if (GetComponent<Character>().m_HoldingFlag) { m_CurrentState = BehaviourState.EBS_RETURNING_WITH_FLAG; return; }

        List<DecisionWeightings> valueRanges = new()
        {
            new(0.0f, m_SeekPrisonWeight, BehaviourState.EBS_SEEK_PRISON),
            new(m_SeekPrisonWeight, m_SeekPrisonWeight + m_SeekFlagsWeight, BehaviourState.EBS_SEEK_FLAGS),
            new(m_SeekPrisonWeight + m_SeekFlagsWeight, m_SeekFlagsWeight + m_DefendPrisonWeight, BehaviourState.EBS_DEFEND_PRISON),
            new(m_SeekFlagsWeight + m_DefendPrisonWeight, m_DefendPrisonWeight + m_DefendFlagsWeight, BehaviourState.EBS_DEFEND_FLAGS),
            new(m_DefendPrisonWeight + m_DefendFlagsWeight, m_DefendFlagsWeight + m_ChaseEnemyWeight, BehaviourState.EBS_CHASE_ENEMY)
        };

        float totalWeightingValues = valueRanges.Last().maxVal;

        float chosenValue = UnityEngine.Random.Range(0.0f, totalWeightingValues);

        for (int i = valueRanges.Count - 1; i >= 0; i--)
        {
            if (chosenValue >= valueRanges[i].minVal && chosenValue <= valueRanges[i].maxVal)
            {
                m_CurrentState = valueRanges[i].state;
                if (m_CurrentState == BehaviourState.EBS_DEFEND_FLAGS)
                {
                    Debug.Log(gameObject.name + "Defending flags " + chosenValue);
                }
                return;
            }
        }
        m_CurrentState = BehaviourState.EBS_SEEK_FLAGS;

    }

    #region States (FSM)
    private void SeekPrison()
    {
        m_TargetPos = TeamToRescueFrom.m_Zone.m_Prison.transform.position;
        m_SeekForceWeighting = 1.0f;

        if ((m_TargetPos - m_Position).sqrMagnitude <= 4.0f)
        {
            m_CurrentState = BehaviourState.EBS_RETURNING_FROM_PRISON;
            AgentSharedMemory.Instance.RescueTeamMember(m_Team, TeamToRescueFrom);
        }
    }

    private void SeekFlags()
    {
        m_TargetPos = TeamToSeekFlags.m_Zone.m_FlagZone.transform.position;
        m_SeekForceWeighting = 1.0f;
    }

    private void DefendPrison()
    {
        m_TargetPos = m_Team.m_Zone.m_Prison.transform.position;
    }

    private void DefendFlags()
    {
        m_TargetPos = m_Team.m_Zone.m_FlagZone.transform.position;
    }

    private void ChaseEnemy()
    {
        m_TargetPos = m_TargetCharacter.GetPosition();
        m_SeekForceWeighting = 1.0f;

        if (!m_Team.m_Zone.ZoneBoundary.Contains((Vector3)m_Position))
        {
            DecideCurrentState();
        }
    }

    private void InPrison()
    {
        m_SeekForceWeighting = 0.0f;
        m_FleeForceWeighting = 0.0f;
        m_ArriveForceWeighting = 0.0f;
    }

    private void ReturnFlag()
    {
        m_TargetPos = m_Team.m_Zone.m_FlagZone.transform.position;
    }
    private void ReturnPrison()
    {
        m_TargetPos = m_Team.m_Zone.transform.position;
        if ((m_TargetPos - m_Position).sqrMagnitude <= 9.0f) m_CurrentState = BehaviourState.EBS_NEUTRAL;
    }

    #endregion
}
