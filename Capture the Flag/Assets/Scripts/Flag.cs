using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Flag : MonoBehaviour
{
    public Team OwningTeam;
    public CircleCollider2D PickupRadius;
    public Character CarryingCharacter;

    private Color m_Colour = Color.white;
    private Vector2 m_StartPos;

    private void Start()
    {
        m_StartPos = transform.position;
    }

    private void Update()
    {
        if (CarryingCharacter)
        {
            // move with character
            transform.position = new Vector3(CarryingCharacter.transform.position.x, CarryingCharacter.transform.position.y, transform.position.z);
        }
    }

    // ooh a collision
    // someone come to pick the flag up?
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "BodyCollider")
        {
            var character = collision.gameObject.GetComponentInParent<Character>();

            // pick up if character exists, isnt holding a flag, isnt already carrying this flag, and is of a different team
            if (character && !character.m_HoldingFlag && !CarryingCharacter && character.m_Team != OwningTeam)
            {
                CarryingCharacter = character;
                character.m_HoldingFlag = this;
                character.m_Controller.m_CurrentState = BehaviourState.EBS_RETURNING_WITH_FLAG; // character got a flag!
                OwningTeam.m_Zone.m_FlagZone.m_ContainedFlags.Remove(this);
                PickupRadius.enabled = false; // disable pickup radius just in case!
            }
        }
    }

    // flag changed team!!
    // disconnect from player carrying
    // and change team and colour!
    public void FlagChangedTeam()
    {
        PickupRadius.enabled = true;
        OwningTeam.m_Flags.Remove(this);
        OwningTeam = CarryingCharacter.m_Team;
        gameObject.GetComponent<SpriteRenderer>().color = OwningTeam.TeamColour;
        OwningTeam.m_Flags.Add(this);
        OwningTeam.m_Zone.m_FlagZone.m_ContainedFlags.Add(this);
        CarryingCharacter = null;
        GameManager.Instance.CheckIfGameWon();
    }

    // carrying player got tagged
    // go home, little flag
    // no change of team for you
    public void FlagDropped()
    {
        transform.position = m_StartPos;
        PickupRadius.enabled = true;
        OwningTeam.m_Zone.m_FlagZone.m_ContainedFlags.Add(this);
        CarryingCharacter = null;
    }

}
