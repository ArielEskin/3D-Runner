using UnityEngine;

public class PlayerMovement : MonoBehaviour 
{
    // ==========references================================================================================================================================================
    // [field: SerializeField] allows the GameManager to read and change this speed when the difficulty levels up, while keeping it visible in the Inspector
    [field: SerializeField] public float moveSpeed {get; set;} = 5f;
    [SerializeField] private int sideSpeed = 9;
    
    [SerializeField] int trackNumber = 0; // Tracks target X position (-1, 0, 1)
    [SerializeField] private bool isMoving; 
    [SerializeField] int moveDirection; // (1=Left) (2=Right)

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpSpeed = 6f;
    [SerializeField] private Animator animator;
    
    private bool isJumping = false; // To prevent double jump while in the air
    public bool isFalling {get; set;}
    private float originalY; // Remembers where the "ground" is so the player doesn't fall through the floor
    
    // ==========================================================================================================================================================

    void Start() // Caches the Animator and saves the player's starting Y-position to act as the permanent ground level
    {
        originalY = transform.position.y;
        if (animator == null) animator = GetComponentInChildren<Animator>(); 
    }

    void Update() // Pushes the player forward constantly and checks if they need to shift lanes or update their jump arc
    {
        if (GameManager.gameManager.isDead) return;
        
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime), Space.World);

        if (isMoving)
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
        HandleJump();
    }
    

    private void HandleJump() // Manages the physics math to move the player up to a peak height and back down to the exact ground level.
    {
        if (isJumping)
        {
            transform.Translate(Vector3.up * (jumpSpeed * Time.deltaTime), Space.World);
            if (transform.position.y >= originalY + jumpHeight)
            {
                isJumping = false;
                isFalling = true;
            }
        }
        else if (isFalling)
        {
            transform.Translate(Vector3.down * (jumpSpeed * Time.deltaTime), Space.World);
            if (transform.position.y <= originalY)
            {
                // Snap cleanly back to the ground level
                transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
                isFalling = false;
            }
        }
    }
    
    private void StopMoving() // Snaps the player exactly to the mathematical center of their target lane to prevent drifting.
    {
        isMoving = false;
        moveDirection = 0;
        transform.position = new Vector3(trackNumber, transform.position.y, transform.position.z);
    }
    
    public void LeftMove() // Called when the UI Left Button is pressed
    {
        if (isMoving) return;

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
        if (isMoving) return;

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
        bool isPhysicallyGrounded = !isJumping && !isFalling; 
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
    public void TriggerDeathAnimation(string obstacleTag) // Triggered by the GameManager when the player collides with an obstacle
    {
        if (animator != null)
        {
            if (obstacleTag == "LowObstacle")
            {
                animator.SetTrigger("DieLow");
            }
            else if (obstacleTag == "HighObstacle")
            {
                animator.SetTrigger("DieHigh");
            }
        }
    }
}