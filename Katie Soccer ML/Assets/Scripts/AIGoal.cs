using UnityEngine;

public class AIGoal : MonoBehaviour
{
    public KatieSoccerAgent ScoringAgent;
    public KatieSoccerAgent DefendingAgent;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Ball"))
        {
            if (DefendingAgent != null)
            {
                DefendingAgent.OpponentScored();
            }
            
            if (ScoringAgent != null)
            {
                ScoringAgent.GoalScored();
            }
        }
    }
}
