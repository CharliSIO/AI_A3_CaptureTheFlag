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
    void Update()
    {
        
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
        // check shared memory to see what other team members are doing

        // increase/decrease weightings
    }
    void DecideCurrentState()
    {
        
    }
}
