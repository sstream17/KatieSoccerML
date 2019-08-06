using MLAgents;
using System.Collections;
using TMPro;
using UnityEngine;

public class KatieSoccerAgent : Agent
{
    public KatieSoccerAcademy academy;
    public KatieSoccerAgent opposingAgent;
    public GameObject[] TeamPieces;
    public GameObject[] OpposingPieces;
    public GameObject[] Walls;
    public TextMeshProUGUI Score;

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
    public bool AllowShot = false;

    private Collider[] walls;
    private int numberOfWalls = 12;

    private GameObject[] allPieces;
    private int numberOfPieces = 3;
    private float goalReward = 30f;
    private float minimumScoringDistance = 6f;
    private float minStrength = 0.9f;
    private float maxStrength = 5f;
    private float speed = 200f;
    public float OffsetX = 0f;
    public float OffsetY = 0f;
    private float MinX = -4.25f;
    private float MaxX = 4.25f;
    private float MinY = -3.9f;
    private float MaxY = 2.1f;

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

        walls = new Collider[numberOfWalls];

        i = 0;
        for (int j = 0; j < Walls.Length; j++)
        {
            Collider[] colliders = Walls[j].GetComponents<Collider>();
            for (int k = 0; k < colliders.Length; k++)
            {
                walls[i] = colliders[k];
                i++;
            }
        }
    }

    void Update()
    {
        if (AllowShot)
        {
            AllowShot = false;
            RequestDecision();
        }

        if (Score != null)
        {
            Score.text = GetCumulativeReward().ToString();
        }
    }

    public override void CollectObservations()
    {
        for (int i = 0; i < numberOfPieces; i++)
        {
            if (i == 0)
            {
                AddVectorObs(TeamPieces[i].transform.position);
            }
            else
            {
                AddVectorObs(Vector3.zero);
            }
        }

        for (int i = 0; i < numberOfPieces; i++)
        {
            AddVectorObs(Vector3.zero);
        }

        for (int i = 0; i < walls.Length; i++)
        {
            AddVectorObs(walls[i].transform.position);
        }

        AddVectorObs(ball.transform.position);
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
                if (distanceToGoal <= minimumScoringDistance)
                {
                    AddReward(1 / score);
                }
            }
            
            lastDistance = distanceToGoal;
            yield return new WaitForFixedUpdate();
        }
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
