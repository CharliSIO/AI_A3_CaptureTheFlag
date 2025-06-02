using System.Collections.Generic;
using UnityEngine;

public class AgentSharedMemory : Singleton<AgentSharedMemory>
{
    public List<Character> AttackingAgents;
    public List<Character> DefendingAgents;
    public List<Character> ImprisonedAgents;
    public List<Character> ReturningAgents;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateAgentStates();
    }

    private void UpdateAgentStates()
    {
        AttackingAgents.Clear();
        DefendingAgents.Clear();
        ImprisonedAgents.Clear();
        ReturningAgents.Clear();

        for (int i = 0; i < GameManager.Instance.TeamCount; i++)
        {
            for (int j = 0; j < GameManager.Instance.TeamSize; j++)
            {
                Character currentAgent = GameManager.Instance.m_Teams[i].m_TeamMembers[j].GetComponent<Character>();
                BehaviourState state = currentAgent.m_Controller.m_CurrentState;
                if (state == BehaviourState.EBS_SEEK_PRISON || state == BehaviourState.EBS_SEEK_FLAGS || state == BehaviourState.EBS_RETURNING_WITH_FLAG)
                {
                    AttackingAgents.Add(currentAgent);
                }
                else if (state == BehaviourState.EBS_DEFEND_PRISON || state == BehaviourState.EBS_DEFEND_FLAGS)
                {
                    DefendingAgents.Add(currentAgent);
                }
                else if (state == BehaviourState.EBS_IN_PRISON)
                { ImprisonedAgents.Add(currentAgent); }
                else if (state == BehaviourState.EBS_RETURNING_FROM_PRISON)
                { ReturningAgents.Add(currentAgent); }
            }
        }
    }
}
