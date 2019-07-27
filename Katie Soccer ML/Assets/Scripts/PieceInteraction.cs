using UnityEngine;

public class PieceInteraction : MonoBehaviour
{
    public Rigidbody rb;
    public float Speed = 200f;

    private bool isSelected = false;
    private float triggerOffset = 0.3f;
    private float speedClamp = 5f;
    private float speedAdjust = 3f;
    private Vector3 arrow;
    private bool launchable = false;
    private Vector3 targetVector;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetVector = mousePosition - transform.position;
            targetVector.z = 0f;
            arrow = new Vector3(targetVector.x, 0f, targetVector.y);
            if (targetVector.magnitude >= triggerOffset)
            {
                launchable = true;
            }
            else
            {
                launchable = false;
            }
        }
    }

    private void OnMouseDown()
    {
        if (this.enabled)
        {
            isSelected = true;
        }
    }

    private void OnMouseUp()
    {
        if (this.enabled)
        {
            isSelected = false;
            if (launchable)
            {
                launchable = false;
                rb.AddForce(Vector3.ClampMagnitude(targetVector * speedAdjust, speedClamp) * -Speed);
            }
        }
    }
}
