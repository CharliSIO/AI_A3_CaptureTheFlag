using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Game manager
// Holds game data
// Objects can ask game manager to do things if it manages things they should not directly
public class GameManager : SingletonPersistent<GameManager>
{
    public List<Team> m_Teams = new();
    public List<Color> m_TeamColourList = new() { Color.red, Color.blue, Color.green, Color.yellow };
    public CinemachineCamera PlayerFollowCam;
    
    private Field m_Field;
    public Field Field { get => m_Field; set => m_Field = value; }
    
    [SerializeField] private int m_TeamCount = 2; // default 2 teams
    public int TeamCount {get => m_TeamCount;}
    public int TeamSize = 4; // encapsulate later
    public int FlagCount = 4;

    [SerializeField] private GameObject CharacterPrefab;
    public GameObject FlagPrefab;

    public Team WinningTeam;
    public UnityEvent GameHasWinner;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // wait until end of frame to change player so all calculations are done first
    // dont mess up any references
    public IEnumerator ChangePlayerAtEndOfFrame(Character _oldPlayer, int _newPlayer)
    {
        yield return new WaitForEndOfFrame();

        _oldPlayer.PlayerContr.enabled = false;
        _oldPlayer.AICont.enabled = true;
        _oldPlayer.m_Controller = _oldPlayer.AICont;

        Character newPlayer = _oldPlayer.m_Team.m_TeamMembers[_newPlayer - 1].GetComponent<Character>();
        newPlayer.PlayerContr.enabled = true;
        newPlayer.AICont.enabled = false;
        newPlayer.m_Controller = newPlayer.PlayerContr;
        PlayerFollowCam.Follow = newPlayer.transform;
    }

    #region Create Main Game Scene 

    // add the teams first
    public void LoadGameField()
    {
        for (int i = 0; i < m_TeamCount; i++)
        {
            m_Teams.Add(new());
            m_Teams[i].TeamColour = m_TeamColourList[i];
        }
        SceneManager.LoadScene("GameField"); 
    }

    private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
    {
        if (_scene.name == "GameField") // now in game!
        {
            CreateGame();
        }
    }

    // create the agents, the zones, make sure its in order n stuff
    public void CreateGame()
    {
        for (int i = 0; i < m_TeamCount; i++)
        {
            for (int j = 0; j < TeamSize; j++)
            {
                var newAgent = Instantiate(CharacterPrefab, new Vector3(
                (i == 1 || i == 3 ? 5f : -5f) + j, (m_TeamCount > 2 ? (i == 2 || i == 3 ? -2.5f : 2.5f) : 0.0f) + j, -1.0f), 
                Quaternion.identity);
                Color teamColour = m_Teams[i].TeamColour;
                newAgent.name = "Team " + i + " Member " + j.ToString();

                newAgent.GetComponentInChildren<SpriteRenderer>().color = new(teamColour.r * 1.5f, teamColour.g * 1.5f, teamColour.b * 1.5f, 1.0f);
                newAgent.GetComponent<Character>().m_Team = m_Teams[i];
                newAgent.GetComponent<AgentController>().RandomiserNumber = i + j;
                m_Teams[i].m_TeamMembers.Add(newAgent);

                if (i == 0 && j == 0)
                {
                    newAgent.GetComponent<AgentController>().enabled = false;
                    newAgent.GetComponent<Character>().m_Controller = newAgent.GetComponent<PlayerController>();
                    PlayerFollowCam.Follow = newAgent.transform;
                }
                else
                {
                    newAgent.GetComponent<PlayerController>().enabled = false;
                    newAgent.GetComponent<Character>().m_Controller = newAgent.GetComponent<AgentController>();
                }
                newAgent.GetComponent<CharacterController>().m_Team = m_Teams[i];
            }
            m_Teams[i].m_Zone.m_FlagZone.OwningTeam = m_Teams[i];
            m_Teams[i].m_Zone.m_Prison.OwningTeam = m_Teams[i];
            m_Teams[i].m_Zone.m_FlagZone.CreateFlags();
        }
    }

    public void CheckIfGameWon()
    {
        foreach (var team in m_Teams)
        {
            if (team.m_Flags.Count == FlagCount * TeamCount)
            {
                WinningTeam = team;
                GameHasWinner.Invoke();
                break;
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public static void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    #endregion

    #region Menu Functions
    public void SetTeamCountFour() { m_TeamCount = 4; }
    public void SetTeamCountTwo() { m_TeamCount = 2; }
    #endregion
}

// its a team!
// its just some vars
// perhaps it could have been encapsulated better
// oh well
public class Team
{
    public FieldZone m_Zone;

    public List<GameObject> m_TeamMembers = new();
    public List<Flag> m_Flags = new();

    public Color TeamColour;
    public int MembersInPrison = 0;
    public int MembersDefending = 0;
    public int MembersAttacking = 0;
}
