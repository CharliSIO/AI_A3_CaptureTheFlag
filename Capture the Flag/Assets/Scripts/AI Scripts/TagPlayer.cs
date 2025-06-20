using UnityEngine;

public class TagPlayer : MonoBehaviour
{
    // put on body object so it uses the smaller circle collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "BodyCollider")
        {
            var ownController = GetComponentInParent<Character>().m_Controller;
            var otherController = collision.gameObject.GetComponentInParent<CharacterController>();

            if (otherController.m_CurrentState == BehaviourState.EBS_RETURNING_FROM_PRISON) return;
            if (ownController.m_Team == otherController.m_Team) return;

            if (otherController.m_Team.m_Zone.ZoneBoundary.Contains(ownController.m_Position))
            {
                GetComponentInParent<Character>().GoToPrison(otherController.m_Team);
            }
        }
    }
}
