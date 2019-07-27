using UnityEngine;

public class AIGoal : MonoBehaviour
{
    public PieceAgent agent;

    void OnTriggerEnter(Collider collider)
    {
        // Touched goal.
        if (collider.gameObject.CompareTag("Ball"))
        {
            agent.IScoredAGoal();
        }
    }
}
