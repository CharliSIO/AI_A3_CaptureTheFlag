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
}

public class CharacterController : MonoBehaviour
{

    public Vector2 m_Velocity;
    public Vector2 m_SteeringForce;
    public Vector2 m_Position;
    public Collider2D m_Collider;

    public float m_Speed = 0.0f;
    public float m_MaxSpeed = 10.0f;
    public float m_MaxSteeringForce = 10.0f;

    public BehaviourState m_CurrentState;
    public Team m_Team;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
