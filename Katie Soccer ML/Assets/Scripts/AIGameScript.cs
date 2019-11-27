using System;
using UnityEngine;

public class AIGameScript : MonoBehaviour
{
    public GameObject[] TeamOnePieces;
    public GameObject[] TeamTwoPieces;
    public GameObject Ball;
    public KatieSoccerAgent TeamOneAgent;
    public KatieSoccerAgent TeamTwoAgent;

    public enum Team { TeamOne = -1, TeamTwo = 1 };

    private GameObject[] allPieces;
    private Vector3[] startingPositions;
    private Team currentTurn;
    private bool piecesMoving = false;
    private bool piecesWereMoving = false;

    public void SetStartingPositions()
    {
        for (int i = 0; i < allPieces.Length; i++)
        {
            startingPositions[i] = allPieces[i].transform.position;
        }
    }

    void Awake()
    {
        int numberOfAllPieces = TeamOnePieces.Length + TeamTwoPieces.Length + 1;
        allPieces = new GameObject[numberOfAllPieces];

        int iterator = 0;
        foreach (GameObject piece in TeamOnePieces)
        {
            allPieces[iterator] = piece;
            iterator = iterator + 1;
        }

        foreach (GameObject piece in TeamTwoPieces)
        {
            allPieces[iterator] = piece;
            iterator = iterator + 1;
        }

        allPieces[iterator] = Ball;

        startingPositions = new Vector3[allPieces.Length];

        SetStartingPositions();
    }

    public void GetRandomTurn()
    {
        Array values = Enum.GetValues(typeof(Team));
        int randomIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0f, values.Length));
        currentTurn = (Team)values.GetValue(randomIndex);
        OnNextTurn();
    }

    // Start is called before the first frame update
    void Start()
    {
        GetRandomTurn();
    }

    // Update is called once per frame
    void Update()
    {
        piecesMoving = !PiecesStoppedMoving(allPieces);
        if (piecesMoving)
        {
            piecesWereMoving = true;
        }

        if (!piecesMoving && piecesWereMoving)
        {
            piecesWereMoving = false;
            ChangeTurn();
            StopAllPieces();
            OnNextTurn();
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

    private void ChangeTurn()
    {
        int nextTurn = (int)currentTurn * -1;
        currentTurn = (Team)nextTurn;
    }

    public void StopAllPieces()
    {
        foreach (GameObject piece in allPieces)
        {
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            rb.Sleep();
        }
    }

    public void IlluminatePieces(GameObject[] pieces)
    {
        foreach (GameObject piece in pieces)
        {

            Material material = piece.GetComponent<Renderer>().material;
            Color currentColor = material.color;
            material.SetColor("_Color", new Color(
                currentColor.r,
                currentColor.g,
                currentColor.b, 1f));
        }
    }

    public void DarkenPieces(GameObject[] pieces)
    {
        foreach (GameObject piece in pieces)
        {
            Material material = piece.GetComponent<Renderer>().material;
            Color currentColor = material.color;
            material.SetColor("_Color", new Color(
                currentColor.r,
                currentColor.g,
                currentColor.b, 0.75f));
        }
    }

    public void OnNextTurn()
    {
        if (currentTurn.Equals(Team.TeamOne))
        {
            TeamOneAgent.AllowShot = true;
        }
        else
        {
            TeamTwoAgent.AllowShot = true;
        }
    }

    public void StartScoreForDistance()
    {
        AIBall aiBall = Ball.GetComponent<AIBall>();
        aiBall.Hit = false;
        if (currentTurn.Equals(Team.TeamOne))
        {
            StartCoroutine(TeamOneAgent.ComputeDistanceScore());
        }
        else
        {
            StartCoroutine(TeamTwoAgent.ComputeDistanceScore());
        }
    }
}
