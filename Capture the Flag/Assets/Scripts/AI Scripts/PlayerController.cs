using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : CharacterController
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    override protected void Start()
    {
        base.Start();
        m_SeekForceWeighting = 1.0f;
        StartCoroutine(UpdateStates(0.25f)); // update states only once every 2 seconds
    }

    private void Update()
    {
        if (m_CurrentState == BehaviourState.EBS_IN_PRISON)
        {
            InPrison();
            PlayerHUD.Instance.ShowPrisonText(true);
        }
        else PlayerHUD.Instance.ShowPrisonText(false);
        if (m_CurrentState == BehaviourState.EBS_RETURNING_FROM_PRISON)
        {
            // solve player being always stuck in this state
            if (m_Team.m_Zone.ZoneBoundary.Contains(m_Position)) m_CurrentState = BehaviourState.EBS_NEUTRAL;
        }

            Vector2 moveInput = new();
        if (Input.GetKey(KeyCode.W))
        {
            moveInput.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveInput.y -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveInput.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveInput.x += 1;
        }
        moveInput.Normalize();
        moveInput *= m_MaxSpeed * 1.5f;

        m_SteeringForce = Vector2.zero;
        m_SteeringForce += Seek(m_Position + moveInput);

        Vector2.ClampMagnitude(m_SteeringForce, m_MaxSteeringForce);
        m_Velocity += m_SteeringForce;

        MoveCharacter();

        // choose another character... you cheater
        if (Input.GetKeyDown(KeyCode.Alpha1))
            StartCoroutine(GameManager.Instance.ChangePlayerAtEndOfFrame(gameObject.GetComponent<Character>(), 1));
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            StartCoroutine(GameManager.Instance.ChangePlayerAtEndOfFrame(gameObject.GetComponent<Character>(), 2));
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            StartCoroutine(GameManager.Instance.ChangePlayerAtEndOfFrame(gameObject.GetComponent<Character>(), 3));
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            StartCoroutine(GameManager.Instance.ChangePlayerAtEndOfFrame(gameObject.GetComponent<Character>(), 4));
    }

    // move that bad boy... a little bit faster so the player feels better about their poor skills
    protected override void MoveCharacter()
    {
        Vector2.ClampMagnitude(m_Velocity, m_MaxSpeed + 0.15f);
        m_Position += m_Speed * 1.5f * Time.deltaTime * m_Velocity;
        gameObject.transform.position = (Vector3)m_Position;
    }

    // coroutine that calls the update states - so not every frame
    private IEnumerator UpdateStates(float _time)
    {
        while (true) // TODO: change this to be a while match in progress or something
        {
            DecideCurrentState();
            yield return new WaitForSeconds(_time);
        }
    }

    #region Locomotion
    Vector2 CalculateSteeringForce(Vector2 _desiredVel, float _forceweight)
    {
        Vector2 steerForce = _desiredVel - m_Velocity;
        return _forceweight * Time.deltaTime * Vector2.ClampMagnitude(steerForce, m_MaxSteeringForce);
    }

    Vector2 Seek(Vector2 _targetPos)
    {
        return CalculateSteeringForce((_targetPos - m_Position).normalized * m_MaxSpeed, m_SeekForceWeighting);
    }
    #endregion

    // work out what player is doing!
    void DecideCurrentState()
    {
        if (m_CurrentState == BehaviourState.EBS_IN_PRISON
            || m_CurrentState == BehaviourState.EBS_RETURNING_FROM_PRISON
            || m_CurrentState == BehaviourState.EBS_RETURNING_WITH_FLAG) return;

        float seekPChance = 0.0f;
        float seekFChance = 0.0f;
        float defendingChance = 0.0f;

        foreach (var team in GameManager.Instance.m_Teams)
        {
            if (team == m_Team)
            {
                // player is in own team zone, probably defending
                if (team.m_Zone.ZoneBoundary.Contains(m_Position)) defendingChance += 0.5f;

                Vector2 vecToOwnPrison = (Vector2)team.m_Zone.m_Prison.transform.position - m_Position;
                Vector2 vecToOwnFlags = (Vector2)team.m_Zone.m_FlagZone.transform.position - m_Position;

                float oDot = Vector2.Dot(vecToOwnPrison, m_Velocity);
                if (oDot > 0.3f) defendingChance += oDot; // player is moving towards prison.. probably defending

                oDot = Vector2.Dot(vecToOwnFlags, m_Velocity);
                if (oDot > 0.3f) defendingChance += oDot; // moving towards own flags! yep, defending

                continue; // dont do the next bit because thats for not your own zone
            }

            Vector2 vecToPrison = (Vector2)team.m_Zone.m_Prison.transform.position - m_Position;
            Vector2 vecToFlags = (Vector2)team.m_Zone.m_FlagZone.transform.position - m_Position;

            float dot = Vector2.Dot(vecToPrison, m_Velocity);
            if (dot > 0.3f) seekPChance += dot; // player is headed towards enemy prison, rescue mission?

            dot = Vector2.Dot(vecToFlags, m_Velocity);
            if (dot > 0.3f) seekFChance += dot; // headed towards enemy flags... someones a hero
        }

        if (seekPChance + seekFChance > defendingChance)
        {
            // attacking! attacking what?
            if (seekFChance >= seekPChance)
            {
                m_CurrentState = BehaviourState.EBS_SEEK_FLAGS;
            }
            else m_CurrentState = BehaviourState.EBS_SEEK_PRISON;
        }
        else
        {
            // defending! whichever one this is matters less 
            int decider = UnityEngine.Random.Range(0, 2);
            m_CurrentState = (decider == 0) ? BehaviourState.EBS_DEFEND_PRISON : BehaviourState.EBS_DEFEND_FLAGS;
        }

        // check if player is in a prison
        // if theyre in a prison make sure its not because they got locked up
        // if player was rescuing then rescue someone else! and go home
        foreach (var team in GameManager.Instance.m_Teams)
        {
            if (team != m_Team)
            {
                if (((Vector2)team.m_Zone.m_Prison.transform.position - m_Position).sqrMagnitude <= 4.0f)
                {
                    m_CurrentState = BehaviourState.EBS_RETURNING_FROM_PRISON;
                    AgentSharedMemory.Instance.RescueTeamMember(m_Team, team);
                }
            }
        }

    }
}
