using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMoveToBall : MonoBehaviour
{
    [Header("References")]
    public Transform ball;
    public Transform stanceCenter;   // new: child at middle of feet
    public float moveSpeed = 5f;

    public Rigidbody2D rb;
    private GolfBallPhysics ballPhysics;
    //public BallFlight2D ballPhysics;
    public float stanceOffsetX;

    void Awake()
    {
        rb           = GetComponent<Rigidbody2D>();
        ballPhysics  = ball.GetComponent<GolfBallPhysics>();
        //ballPhysics  = ball.GetComponent<BallFlight2D>();

        rb.bodyType  = RigidbodyType2D.Kinematic;

        // cache how far in local space your stanceCenter sits from the Player origin
        //stanceOffsetX = stanceCenter.localPosition.x;
    }

    void FixedUpdate()
    {
        // only reposition when the ball is truly stopped
        if (ballPhysics.isGrounded && ballPhysics.rb.linearVelocity == Vector2.zero)
        {
            // desired player X so that (playerPos + stanceOffsetX) == ball.x
            float desiredX = ball.position.x - stanceOffsetX;

            Vector2 targetPos = new Vector2(desiredX, rb.position.y);
            Vector2 nextPos   = Vector2.MoveTowards(
                                     rb.position,
                                     targetPos,
                                     moveSpeed * Time.fixedDeltaTime);

            rb.MovePosition(nextPos);
        }
    }
}
