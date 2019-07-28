using UnityEngine;

public class AIGoal : MonoBehaviour
{
    public KatieSoccerAgent agent;

    void OnTriggerEnter(Collider collider)
    {
        // Touched goal.
        if (collider.gameObject.CompareTag("Ball"))
        {
            agent.IScoredAGoal();
        }
    }
}
