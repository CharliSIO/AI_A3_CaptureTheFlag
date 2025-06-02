using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentController : CharacterController
{

    #region Weighting Variables
    [SerializeField] private float m_SeekForceWeighting;
    [SerializeField] private float m_FleeForceWeighting;
    [SerializeField] private float m_ArriveForceWeighting;

    [SerializeField] private float m_SeparationForceWeighting;
    [SerializeField] private float m_CohesionForceWeighting;
    [SerializeField] private float m_AlignmentForceWeighting;

    [SerializeField] private float m_SeekPrisonWeight;
    [SerializeField] private float m_SeekFlagsWeight;

    [SerializeField] private float m_DefendPrisonWeight;
    [SerializeField] private float m_DefendFlagsWeight;

    [SerializeField] private float m_ChaseEnemyWeight; // might not be needed?
    #endregion

    #region Locomotion Variables
    [SerializeField] private Vector2 m_TargetPos;
    [SerializeField] private Character m_TargetCharacter;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DecideCurrentState();
    }

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
        return CalculateSteeringForce((m_Position - _targetPos) * m_MaxSpeed, m_SeekForceWeighting);
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

    void UpdateDecisionWeightings()
    {
        m_SeekPrisonWeight = 0.0f;
        m_SeekFlagsWeight = 0.0f;
        m_DefendPrisonWeight = 0.0f;
        m_DefendFlagsWeight = 0.0f;
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
                    m_DefendFlagsWeight += 2.0f;
                    m_DefendPrisonWeight += 2.0f;
                }
            }
            else
            {
                m_DefendFlagsWeight += 1.0f;
                m_DefendPrisonWeight += 1.0f;
            }
        }

        Team bestTeamAttack;
        int highestImprisoned = 0;
        float seekFWeight = 0;
        foreach (var t in GameManager.Instance.m_Teams)
        {
            if (t == m_Team) continue;
            if (t.MembersInPrison > highestImprisoned)
            {
                bestTeamAttack = t;
                highestImprisoned = t.MembersInPrison;
                seekFWeight  = highestImprisoned * 1.5f;
            }
        }
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
                }
            }
            m_SeekPrisonWeight += mostPrisoners * 2.0f + prisonsHoldingTeammates.Count;
        }
    }

    void DecideCurrentState()
    {
        UpdateDecisionWeightings();


    }
}
