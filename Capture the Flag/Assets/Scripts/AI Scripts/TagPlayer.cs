using UnityEngine;

public class TagPlayer : MonoBehaviour
{
    // put on body object so it uses the smaller circle collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "BodyCollider") // body collide with OTHER body, smaller collider
        {
            var ownController = GetComponentInParent<Character>().m_Controller;
            var otherController = collision.gameObject.GetComponentInParent<CharacterController>();

            // dont check for tag if theyre returning or have a flag
            // no cheaters here
            if (otherController.m_CurrentState == BehaviourState.EBS_RETURNING_FROM_PRISON
                || ownController.m_CurrentState == BehaviourState.EBS_RETURNING_FROM_PRISON
                || otherController.m_CurrentState == BehaviourState.EBS_RETURNING_WITH_FLAG) 
                return;
            if (ownController.m_Team == otherController.m_Team) return;

            // only go to prison if youre in enemy territory!
            if (otherController.m_Team.m_Zone.ZoneBoundary.Contains(ownController.m_Position))
            {
                GetComponentInParent<Character>().GoToPrison(otherController.m_Team);
            }
        }
    }
}
