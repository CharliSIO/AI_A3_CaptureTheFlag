using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentController : CharacterController
{

    #region Weighting Variables
    [SerializeField] private float m_SeekForceWeighting = 1.0f;
    [SerializeField] private float m_FleeForceWeighting = 1.0f;
    [SerializeField] private float m_ArriveForceWeighting = 1.0f;

    [SerializeField] private float m_SeparationForceWeighting = 1.0f;
    [SerializeField] private float m_CohesionForceWeighting = 1.0f;
    [SerializeField] private float m_AlignmentForceWeighting = 1.0f;

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
    [SerializeField] private Character m_TargetCharacter;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    override protected void Start()
    {
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
                break;
            case BehaviourState.EBS_RETURNING_FROM_PRISON:
                ReturnPrison();
                break;
            case BehaviourState.EBS_RETURNING_WITH_FLAG:
                ReturnFlag();
                break;
            case BehaviourState.EBS_CHASE_ENEMY:
                break;
            default:
                break;
        }

        m_SteeringForce = Vector2.zero;
        m_SteeringForce += Seek(m_TargetPos);

        Vector2.ClampMagnitude(m_SteeringForce, m_MaxSteeringForce);
        m_Velocity += m_SteeringForce;
        MoveCharacter();
        //m_Velocity += Flee(m_FleeTargetPos);
    }

    // coroutine that calls the update states - so not every frame
    private IEnumerator UpdateStates(float _time)
    {
        while (true) // TODO: change this to be a while match in progress or something
        {
            UnityEngine.Random.InitState(RandomiserNumber + (int)(Time.time * Time.deltaTime));
            DecideCurrentState();

            yield return new WaitForSeconds(_time + UnityEngine.Random.Range(0.1f, 0.5f));
        }
    }

    #region Locomotion
    Vector2 CalculateSteeringForce(Vector2 _desiredVel, float _forceweight)
    {
       return (_desiredVel - m_Velocity) * _forceweight;
    }

    Vector2 Seek(Vector2 _targetPos)
    {
        return CalculateSteeringForce((_targetPos - m_Position).normalized * m_MaxSpeed, m_SeekForceWeighting);
    }

    Vector2 Flee(Vector2 _targetPos)
    {
        return CalculateSteeringForce((m_Position - _targetPos).normalized * m_MaxSpeed, m_FleeForceWeighting);
    }

    // TODO
    Vector2 Arrive()
    {
        return new();
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
        m_DefendPrisonWeight = UnityEngine.Random.Range(0.1f, 1.0f) + (m_CurrentState == BehaviourState.EBS_DEFEND_PRISON ? 1.0f : 0.0f);
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
                    m_DefendFlagsWeight += 5.0f;
                    m_DefendPrisonWeight += 5.0f;
                }
                else
                {
                    m_DefendFlagsWeight += 1.0f;
                    m_DefendPrisonWeight += 1.0f;
                }
            }
            else
            {
                m_DefendFlagsWeight += 0.5f;
                m_DefendPrisonWeight += 0.5f;
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
                prisonsHoldingTeammates[a.GetComponent<Character>().m_ImprisonedBy] += 1;
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
    }

    // choose which state based on the rules - mix of weighting and random element
    public void DecideCurrentState()
    {
        UpdateDecisionWeightings();

        if (m_CurrentState == BehaviourState.EBS_RETURNING_FROM_PRISON || m_CurrentState == BehaviourState.EBS_RETURNING_WITH_FLAG)
            return;

        List<DecisionWeightings> valueRanges = new()
        {
            new(0.0f, m_SeekPrisonWeight, BehaviourState.EBS_SEEK_PRISON),
            new(m_SeekPrisonWeight, m_SeekPrisonWeight + m_SeekFlagsWeight, BehaviourState.EBS_SEEK_FLAGS),
            new(m_SeekFlagsWeight, m_SeekFlagsWeight + m_DefendPrisonWeight, BehaviourState.EBS_DEFEND_PRISON),
            new(m_DefendPrisonWeight, m_DefendPrisonWeight + m_DefendFlagsWeight, BehaviourState.EBS_DEFEND_FLAGS),
            new(m_DefendFlagsWeight, m_DefendFlagsWeight + m_ChaseEnemyWeight, BehaviourState.EBS_CHASE_ENEMY)
        };

        float totalWeightingValues = valueRanges.Last().maxVal;

        float chosenValue = UnityEngine.Random.Range(0.0f, totalWeightingValues);

        for (int i = valueRanges.Count - 1; i >= 0; i--)
        {
            if (chosenValue >= valueRanges[i].minVal && chosenValue <= valueRanges[i].maxVal)
            {
                m_CurrentState = valueRanges[i].state;
                return;
            }
        }
        m_CurrentState = BehaviourState.EBS_SEEK_FLAGS;

    }

    #region States (FSM)
    private void SeekPrison()
    {
        m_TargetPos = TeamToRescueFrom.m_Zone.m_Prison.transform.position;

        if ((m_TargetPos - m_Position).sqrMagnitude <= 2.0f)
        {
            m_CurrentState = BehaviourState.EBS_RETURNING_FROM_PRISON;
            AgentSharedMemory.Instance.RescueTeamMember(m_Team, TeamToRescueFrom);
        }
    }

    private void SeekFlags()
    {
        m_TargetPos = TeamToSeekFlags.m_Zone.m_FlagZone.transform.position;
    }

    private void DefendPrison()
    {
        m_TargetPos = m_Team.m_Zone.m_Prison.transform.position;
    }

    private void DefendFlags()
    {
        //Vector3 vectorToPrison = (Vector2)m_Team.m_Zone.m_Prison.transform.position - m_Position;

        //if ((vectorToPrison).sqrMagnitude <= 25.0f)
        //{
        //    if (Mathf.Sin(2 * Time.time) >=0)
        //    {
        //        m_TargetPos = (Vector3)m_Position + Vector3.Cross(vectorToPrison.normalized, Vector3.forward).normalized;
        //    }
        //    else
        //    {
        //        m_TargetPos = (Vector3)m_Position + -Vector3.Cross(vectorToPrison.normalized, Vector3.forward).normalized;
        //    }
        //}
        //else { m_TargetPos = m_Team.m_Zone.m_Prison.transform.position; }

        m_TargetPos = m_Team.m_Zone.m_Prison.transform.position;
    }

    private void InPrison()
    {

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
