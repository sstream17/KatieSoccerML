using UnityEngine;

public class AIBall : MonoBehaviour
{
    public bool Hit = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (!Hit)
        {
            Collider collider = collision.collider;
            if (collider.tag.Contains("Piece"))
            {
                KatieSoccerAgent katieSoccerAgent = collider.gameObject.GetComponentInParent<KatieSoccerAgent>();
                if (katieSoccerAgent != null)
                {
                    Hit = true;
                    StartCoroutine(katieSoccerAgent.ComputeDistanceScore());
                }
            }
        }
    }
}
