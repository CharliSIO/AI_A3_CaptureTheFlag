using UnityEngine;

// BASE class for character controller
// Character will have either AI_Controller OR PlayerController
// Defines rules of the game 
// Handles motion, etc
public class CharacterController : MonoBehaviour
{
    public enum BehaviourState
    {

    }

    public Vector2 m_Velocity;
    public Vector2 m_SteeringForce;
    public Vector2 m_Position;
    public Collider2D m_Collider;

    public BehaviourState m_CurrentState;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
