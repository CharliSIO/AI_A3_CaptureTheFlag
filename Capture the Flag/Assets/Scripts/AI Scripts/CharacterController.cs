using UnityEngine;

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

    public Vector2 m_Velocity;
    public Vector2 m_SteeringForce;
    public Vector2 m_Position;
    public CircleCollider2D m_DetectionRadius;

    public float m_Speed = 2.0f;
    public float m_MaxSpeed = 3.0f;
    public float m_MaxSteeringForce = 2.0f;

    public BehaviourState m_CurrentState;
    public Team m_Team;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        m_Position = this.gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void MoveCharacter()
    {
        Vector2.ClampMagnitude(m_Velocity, m_MaxSpeed);
        m_Position += m_Speed * Time.deltaTime * m_Velocity;
        this.gameObject.transform.position = (Vector3)m_Position;
    }

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
}
