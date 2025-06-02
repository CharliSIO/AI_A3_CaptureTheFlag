using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterController m_Controller;
    public Team m_Team;
    public Team m_ImprisonedBy = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Controller = GetComponentInParent<CharacterController>();
        m_Controller.m_Team = m_Team;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
