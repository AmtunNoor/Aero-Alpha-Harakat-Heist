using UnityEngine;

public class PlaneHeistController : MonoBehaviour
{
    [Header("Network Assignment")]
    public string associatedPlayerKey = "Player_192_168_1_55"; // Tied to the specific phone IP
    public string playerName = "Ahmed";

    [Header("Base Flight Settings")]
    public float baseSpeed = 8f;
    public float turnSpeed = 220f;
    public float boostMultiplier = 1.5f;

    [Header("Hot Carrier Penalties")]
    public bool isHotCarrier = false;
    [Range(0.5f, 1f)] public float carrierAccelerationPenalty = 0.75f; // Slightly reduced acceleration
    [Range(0.5f, 1f)] public float carrierTurnPenalty = 0.8f;         // Slightly wider turns

    [Header("Visual Trappings")]
    public GameObject goldenTrailIndicator; // Active during Hot Carrier Mode
    public GameObject regularTrailIndicator;

    private Rigidbody2D rb;
    private float currentRotation = 0f;

    // Runtime tracking of gathered items
    [HideInInspector] public bool holdsLetter = false;
    [HideInInspector] public bool holdsHarakah = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f; // Infinite open sky layout - zero gravity pull
    }

    void FixedUpdate()
    {
        // 1. Fetch values safely out of the concurrent server dictionary
        if (LocalWebSocketServer.PlayerInputs.TryGetValue(associatedPlayerKey, out var input))
        {
            HandleFlightMovement(input.x, input.y, input.boost);
        }
    }

    private void HandleFlightMovement(float inputX, float inputY, bool isBoosting)
    {
        // 2. Evaluate current modifier state based on core inventory combinations
        float targetTurnSpeed = turnSpeed;
        float targetSpeed = baseSpeed;

        if (isHotCarrier)
        {
            targetTurnSpeed *= carrierTurnPenalty;
            targetSpeed *= carrierAccelerationPenalty;
            
            if (goldenTrailIndicator != null) goldenTrailIndicator.SetActive(true);
            if (regularTrailIndicator != null) regularTrailIndicator.SetActive(false);
        }
        else
        {
            if (isBoosting) targetSpeed *= boostMultiplier;
            if (goldenTrailIndicator != null) goldenTrailIndicator.SetActive(false);
            if (regularTrailIndicator != null) regularTrailIndicator.SetActive(true);
        }

        // 3. Process 360-degree rotation vectors (Standard plane mechanics)
        if (Mathf.Abs(inputX) > 0.1f)
        {
            currentRotation -= inputX * targetTurnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(currentRotation);
        }

        // 4. Drive structural forward velocity based on joystick vector heights
        Vector2 forwardDirection = transform.up;
        float throttle = Mathf.Max(0.2f, inputY); // Continuous slow cruise if no input
        rb.linearVelocity = forwardDirection * (throttle * targetSpeed);
    }

    // 5. Interception & Collision Matrix (The Steal Mechanic)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlaneHeistController opponent = collision.gameObject.GetComponent<PlaneHeistController>();
        if (opponent == null) return;

        // Triggers impact screen feedback logic directly on main camera
        TriggerCollisionFeedback();

        // If colliding with the Hot Carrier, force an immediate item drop/scramble
        if (opponent.isHotCarrier)
        {
            ExecuteStealScramble(opponent);
        }
    }

    private void ExecuteStealScramble(PlaneHeistController victim)
    {
        victim.isHotCarrier = false;
        
        // Randomly split the combination back into individual items
        if (Random.value > 0.5f)
        {
            victim.holdsLetter = false;
            this.holdsLetter = true; // Stripped straight to interceptor
            Debug.Log($"[Match Event] {this.playerName} stole the Letter from {victim.playerName}!");
        }
        else
        {
            victim.holdsHarakah = false;
            this.holdsHarakah = true; // Stripped straight to interceptor
            Debug.Log($"[Match Event] {this.playerName} stole the Harakah from {victim.playerName}!");
        }
        
        // Recalculate component assemblies on both ends
        victim.EvaluateInventoryCombination();
        this.EvaluateInventoryCombination();
    }

    public void EvaluateInventoryCombination()
    {
        if (holdsLetter && holdsHarakah)
        {
            isHotCarrier = true;
            Debug.Log($"[Match Event] {playerName} assembled a syllable! Entering HOT CARRIER MODE.");
        }
    }

    private void TriggerCollisionFeedback()
    {
        // Broadcasts state changes to trigger shockwaves, bursts, and screen shake
        Debug.Log("[Impact] Collision occurred. Triggering visual feedback loops.");
    }
}
