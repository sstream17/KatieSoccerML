using MLAgents;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class KatieSoccerAgent : Agent
{
    public KatieSoccerAcademy academy;
    public KatieSoccerAgent opposingAgent;
    public GameObject[] TeamPieces;
    public GameObject[] OpposingPieces;

    /// <summary>
    /// The goal to push the block to.
    /// </summary>
    public GameObject goal;

    /// <summary>
    /// The block to be pushed to the goal.
    /// </summary>
    public GameObject ball;

    public AIGoal goalDetect;

    public RayPerception rayPerception;

    private Rigidbody[] teamRBs;
    public bool AllowShot = false;

    private GameObject[] allPieces;
    private int numberOfPieces = 3;
    private float rayDistance = 12f;
    private float[] rayAngles;
    private float goalReward = 100f;
    private float minStrength = 0.9f;
    private float maxStrength = 5f;
    private float speed = 200f;
    public float OffsetX = 0f;
    public float OffsetY = 0f;
    private float MinX = -4.25f;
    private float MaxX = 4.25f;
    private float MinY = -3.9f;
    private float MaxY = 2.1f;
    private float ballZ = 1.65f;
    private float pieceZ = 1.7f;
    private float fieldWidth = 11.95f;
    private float fieldHeight = 6.45f;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
    }

    private void Start()
    {
        teamRBs = new Rigidbody[TeamPieces.Length];
        allPieces = new GameObject[TeamPieces.Length + 1];
        int i;
        for (i = 0; i < TeamPieces.Length; i++)
        {
            GameObject piece = TeamPieces[i];
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            teamRBs[i] = rb;
            allPieces[i] = piece;
        }

        allPieces[i] = ball;

        var angles = from angle in Enumerable.Range(0, 360)
                     where angle % 6 == 0
                     select angle;

        rayAngles = new float[angles.Count()];
        i = 0;
        foreach (int angle in angles)
        {
            rayAngles[i] = angle;
            i++;
        }
    }

    void Update()
    {
        if (AllowShot)
        {
            AllowShot = false;
            RequestDecision();
        }
    }

    public override void CollectObservations()
    {
        var detectableObjects = new[] { "Ball", "TeamOneGoal", "TeamTwoGoal", "Wall", "TeamOnePiece", "TeamTwoPiece" };
        for (int i = 0; i < numberOfPieces; i++)
        {
            if (i < TeamPieces.Length)
            {
                AddVectorObs(rayPerception.Perceive(TeamPieces[i].transform, rayDistance, rayAngles, detectableObjects, 0f, 0f));
            }
            else
            {
                AddVectorObs(rayPerception.Perceive(transform, rayDistance, rayAngles, detectableObjects, 0f, 0f));
            }
        }

        for (int i = 0; i < numberOfPieces; i++)
        {
            if (i < OpposingPieces.Length)
            {
                AddVectorObs(rayPerception.Perceive(OpposingPieces[i].transform, rayDistance, rayAngles, detectableObjects, 0f, 0f));
            }
            else
            {
                AddVectorObs(rayPerception.Perceive(transform, rayDistance, rayAngles, detectableObjects, 0f, 0f));
            }
        }

        AddVectorObs(rayPerception.Perceive(ball.transform, rayDistance, rayAngles, detectableObjects, 0f, -0.1f));
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

    public IEnumerator ComputeDistanceScore()
    {
        PieceMovement ballMovement = ball.GetComponent<PieceMovement>();
        var lastDistance = (ball.transform.position - goal.transform.position).magnitude;
        while (ballMovement.IsMoving)
        {
            var distanceToGoal = (ball.transform.position - goal.transform.position).magnitude;
            if (distanceToGoal < lastDistance)
            {
                var score = (distanceToGoal * goalReward) + 1;
				AddReward(1 / score);
            }
            
            lastDistance = distanceToGoal;
            yield return new WaitForFixedUpdate();
        }
    }

    public Vector3 GetRandomSpawnPos(float currentPositionZ)
    {
        float randomPositionX = OffsetX + Random.Range(
            MinX * academy.spawnAreaMarginMultiplier,
            MaxX * academy.spawnAreaMarginMultiplier);

        float randomPositionY = OffsetY + Random.Range(
            MinY * academy.spawnAreaMarginMultiplier,
            MaxY * academy.spawnAreaMarginMultiplier);

        Vector3 randomSpawnPos = new Vector3(randomPositionX, randomPositionY, currentPositionZ);
        return randomSpawnPos;
    }

    /// <summary>
    /// Called when the agent moves the block into the goal.
    /// </summary>
    public void GoalScored()
    {
        AddReward(goalReward);

        // By marking an agent as done AgentReset() will be called automatically.
        Done();
        ////opposingAgent.Done();
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
        float offset = transform.position.z + ballZ;
        ball.transform.position = GetRandomSpawnPos(offset);
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
            float offset = transform.position.z + pieceZ;
            piece.transform.position = GetRandomSpawnPos(offset);
            PieceMovement pieceMovement = piece.gameObject.GetComponent<PieceMovement>();
            pieceMovement.SetStartingPositions();
        }
    }
}
