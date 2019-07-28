using UnityEngine;

public class AIGoal : MonoBehaviour
{
    public KatieSoccerAgent ScoringAgent;
    public KatieSoccerAgent DefendingAgent;

    void OnTriggerEnter(Collider collider)
    {
        // Touched goal.
        if (collider.gameObject.CompareTag("Ball"))
        {
            DefendingAgent.OpponentScored();
            ScoringAgent.GoalScored();
        }
    }
}
