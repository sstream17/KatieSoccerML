using UnityEngine;

public class AIBall : MonoBehaviour
{
    public bool Hit = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (!Hit)
        {
            Hit = true;
            Collider collider = collision.collider;
            if (collider.tag.Contains("Piece"))
            {
                KatieSoccerAgent katieSoccerAgent = collider.gameObject.GetComponentInParent<KatieSoccerAgent>();
                if (katieSoccerAgent != null)
                {
                    StartCoroutine(katieSoccerAgent.ComputeDistanceScore());
                }
            }
        }
    }
}
