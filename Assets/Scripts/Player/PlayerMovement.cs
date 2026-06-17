using UnityEngine;

// Handles all player movement logic including constant forward running,
// snapping to left/middle/right lanes, and jumping over obstacles.
public class PlayerMovement : MonoBehaviour 
{
    // [field: SerializeField] allows the GameManager to read and change this speed 
    // when the difficulty levels up, while keeping it visible in the Inspector
    [field: SerializeField] public float moveSpeed {get; set;} = 5f;
    [SerializeField] private int sideSpeed = 9; // How fast the player visibly slides from one lane to anothe
    
    [SerializeField] int trackNumber = 0; // Tracks target X position (-1, 0, 1)
    [SerializeField] private bool isMoving; // Prevents the player from starting a new turn while they are already sliding
    [SerializeField] int moveDirection; // (1=Left) (2=Right)

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpSpeed = 6f;
    [SerializeField] private Animator animator; // Reference to trigger the jump animation
    
    private bool isJumping = false; // Physics state tracking so the player can't double jump while in the air
    public bool isFalling {get; set;}
    private float originalY; // Remembers where the "ground" is so the player doesn't fall through the floor

    void Start()
    {
        originalY = transform.position.y; // Save the exact starting height of the player as our permanent ground level
        if (animator == null) animator = GetComponentInChildren<Animator>(); 
    }

    void Update()
    {
        if (GameManager.gameManager.isDead) return; // Stop all movement if the player is dead
        
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime), Space.World); // Constant forward movement

        if (isMoving) // Handle horizontal movement
        {
            if (moveDirection == 1) // Moving Left
            {
                transform.Translate(Vector3.left * (sideSpeed * Time.deltaTime), Space.World);

                if (transform.position.x <= trackNumber)
                {
                    StopMoving();
                }
            }
            else if (moveDirection == 2) // Moving Right
            {
                transform.Translate(Vector3.right * (sideSpeed * Time.deltaTime), Space.World);

                if (transform.position.x >= trackNumber)
                {
                    StopMoving();
                }
            }
        }
        HandleJump(); // Always run the jump logic to see if we need to move up or down
    }
    

    private void HandleJump()
    {
        if (isJumping)
        {
            // Move up towards the peak of the jump
            transform.Translate(Vector3.up * (jumpSpeed * Time.deltaTime), Space.World);
            if (transform.position.y >= originalY + jumpHeight)
            {
                isJumping = false;
                isFalling = true; // Reached the top, start falling
            }
        }
        else if (isFalling)
        {
            // Move back down
            transform.Translate(Vector3.down * (jumpSpeed * Time.deltaTime), Space.World);
            if (transform.position.y <= originalY)
            {
                // Snap cleanly back to the ground level
                transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
                isFalling = false;
            }
        }
    }
    
    private void StopMoving() // Snaps the player exactly to the center of their lane to correct any tiny math errors 
    {
        isMoving = false;
        moveDirection = 0;
        transform.position = new Vector3(trackNumber, transform.position.y, transform.position.z); // Force the X position to be exactly -1, 0, or 1 based on the trackNumber
    }
    
    public void LeftMove() // Called when the UI Left Button is pressed
    {
        if (isMoving) return;  // Don't accept input if we are already in the middle of sliding

        if (trackNumber == 0) // If in the middle, go left
        {
            SoundManager.instance.PlaySound3D("DashingSound",  transform.position);
            isMoving = true;
            moveDirection = 1;
            trackNumber = -1;
        }
        else if (trackNumber == 1) // If on the right, go to the middle
        {
            SoundManager.instance.PlaySound3D("DashingSound",  transform.position);
            isMoving = true;
            moveDirection = 1;
            trackNumber = 0;
        }
    }

    public void RightMove() // Called when the UI Right Button is pressed
    {
        if (isMoving) return; // Don't accept input if we are already in the middle of sliding

        if (trackNumber == 0) // If in the middle, go right
        {
            SoundManager.instance.PlaySound3D("DashingSound",  transform.position);
            isMoving = true;
            moveDirection = 2;
            trackNumber = 1;
        }
        else if (trackNumber == -1) // If on the left, go to the middle
        {
            SoundManager.instance.PlaySound3D("DashingSound",  transform.position);
            isMoving = true;
            moveDirection = 2;
            trackNumber = 0;
        }
    }
    public void Jump() // Called when the UI Jump Button is pressed
    {
        bool isPhysicallyGrounded = !isJumping && !isFalling; // Ensure the player is firmly on the ground
        bool isVisuallyRunning = animator.GetCurrentAnimatorStateInfo(0).IsName("Running") && !animator.IsInTransition(0); 
        
        if (isPhysicallyGrounded && isVisuallyRunning)
        {
            isJumping = true;
            
            if (animator != null)
            {
                SoundManager.instance.PlaySound3D("JumpingSound",  transform.position);
                animator.SetTrigger("Jump");
            }
        }
    }
    public void TriggerDeathAnimation(string obstacleTag) // Triggered by the GameManager when the player collides with an obstacle box.
    {
        if (animator != null)
        {
            if (obstacleTag == "LowObstacle") // Play a tripping animation if they hit a low hurdle
            {
                animator.SetTrigger("DieLow");
            }
            else if (obstacleTag == "HighObstacle") // Play a face-plant animation if they crash into a tall wall
            {
                animator.SetTrigger("DieHigh");
            }
        }
    }
}