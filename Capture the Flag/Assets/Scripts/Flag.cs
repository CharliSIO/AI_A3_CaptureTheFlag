using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Flag : MonoBehaviour
{
    public Team OwningTeam;
    public CircleCollider2D PickupRadius;
    public Character CarryingCharacter;

    private Color m_Colour = Color.white;

    private void Update()
    {
        if (CarryingCharacter)
        {
            transform.position = CarryingCharacter.transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "BodyCollider")
        {
            var character = collision.gameObject.GetComponentInParent<Character>();
            if (character && !character.m_HoldingFlag && !CarryingCharacter && character.m_Team != OwningTeam)
            {
                CarryingCharacter = character;
                character.m_HoldingFlag = this;
                character.m_Controller.m_CurrentState = BehaviourState.EBS_RETURNING_WITH_FLAG;
                OwningTeam.m_Zone.m_FlagZone.m_ContainedFlags.Remove(this);
                PickupRadius.enabled = false;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Flag>(out Flag component) && !CarryingCharacter)
        {
            Vector3 difference = transform.position - collision.transform.position;
            transform.position += difference.normalized;
        }
    }

    public void FlagChangedTeam()
    {
        PickupRadius.enabled = true;
        OwningTeam.m_Flags.Remove(this);
        OwningTeam = CarryingCharacter.m_Team;
        gameObject.GetComponent<SpriteRenderer>().color = OwningTeam.TeamColour;
        OwningTeam.m_Flags.Add(this);
        OwningTeam.m_Zone.m_FlagZone.m_ContainedFlags.Add(this);
        CarryingCharacter = null;
    }

}
