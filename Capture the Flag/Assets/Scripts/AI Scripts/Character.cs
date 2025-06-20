using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterController m_Controller;
    public Team m_Team;
    public Team m_ImprisonedBy = null;

    public Flag m_HoldingFlag = null;

    public CharacterController AICont;
    public CharacterController PlayerContr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Controller.m_Team = m_Team;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x > 21 || transform.position.x < -21
            || transform.position.y > 11 || transform.position.y < -11)
            m_Controller.m_Position = Vector2.zero;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    // go to prison
    // in prison state
    // position is now in prison
    // velocity is zero
    public void GoToPrison(Team _ImprisonedBy)
    {
        if (m_HoldingFlag) DropFlag();

        m_ImprisonedBy = _ImprisonedBy;
        m_Controller.m_CurrentState = BehaviourState.EBS_IN_PRISON;
        m_Controller.m_Position = (Vector2)m_ImprisonedBy.m_Zone.m_Prison.transform.position + new Vector2(Random.Range(0.15f, 0.35f), Random.Range(0.15f, 0.35f));
        m_ImprisonedBy.m_Zone.m_Prison.m_ContainedCharacters.Add(this);
        m_Team.MembersInPrison++;
        m_Controller.m_Velocity = Vector2.zero;
    }

    // hurrah! 
    public void EscapePrison()
    {
        m_Controller.EscapePrison();
    }

    // you dropped the flag
    // and been caught
    // look what youve done
    public void DropFlag()
    {
        m_HoldingFlag.FlagDropped();
        m_HoldingFlag = null;
    }
}
