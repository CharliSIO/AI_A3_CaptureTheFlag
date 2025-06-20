using UnityEngine;
using UnityEngine.Events;

// BASE class for character controller
// Character will have either AI_Controller OR PlayerController
// Defines rules of the game 
// Handles motion, etc

public enum BehaviourState
{
    EBS_NEUTRAL,
    EBS_SEEK_PRISON,
    EBS_SEEK_FLAGS,
    EBS_DEFEND_PRISON,
    EBS_DEFEND_FLAGS,
    EBS_IN_PRISON,
    EBS_RETURNING_FROM_PRISON,
    EBS_RETURNING_WITH_FLAG,
    EBS_CHASE_ENEMY,
}

public class CharacterController : MonoBehaviour
{
    // general controller things
    public Vector2 m_Velocity;
    public Vector2 m_SteeringForce;
    public Vector2 m_Position;
    public CircleCollider2D m_DetectionRadius;

    public float m_Speed = 2.0f;
    public float m_MaxSpeed = 3.0f;
    public float m_MaxSteeringForce = 3.0f;

    public BehaviourState m_CurrentState;
    public Team m_Team;

    // locomotion weightings
    protected float m_SeekForceWeighting = 0.0f;
    protected float m_FleeForceWeighting = 0.0f;
    protected float m_ArriveForceWeighting = 1.0f;
    
    protected float m_SeparationForceWeighting = 1.0f;
    protected float m_CohesionForceWeighting = 1.0f;
    protected float m_AlignmentForceWeighting = 1.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        m_Position = gameObject.transform.position;
        GameManager.Instance.GameHasWinner.AddListener(EndGame);
    }

    protected void OnEnable()
    {
        m_Team = gameObject.GetComponent<Character>().m_Team;
    }

    // move the character
    protected virtual void MoveCharacter()
    {
        Vector2.ClampMagnitude(m_Velocity, m_MaxSpeed);
        m_Position += m_Speed * Time.deltaTime * m_Velocity;
        gameObject.transform.position = (Vector3)m_Position;
    }

    // flag changed team!!
    public void FlagReturned()
    {
        gameObject.GetComponent<Character>().m_HoldingFlag.FlagChangedTeam();
        gameObject.GetComponent<Character>().m_HoldingFlag = null;
        m_CurrentState = BehaviourState.EBS_NEUTRAL;
        if (TryGetComponent<AgentController>(out var cont))
        {
            cont.DecideCurrentState();
        }
    }

    // you made it out of prison!!
    // good job
    public void EscapePrison()
    {
        m_CurrentState = BehaviourState.EBS_RETURNING_FROM_PRISON;
        m_SeekForceWeighting = 1.0f;
        m_ArriveForceWeighting = 1.0f;
    }

    // youre in prison.. less good job
    protected void InPrison()
    {
        m_SeekForceWeighting = 0.0f;
        m_FleeForceWeighting = 0.0f;
        m_ArriveForceWeighting = 0.0f;
    }
    // ---------

    protected void EndGame()
    {
        this.enabled = false;
    }
}
