using UnityEngine;

public class AgentSharedMemory : MonoBehaviour
{
    private static AgentSharedMemory m_Instance;

    // get instance, game manager is singleton
    public static AgentSharedMemory Instance
    {
        get
        {
            if (m_Instance != null)
            {
                return m_Instance;
            }

            var allInstances = FindObjectsByType<AgentSharedMemory>(FindObjectsSortMode.None);
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
            return m_Instance = new GameObject("Game Manager").AddComponent<AgentSharedMemory>();
        }
    }





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
