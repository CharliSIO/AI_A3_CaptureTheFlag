using System.Collections;
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
            newFlag.GetComponent<SpriteRenderer>().color = OwningTeam.TeamColour * new Vector4(1.5f, 1.5f, 1.5f, 1.0f);
            newFlag.PickupRadius = newFlag.GetComponent<CircleCollider2D>();

            m_ContainedFlags.Add(newFlag);
            OwningTeam.m_Flags.Add(newFlag);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent<Character>(out var character);
        if (character && character.m_HoldingFlag && character.m_Team == OwningTeam)
        {
            StartCoroutine(ReturnFlagAfterTime(character.m_Controller));
        }
    }

    private IEnumerator ReturnFlagAfterTime(CharacterController _c)
    {
        yield return new WaitForSeconds(0.5f);
        _c.FlagReturned();
    }
}
