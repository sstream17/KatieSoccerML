using UnityEngine;

public class AIBallOneTeam : MonoBehaviour
{
    public AIGameScriptOneTeam GameScript;
    private PieceMovement pieceMovement;
    public bool Hit = false;

    private bool wasMoving = false;

    private void Start()
    {
        pieceMovement = GetComponent<PieceMovement>();
    }

    private void Update()
    {
        if (pieceMovement.IsMoving && !wasMoving)
        {
            Hit = true;
        }

        if (pieceMovement.IsMoving)
        {
            wasMoving = true;
        }
        else
        {
            wasMoving = false;
        }
    }

    private void FixedUpdate()
    {
        if (Hit)
        {
            GameScript.StartScoreForDistance();
        }
    }
}
