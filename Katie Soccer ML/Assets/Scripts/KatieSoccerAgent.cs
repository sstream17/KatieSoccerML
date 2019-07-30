using MLAgents;
using System.Linq;
using UnityEngine;

public class KatieSoccerAgent : Agent
{
    public KatieSoccerAcademy academy;
    public KatieSoccerAgent opposingAgent;
    public GameObject[] TeamPieces;

    /// <summary>
    /// The goal to push the block to.
    /// </summary>
    public GameObject goal;

    /// <summary>
    /// The block to be pushed to the goal.
    /// </summary>
    public GameObject ball;

    public AIGoal goalDetect;

    private Rigidbody[] teamRBs;
    public RayPerception rayPerception;
    public bool AllowShot = false;

    private GameObject[] allPieces;
    private float goalReward = 5f;
    private float minStrength = 0.9f;
    private float maxStrength = 5f;
    private float speed = 200f;
    public float OffsetX = 0f;
    public float OffsetY = 0f;
    private float MinX = -4.25f;
    private float MaxX = 4.25f;
    private float MinY = -3.9f;
    private float MaxY = 2.1f;
    private float raycastStep = 0.5f;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
    }

    private void Start()
    {
        teamRBs = new Rigidbody[TeamPieces.Length];
        allPieces = new GameObject[TeamPieces.Length + 1];
        int i = 0;
        foreach (GameObject piece in TeamPieces)
        {
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            teamRBs[i] = rb;
            allPieces[i] = piece;
            i++;
        }
        allPieces[i] = ball;
    }

    void Update()
    {
        if (AllowShot)
        {
            AllowShot = false;
            RequestDecision();
        }

        var distanceToGoal = (ball.transform.position - goal.transform.position).magnitude;
        var denominator = (distanceToGoal * goalReward) + 1;
        AddReward(1 / denominator);
    }

    public override void CollectObservations()
    {
        var rayDistance = 12f;
        var angles = from angle in Enumerable.Range(0, 360)
                     where angle % 6 == 0
                     select angle;

        float[] rayAngles = new float[angles.Count()];
        int i = 0;
        foreach (int angle in angles)
        {
            rayAngles[i] = angle;
            i++;
        }

        var detectableObjects = new[] { "Ball", "TeamOneGoal", "TeamTwoGoal", "Wall", "TeamOnePiece", "TeamTwoPiece" };
        float startingPosition = -transform.position.z;
        for (float j = startingPosition; j < rayDistance; j += raycastStep)
        {
            AddVectorObs(rayPerception.Perceive(rayDistance, rayAngles, detectableObjects, 0f, j));
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        for (int i = 0; i < vectorAction.Length; i++)
        {
            vectorAction[i] = Mathf.Clamp(vectorAction[i], -1f, 1f);
        }

        float magnitude = ScaleAction(vectorAction[0], minStrength, maxStrength);
        float direction = ScaleAction(vectorAction[1], 0f, 2 * Mathf.PI);
        int selectedPiece = Mathf.FloorToInt(ScaleAction(vectorAction[2], 0f, TeamPieces.Length - 0.01f));

        Vector3 targetVector = GetTargetVector(magnitude, direction);
        teamRBs[selectedPiece].AddForce(targetVector * speed);

        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-1f / academy.TimePenalty);
    }

    private Vector3 GetTargetVector(float magnitude, float direction)
    {
        float x = magnitude * Mathf.Cos(direction);
        float y = magnitude * Mathf.Sin(direction);

        return new Vector3(x, y, 0f);
    }

    public Vector3 GetRandomSpawnPos(Vector3 currentPosition)
    {
        float randomPositionX = OffsetX + Random.Range(
            MinX * academy.spawnAreaMarginMultiplier,
            MaxX * academy.spawnAreaMarginMultiplier);

        float randomPositionY = OffsetY + Random.Range(
            MinY * academy.spawnAreaMarginMultiplier,
            MaxY * academy.spawnAreaMarginMultiplier);

        Vector3 randomSpawnPos = new Vector3(randomPositionX, randomPositionY, currentPosition.z);
        return randomSpawnPos;
    }

    /// <summary>
    /// Called when the agent moves the block into the goal.
    /// </summary>
    public void GoalScored()
    {
        // We use a reward of 5.
        AddReward(goalReward);

        // By marking an agent as done AgentReset() will be called automatically.
        Done();
        opposingAgent.Done();
    }

    public void OpponentScored()
    {
        AddReward(-goalReward);
    }

    /// <summary>
    /// Resets the block position and velocities.
    /// </summary>
    void ResetBall()
    {
        // Get a random position for the block.
        ball.transform.position = GetRandomSpawnPos(ball.transform.position);
    }


    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be 
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
	public override void AgentReset()
    {
        ResetBall();

        foreach (GameObject piece in TeamPieces)
        {
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            piece.transform.position = GetRandomSpawnPos(piece.transform.position);
            PieceMovement pieceMovement = piece.gameObject.GetComponent<PieceMovement>();
            pieceMovement.SetStartingPositions();
        }
    }
}
