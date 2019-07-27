﻿using MLAgents;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PieceAgent : Agent
{
    public PieceAcademy academy;

    /// <summary>
    /// The goal to push the block to.
    /// </summary>
    public GameObject goal;

    /// <summary>
    /// The block to be pushed to the goal.
    /// </summary>
    public GameObject ball;

    public AIGoal goalDetect;

    public Rigidbody ballRB;
    public Rigidbody agentRB;
    public RayPerception rayPerception;

    private GameObject[] allPieces;
    private float minStrength;
    private float maxStrength;
    private float speed = 200f;
    private float MinX = -4.25f;
    private float MaxX = 4.25f;
    private float MinZ = -3.9f;
    private float MaxZ = 2.1f;
    private bool allowShot = true;
    private bool piecesMoving = false;
    private bool piecesWereMoving = false;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
    }

    private void Start()
    {
        allPieces = new GameObject[2];
        allPieces[0] = gameObject;
        allPieces[1] = ball;

        minStrength = Mathf.Sqrt(0.9f * 0.9f / 2);
        maxStrength = Mathf.Sqrt(5f * 5f / 2);
    }

    void Update()
    {
        if (allowShot)
        {
            allowShot = false;
            Debug.Log("requesting decision");
            RequestDecision();
        }
        piecesMoving = !PiecesStoppedMoving(allPieces);
        if (piecesMoving)
        {
            piecesWereMoving = true;
        }

        if (!piecesMoving && piecesWereMoving)
        {
            piecesWereMoving = false;
            allowShot = true;
        }
    }

    private bool PiecesStoppedMoving(GameObject[] pieces)
    {
        foreach (GameObject piece in pieces)
        {
            PieceMovement pieceMovement = piece.GetComponent<PieceMovement>();
            if (pieceMovement.IsMoving)
            {
                return false;
            }
        }
        return true;
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

        var detectableObjects = new[] { "Ball", "Goal", "Wall", "Piece" };
        AddVectorObs(rayPerception.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        for (int i = 0; i < vectorAction.Length; i++)
        {
            vectorAction[i] = Mathf.Clamp(vectorAction[i], -1f, 1f);
        }

        float x = ScaleAction(vectorAction[0], minStrength, maxStrength);
        float y = ScaleAction(vectorAction[1], minStrength, maxStrength);
        Vector3 targetVector = new Vector3(x, y, 0f);
        agentRB.AddForce(targetVector * speed);

        //AddReward(-0.5f / agentParameters.maxStep);
    }

    public Vector3 GetRandomSpawnPos(Vector3 currentPosition)
    {
        float randomPositionX = Random.Range(
            MinX * academy.spawnAreaMarginMultiplier,
            MaxX * academy.spawnAreaMarginMultiplier);

        float randomPositionY = Random.Range(
            MinZ * academy.spawnAreaMarginMultiplier,
            MaxZ * academy.spawnAreaMarginMultiplier);

        Vector3 randomSpawnPos = new Vector3(randomPositionX, randomPositionY, currentPosition.z);
        return randomSpawnPos;
    }

    /// <summary>
    /// Called when the agent moves the block into the goal.
    /// </summary>
    public void IScoredAGoal()
    {
        // We use a reward of 5.
        AddReward(5f);

        // By marking an agent as done AgentReset() will be called automatically.
        Done();
    }

    /// <summary>
    /// Resets the block position and velocities.
    /// </summary>
    void ResetBlock()
    {
        // Get a random position for the block.
        ball.transform.position = GetRandomSpawnPos(ball.transform.position);

        // Reset block velocity back to zero.
        ballRB.velocity = Vector3.zero;

        // Reset block angularVelocity back to zero.
        ballRB.angularVelocity = Vector3.zero;
    }


    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be 
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
	public override void AgentReset()
    {
        allowShot = true;
        ResetBlock();
        transform.position = GetRandomSpawnPos(transform.position);
        PieceMovement pieceMovement = gameObject.GetComponent<PieceMovement>();
        pieceMovement.SetStartingPositions();
        agentRB.velocity = Vector3.zero;
    }
}
