using UnityEngine;
using System.Collections;

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
 
    private Rigidbody2D rb;

    private bool _hasHit = false;

    [Header("Layers")]
    public int clubLayer = 8; // index of ClubLayer
    public int ballLayer = 9; // index of BallLayer

    [Header("References")]
    public Collider2D collider; 
    

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        _hasHit = true;
        collider.enabled = false; 

        //Physics2D.IgnoreLayerCollision(clubLayer, ballLayer, true);
        Debug.Log("[Awake] swing locked, collisions ignored");
    } 

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"[Update] Mouse clicked!  _hasHit = {_hasHit}");
        }

        // click to *unlock* the swing
        if (_hasHit && Input.GetMouseButtonDown(0))
        {
            _hasHit = false;
            // now allow actual collisions
            //Physics2D.IgnoreLayerCollision(clubLayer, ballLayer, false);
            collider.enabled = true;
            Debug.Log("[Update] swing unlocked, collisions enabled");
        }
    }

    void FixedUpdate()
    {
        if (_hasHit) return;

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
    [Range(30f,45f)] public float staticLoftDeg = 38f;
    [Range(1.25f,1.35f)] public float smashFactor = 1.30f;

    [Header("Ball prefab")]
    public GameObject ballPrefab;           
    public Transform  ballSpawnPoint;      

    const float baseShaftLean = 10f;       
    const float leanPerAoA   = 0.4f;       
    const float launchLoss   = 2f;         
    const float spinPerLoft  = 200f;       

    [System.Serializable] public struct ShotType {
    public string name;
    public float aoaAdj;   
    public float loftAdj;   
    }
    public ShotType[] shotTypes;     
    public int        currentShot;  

    [Header("Audio")]
    public AudioSource audioSource;    
    public AudioClip hitSound;          


    void OnCollisionEnter2D(Collision2D col)
    {
        if (_hasHit || !col.collider.CompareTag("Ball")) return;
        _hasHit = true;
        collider.enabled = true;

        Debug.Log("[OnCollisionEnter2D] Ball hit → locking swing");


        // --- 1.  raw club‑head data at impact 
        Rigidbody2D clubRb   = GetComponent<Rigidbody2D>();
        Vector2      contact = col.GetContact(0).point;
        Vector2      vClub   = clubRb.GetPointVelocity(contact);
        float        clubSpd = vClub.magnitude;               
        float dirSign = Mathf.Sign(vClub.x);



        float attackAngleDeg = Vector2.SignedAngle(Vector2.right, vClub);
        // --- 2.  derive dynamic loft & launch‑angle 
        ShotType st   = shotTypes[currentShot];
        float adjAoA  = attackAngleDeg + st.aoaAdj;
        float dynLoft = staticLoftDeg
                        - baseShaftLean
                        - Mathf.Clamp(-adjAoA * leanPerAoA, 0f, 15f)
                        + st.loftAdj;

        float launchDeg = dynLoft - launchLoss;          
        float launchRad = launchDeg * Mathf.Deg2Rad;

        // --- 3.  ball speed & spin 
        float ballSpd  = clubSpd * smashFactor;               
        float spinRpm  = dynLoft * spinPerLoft                 
                         * Mathf.InverseLerp(0f,50f, clubSpd); 
        float spinRad  = spinRpm * Mathf.Deg2Rad / 60f;       

        

        // --- 4.  instantiate / reset ball 
        Rigidbody2D ball = col.rigidbody; 
        GolfBallPhysics bf  = ball.GetComponent<GolfBallPhysics>();
        bf.RecordTeePosition();

        // --- 5.  Sand Penalty
        float sandFactor = bf.InSand ? bf.sandPenaltyFactor : 1f;
        float finalSpd   = ballSpd * sandFactor;

        // --- 6. Final Launch Conditions
        ball.linearVelocity    = new Vector2(Mathf.Cos(launchRad), Mathf.Sin(launchRad)) * finalSpd * dirSign;
        ball.angularVelocity = spinRad * Mathf.Rad2Deg * dirSign; 

        Debug.Log($"finalspd={finalSpd:F1}, ");

        float mag = ball.linearVelocity.magnitude;
        Debug.Log($"[Launch] speed = {mag:F3}");

        audioSource.PlayOneShot(hitSound);


        collider.enabled = false;

    }
}

