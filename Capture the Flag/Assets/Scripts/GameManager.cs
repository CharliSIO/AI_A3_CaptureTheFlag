using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

// Game manager
// Holds game data
// Objects can ask game manager to do things if it manages things they should not directly
public class GameManager : SingletonPersistent<GameManager>
{
    public List<Team> m_Teams = new();
    public List<Color> m_TeamColourList = new() { Color.red, Color.blue, Color.green, Color.yellow };
    
    private Field m_Field;
    public Field Field { get => m_Field; set => m_Field = value; }
    
    [SerializeField] private int m_TeamCount = 2; // default 2 teams
    public int TeamCount {get => m_TeamCount;}
    public int TeamSize = 4; // encapsulate later

    [SerializeField] private GameObject CharacterPrefab;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #region Create Main Game Scene 

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
        if (_scene.name == "GameField")
        {
            CreateGame();
        }
    }

    public void CreateGame()
    {
        for (int i = 0; i < m_TeamCount; i++)
        {
            for (int j = 0; j < TeamSize; j++)
            {
                var newAgent = Instantiate(CharacterPrefab);
                Color teamColour = m_Teams[i].TeamColour;
                newAgent.GetComponentInChildren<SpriteRenderer>().color = new(teamColour.r * 1.5f, teamColour.g * 1.5f, teamColour.b * 1.5f, teamColour.a * 2f);
                newAgent.transform.position = new Vector3(
                (i == 1 || i == 3 ? 5f : -5f) + j/2f, (m_TeamCount > 2 ? (i == 2 || i == 3 ? -2.5f : 2.5f) : 0.0f) + j / 2f, -1.0f);
                newAgent.GetComponent<Character>().m_Team = m_Teams[i];
                m_Teams[i].m_TeamMembers.Add(newAgent);
            }
        }
    }
    #endregion

    #region Menu Functions
    public void SetTeamCountFour() { m_TeamCount = 4; }
    public void SetTeamCountTwo() { m_TeamCount = 2; }
    #endregion
}

public class Team
{
    public List<GameObject> m_TeamMembers = new();
    public Color TeamColour;
    public int MembersInPrison = 0;
    public int MembersDefending = 0;
    public int MembersAttacking = 0;
}

public class Flag
{

}