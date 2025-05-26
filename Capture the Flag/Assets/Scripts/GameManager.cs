using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

// Game manager
// Holds game data
// Objects can ask game manager to do things if it manages things they should not directly
public class GameManager : MonoBehaviour
{
    private static GameManager m_Instance;

    // get instance, game manager is singleton
    public static GameManager Instance
    {
        get
        {
            if (m_Instance != null)
            {
                return m_Instance;
            }

            var allInstances = FindObjectsByType<GameManager>(FindObjectsSortMode.None);
            int iInstanceCount = allInstances.Length;

            if (iInstanceCount > 0)
            {
                if (iInstanceCount == 1)
                {
                    return allInstances[0];
                }

                // more than one game manager??? oh no
                for (int i = (iInstanceCount - 1); i > 0; i--) // iterate backwards for safety!!! length will change as items deleted
                {
                    Destroy(allInstances[i]);
                }
                return m_Instance = allInstances[0];
            }

            // no instance yet! make one
            return m_Instance = new GameObject("Game Manager").AddComponent<GameManager>();
        }
    }

    public List<Team> m_Teams = new();
    public List<Color> m_TeamColourList = new() { Color.red, Color.blue, Color.green, Color.yellow };
    
    private Field m_Field;
    public Field Field { get => m_Field; set => m_Field = value; }
    
    [SerializeField] private int m_TeamCount = 2; // default 2 teams
    public int TeamCount {get => m_TeamCount;}
    public int TeamSize; // encapsulate later

    public AgentSharedMemory AgentMemory;

    [SerializeField] private GameObject CharacterPrefab;

    private void Awake()
    {
        m_Instance = Instance; // make sure the instance is correct and not multiple by using the get!!
        DontDestroyOnLoad(this.gameObject); // persist through scenes
    }

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
            SceneManager.sceneLoaded -= OnSceneLoaded;
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
                newAgent.GetComponentInChildren<SpriteRenderer>().color = new(teamColour.r * 1.1f, teamColour.g * 1.25f, teamColour.b * 1.25f, teamColour.a * 2f);
                m_Teams[i].m_TeamMembers.Add(newAgent);
            }
        }

        AgentMemory = AgentSharedMemory.Instance;
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
}


public class Flag
{

}