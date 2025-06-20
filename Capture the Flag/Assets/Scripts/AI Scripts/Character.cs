using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterController m_Controller;
    public Team m_Team;
    public Team m_ImprisonedBy = null;

    public Flag m_HoldingFlag = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    public void GoToPrison(Team _ImprisonedBy)
    {
        m_ImprisonedBy = _ImprisonedBy;
        m_Controller.m_CurrentState = BehaviourState.EBS_IN_PRISON;
        m_Controller.m_Position = (Vector2)m_ImprisonedBy.m_Zone.m_Prison.transform.position + new Vector2(Random.Range(0.15f, 0.35f), Random.Range(0.15f, 0.35f));
        m_ImprisonedBy.m_Zone.m_Prison.m_ContainedCharacters.Add(this);
        m_Team.MembersInPrison++;
        m_Controller.m_Velocity = Vector2.zero;
    }

    public void EscapePrison()
    {
        m_Controller.EscapePrison();
    }
}
