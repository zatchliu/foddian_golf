using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GolfBallPhysics : MonoBehaviour
{
    [Header("Physics Settings")]
    public float dragInAir = 0.2f;
    public float gravityScale = 2.5f;
    public float rollingResistance = 0.9f; // Deceleration factor when rolling

    public Rigidbody2D rb;

    public bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = dragInAir;
        rb.gravityScale = gravityScale;
    }

    void FixedUpdate()
    {
        if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * rollingResistance, rb.linearVelocity.y);

            if (Mathf.Abs(rb.linearVelocity.x) < 0.05f)
                rb.linearVelocity = Vector2.zero;
        }
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