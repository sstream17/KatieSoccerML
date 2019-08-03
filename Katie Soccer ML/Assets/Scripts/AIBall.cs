using UnityEngine;

public class AIBall : MonoBehaviour
{
    public AIGameScript GameScript;
    public bool Hit = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!Hit)
        {
            Collider collider = collision.collider;
            if (collider.tag.Contains("Piece"))
            {
                Hit = true;
                GameScript.StartScoreForDistance();
            }
        }
    }
}
