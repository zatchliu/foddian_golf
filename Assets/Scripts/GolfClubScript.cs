using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GolfClubScript : MonoBehaviour
{
    [Header("Club Motion")]
    public Transform pivotPoint;
    public float radius = 1.0f;
    public Transform leftForearm; // Reference to the left forearm
    public Transform gripPoint; // Point where the forearm should connect to the club

    [Header("Hit Tuning")]
    public float hitStrength = 0.5f;

    Rigidbody2D rb;
    Rigidbody2D forearmRb;
    Vector2 forearmOffset; // Store the local offset from grip point to forearm

    void Start()
    {
        if (leftForearm != null && gripPoint != null)
        {
            forearmRb = leftForearm.GetComponent<Rigidbody2D>();
            // Calculate the initial offset between forearm and grip point
            forearmOffset = leftForearm.position - gripPoint.position;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        var direction = mousePosition - pivotPoint.position;
        if (direction.magnitude > radius)
        {
            direction.Normalize();
            direction *= radius;
            mousePosition = pivotPoint.position + direction;
        }

        GetComponent<Rigidbody2D>().MovePosition(mousePosition);
        /*
        // Update forearm position and rotation to follow the club
        if (leftForearm != null && gripPoint != null && forearmRb != null)
        {
            // Calculate the angle between the club and vertical
            float angle = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
            
            // Use MovePosition and MoveRotation to work with physics
            forearmRb.MovePosition(gripPoint.position);
            forearmRb.MoveRotation(angle);
            
            // Ensure the forearm's velocity matches the club's velocity
            forearmRb.linearVelocity = rb.linearVelocity;
            forearmRb.angularVelocity = rb.angularVelocity;
        }
        */
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            // grab the first contact
            ContactPoint2D contact = col.GetContact(0);
            Vector2 point = contact.point;

            // get the club's true velocity at that point
            Vector2 clubVel = rb.GetPointVelocity(point);

            // apply it as an impulse to the ball
            Rigidbody2D ballRb = col.rigidbody;
            ballRb.AddForceAtPosition(clubVel * hitStrength, point, ForceMode2D.Impulse);
        }
    }
}
