using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class GolfBallPhysics : MonoBehaviour
{
    [Header("Physics Settings")]
    public float dragInAir = 0.2f;
    public float gravityScale = 2.5f;
    public float rollingResistance = 0.9f; // Deceleration factor when rolling

    public Rigidbody2D rb;

    public bool isGrounded;
    private Vector2 lastTeePosition;

    [Header("Reset Time")]
    public float ResetDelay = 2f;  // seconds to “sit in misery” 

    [HideInInspector] public bool InSand = false;
    [Tooltip("Bunker Penalty")]
    public float sandPenaltyFactor = 0.5f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = dragInAir;
        rb.gravityScale = gravityScale;
    }

    void Start()
    {
        // initialize your tee position
        lastTeePosition = transform.position;
    }

    void FixedUpdate()
    {
        if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * rollingResistance, rb.linearVelocity.y);

            if (Mathf.Abs(rb.linearVelocity.x) < 0.05f)
                rb.linearVelocity = Vector2.zero;
        }
        Debug.DrawRay(transform.position, GetComponent<Rigidbody2D>().linearVelocity, Color.cyan, 0.1f);

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    // call this whenever the player takes a shot
    public void RecordTeePosition()
    {
        lastTeePosition = transform.position;
    }

    // trigger handler for water
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water")) {
            StartCoroutine(HandleReset("Water Hazard"));
        }
       
        if (other.CompareTag("Sand")) {
            Debug.Log("In sand");
            InSand = true;
        }
            
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("OutOfBounds")) {
            StartCoroutine(HandleReset("Out of range!"));
        }

        if (other.CompareTag("Sand")) {
            InSand = false;
        } 
            
    }

    private IEnumerator HandleReset(string reasom)
    {
        // 1) Disable the club (and any other scripts you want frozen)
        var club = FindObjectOfType<ClubImpact2D>();
        var player = FindObjectOfType<PlayerMoveToBall>();
        if (club != null) club.enabled = false;
        if (player != null) player.enabled = false;


        // 2) (Optional) play splash VFX/audio here

        // 3) Wait in misery
        yield return new WaitForSeconds(ResetDelay);

        // 4) Stop all ball motion
        var rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 5) Move back to last tee
        transform.position = lastTeePosition;

        // 6) Re-enable the club
        if (club != null) club.enabled = true;
        if (player != null) player.enabled = true;
    }


}




/* using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallFlight2D : MonoBehaviour
{
    [Header("Physics Settings")]
    public float dragInAir = 0.2f;
    public float gravityScale = 2.5f;
    public float rollingResistance = 0.9f; // Deceleration factor when rolling

    [Header("Aerodynamics")]
    public float dragCoef   = 0.25f;   // Cd
    public float liftCoef0  = 0.00025f; // base constant for Magnus (empirical)
    public float radius     = 0.02135f; // m (golf ball)
    float spinRadPerSec;


    public Rigidbody2D rb;

    public bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = dragInAir;
        rb.gravityScale = gravityScale;
    }

    public void SetSpin(float spinRad) {
        spinRadPerSec = spinRad;
    } 
    
    void FixedUpdate()
    {
        if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * rollingResistance, rb.linearVelocity.y);

            if (Mathf.Abs(rb.linearVelocity.x) < 0.05f)
                rb.linearVelocity = Vector2.zero;
        }

        Vector2 v = rb.linearVelocity;
        float   s = v.magnitude;
        if (s < 0.01f) return;

        // --- Drag  Fd = -½ ρ Cd A v² v̂ (ρA merged into constant)
        Vector2 fDrag = -dragCoef * s * v;

        // --- Magnus Lift  Fl ≈ Cl ρ A v²  (Cl proportional to spin / v)
        // In 2‑D we use perp(velocity) to point lift upward relative to flight path
        float   cl = liftCoef0 * spinRadPerSec;      // simple linear spin→Cl
        Vector2 vPerp = new Vector2(-v.y, v.x).normalized; // 90° to velocity
        Vector2 fLift = cl * s * s * vPerp;          // magnitude ∝ v²

        rb.AddForce(fDrag + fLift, ForceMode2D.Force);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
 */