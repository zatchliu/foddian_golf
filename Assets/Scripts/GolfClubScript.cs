using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GolfClubScript : MonoBehaviour
{
    [Header("Club Motion")]
    public Transform pivotPoint;
    public float radius = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Hit Tuning")]
    public float hitStrength = 0.5f;

    Rigidbody2D rb;

    void Start()
    {
        
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 1) Get mouse in world‐space (at the camera’s near plane)
        Vector3 mouseWorld3 = Camera.main.ScreenToWorldPoint(
            new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                Camera.main.nearClipPlane
            )
        );

        // 2) Work entirely in Vector2 from here on out
        Vector2 pivot2     = pivotPoint.position;
        Vector2 mouseWorld = mouseWorld3;

        // 3) Clamp to your circle
        Vector2 direction = mouseWorld - pivot2;
        if (direction.magnitude > radius)
            direction = direction.normalized * radius;

        // 4) Compute targetPos as a Vector2—no more mixing types
        Vector2 targetPos = pivot2 + direction;

        // 5) MovePosition will calculate rb.velocity for you
        rb.MovePosition(targetPos);
    }


    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            // grab the first contact
            ContactPoint2D contact = col.GetContact(0);
            Vector2 point = contact.point;

            // get the club’s true velocity at that point
            Vector2 clubVel = rb.GetPointVelocity(point);

            // apply it as an impulse to the ball
            Rigidbody2D ballRb = col.rigidbody;
            ballRb.AddForceAtPosition(clubVel * hitStrength, point, ForceMode2D.Impulse);
        }
    }

}
