using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dependencies")]
    public GolfBallPhysics ball;    // assign your ball here
    public UIManager      ui;      // assign your UIManager here

    [Header("Timings")]
    public float holeDelay = 2f;    // pause before showing Game Over

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        EnterStartState();
    }

    // ----------------- State Transitions -----------------

    public void EnterStartState()
    {
        ui.ShowStart();         // show the title screen
        ball.DeactivatePhysics();
        ball.ResetToTee();      // position ball at tee
        
        var playerMover = FindObjectOfType<PlayerMoveToBall>();
        if (playerMover != null)
            playerMover.ResetToStart();
    }

    public void OnPlayButton()
    {
        EnterPlayState();
    }

    void EnterPlayState()
    {
        ui.ShowGame();          // hide start, show gameplay UI
        ball.ActivatePhysics();
        ball.ResetToTeeNewGame();      
        ball.Strokes = 0;       // reset stroke count
    }

    public void OnBallInHole()
    {
        EnterEndState();
    }

    void EnterEndState()
    {
        ball.DeactivatePhysics();
        Invoke(nameof(ShowGameOver), holeDelay);
    }

    void ShowGameOver()
    {
        ui.ShowGameOver();
    }
}
