using System.Collections.Generic;
using UnityEngine;

public class FlagZone : Zone
{
    public List<Flag> m_ContainedFlags = new();

    public void CreateFlags()
    {
        for (float i = GameManager.Instance.FlagCount; i > 0; i--)
        {
            Vector3 posVec = new(i / (GameManager.Instance.FlagCount - 1), i / (GameManager.Instance.FlagCount - 1), -2.0f);
            Flag newFlag = Instantiate(GameManager.Instance.FlagPrefab, gameObject.transform.position + posVec, Quaternion.identity).GetComponent<Flag>();
            newFlag.OwningTeam = OwningTeam;
            //newFlag.GetComponent<SpriteRenderer>().color = OwningTeam.TeamColour;

            m_ContainedFlags.Add(newFlag);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent<Character>(out var character);
        if (character && character.m_HoldingFlag && character.m_Team == OwningTeam)
        {
            character.m_Controller.FlagReturned();
        }
    }
}
