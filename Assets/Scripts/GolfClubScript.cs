/* using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GolfClubScript : MonoBehaviour
{
    [Header("Club Motion")]
    public Transform pivotPoint;
    public float radius = 1.0f;
    public float followStrength = 500f;
    public float damping = 50f;
    public float maxSpeed = 20f;

    [Header("Hit Tuning")]
    public float hitStrength      = 0.05f;
    public float minSwingSpeed    = 6f;   // speed at which angle = baseAngle
    public float maxSwingSpeed    = 20f;  // speed at which angle = maxAngle
    public float baseLaunchAngle  = 30f;  // degrees
    public float maxLaunchAngle   = 65f;  // degrees

    [Header("Hit Curve")]
    [Tooltip("0 = softest hit, 1 = hardest hit")]
    public AnimationCurve angleCurve = 
        new AnimationCurve(
            new Keyframe(0f, 30f),   // at power 0 → 30°
            new Keyframe(1f, 65f));  // at power 1 → 65°


    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float pivotZ = Camera.main.WorldToScreenPoint(pivotPoint.position).z;
        Vector3 ms = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pivotZ);
        Vector3 mw3 = Camera.main.ScreenToWorldPoint(ms);
        Vector2 pivot2 = pivotPoint.position;
        Vector2 dir = (Vector2)mw3 - pivot2;

        if (dir.magnitude > radius)
            dir = dir.normalized * radius;

        Vector2 targetPos = pivot2 + dir;

        Vector2 disp = targetPos - rb.position;
        Vector2 springForce = disp * followStrength;
        Vector2 dampingForce = -rb.linearVelocity * damping;
        rb.AddForce(springForce + dampingForce);

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

void OnCollisionEnter2D(Collision2D col) {
    if (!col.gameObject.CompareTag("Ball")) return;

    // 1) how hard?
    float clubSpeed = rb.GetPointVelocity(col.GetContact(0).point).magnitude;
    float rawPF     = Mathf.InverseLerp(minSwingSpeed, maxSwingSpeed, clubSpeed);
    float angleDeg  = angleCurve.Evaluate(rawPF);
    float angleRad  = angleDeg * Mathf.Deg2Rad;

    // 2) get contact normal & tangent
    ContactPoint2D cp    = col.GetContact(0);
    Vector2 normal       = cp.normal;                             // unit
    Vector2 tangent      = new Vector2(-normal.y, normal.x);       // along the club’s path

    // 3) rotate tangent toward the normal by angleRad:
    Vector2 launchDir = (tangent * Mathf.Cos(angleRad) +
                         normal  * Mathf.Sin(angleRad)).normalized;

    // 4) apply impulse
    float impulseMag = clubSpeed * hitStrength;
    col.rigidbody.AddForce(launchDir * impulseMag, ForceMode2D.Impulse);
    col.rigidbody.AddTorque(-launchDir.x * impulseMag * 0.05f, ForceMode2D.Impulse);

    // 5) tweak ball physics
    col.rigidbody.gravityScale  = 1.0f;
    col.rigidbody.linearDamping = 0.3f;

    Debug.Log(
        $"clubSpeed={clubSpeed:F1}, " +
        $"rawPF={rawPF:F2}, " +
        $"angle={angleDeg:F1}°"
        );
    }
} */


using UnityEngine;

/// Attach to the club‑head collider
public class ClubImpact2D : MonoBehaviour
{
    [Header("Club Motion")]
    public Transform pivotPoint;
    public float radius = 1.0f;
    public float followStrength = 500f;
    public float damping = 50f;
    public float maxSpeed = 20f;

    [Header("Hit Tuning")]
    public float hitStrength      = 0.05f;
    public float minSwingSpeed    = 6f;   // speed at which angle = baseAngle
    public float maxSwingSpeed    = 20f;  // speed at which angle = maxAngle
 
    public Rigidbody2D rb;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    } 

    void FixedUpdate()
    {
        float pivotZ = Camera.main.WorldToScreenPoint(pivotPoint.position).z;
        Vector3 ms = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pivotZ);
        Vector3 mw3 = Camera.main.ScreenToWorldPoint(ms);
        Vector2 pivot2 = pivotPoint.position;
        Vector2 dir = (Vector2)mw3 - pivot2;

        if (dir.magnitude > radius)
            dir = dir.normalized * radius;

        Vector2 targetPos = pivot2 + dir;

        Vector2 disp = targetPos - rb.position;
        Vector2 springForce = disp * followStrength;
        Vector2 dampingForce = -rb.linearVelocity * damping;
        rb.AddForce(springForce + dampingForce);

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    [Header("Club spec")]
    [Range(30f,45f)] public float staticLoftDeg = 38f;   // 8‑iron ≈37–39°, 9‑iron ≈40–44°
    [Range(1.25f,1.35f)] public float smashFactor = 1.30f;

    [Header("Ball prefab")]
    public GameObject ballPrefab;           // assign in inspector
    public Transform  ballSpawnPoint;       // usually on StanceCenter

    const float baseShaftLean = 10f;        // ° lost to forward‑lean on a stock shot
    const float leanPerAoA   = 0.4f;        // extra lean per degree downward
    const float launchLoss   = 2f;          // ball launches a hair under dynamic loft
    const float spinPerLoft  = 200f;        // rpm ≈ 200 × dynamic loft @ full swing

    [System.Serializable] public struct ShotType {
    public string name;
    public float aoaAdj;     // e.g. punch -2°, high +3°
    public float loftAdj;    // e.g. punch -5°, high +5°
    }
    public ShotType[] shotTypes;     // define in Inspector
    public int        currentShot;   // expose via UI



    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Ball")) return;

        // --- 1.  raw club‑head data at impact -------------------------------
        Rigidbody2D clubRb   = GetComponent<Rigidbody2D>();
        Vector2      contact = col.GetContact(0).point;
        Vector2      vClub   = clubRb.GetPointVelocity(contact);
        float        clubSpd = vClub.magnitude;                // m/s

        // attack‑angle <0 = descending (down on ball)
        float attackAngleDeg = Vector2.SignedAngle(Vector2.right, vClub);

        // --- 2.  derive dynamic loft & launch‑angle -------------------------
        ShotType st   = shotTypes[currentShot];
        float adjAoA  = attackAngleDeg + st.aoaAdj;
        float dynLoft = staticLoftDeg
                        - baseShaftLean
                        - Mathf.Clamp(-adjAoA * leanPerAoA, 0f, 15f)
                        + st.loftAdj;

        float launchDeg = dynLoft - launchLoss;                // e.g. 18–20°
        float launchRad = launchDeg * Mathf.Deg2Rad;

        // --- 3.  ball speed & spin -----------------------------------------
        float ballSpd  = clubSpd ;//* smashFactor;                // simple smash
        float spinRpm  = dynLoft * spinPerLoft                 // 8‑iron ~8000‑9000
                         * Mathf.InverseLerp(0f,50f, clubSpd); // scale for soft swings
        float spinRad  = spinRpm * Mathf.Deg2Rad / 60f;        // to rad/s

        // --- 4.  instantiate / reset ball ----------------------------------
        Rigidbody2D ball = col.rigidbody;                      // using collided ball
        // if you prefer always‑new balls: Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity).GetComponent<Rigidbody2D>();

        ball.linearVelocity    = new Vector2(Mathf.Cos(launchRad), Mathf.Sin(launchRad)) * ballSpd;
        ball.angularVelocity = spinRad * Mathf.Rad2Deg;       // for visual spin (deg/s)

        //BallFlight2D bf  = ball.GetComponent<BallFlight2D>();
        GolfBallPhysics bf  = ball.GetComponent<GolfBallPhysics>();

        //bf.SetSpin(spinRad);


        // OPTIONAL small backspin torque so the sprite spins correctly:
        // ball.AddTorque(-Mathf.Sign(vClub.x) * spinRad * 0.001f, ForceMode2D.Impulse);
    }
}
