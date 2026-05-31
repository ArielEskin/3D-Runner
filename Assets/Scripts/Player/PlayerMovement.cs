using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [field: SerializeField] public float moveSpeed {get; set;} = 5f;
    [SerializeField] private int sideSpeed = 9;
    
    
    [SerializeField] int trackNumber = 0; // Tracks target X position (-1, 0, 1)
    [SerializeField] private bool isMoving;
    [SerializeField] int moveDirection; // (1=Left) (2=Right)

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpSpeed = 6f;
    [SerializeField] private Animator animator; // Reference to trigger the jump animation
    
    private bool isJumping = false;
    public bool isFalling {get; set;}
    private float originalY;

    void Start()
    {
        originalY = transform.position.y;
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
        HandleJump();
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

    private void StopMoving() 
    {
        isMoving = false;
        moveDirection = 0;
        transform.position = new Vector3(trackNumber, transform.position.y, transform.position.z);
    }
    
    public void LeftMove()
    {
        if (isMoving) return; 

        if (trackNumber == 0)
        {
            isMoving = true;
            moveDirection = 1;
            trackNumber = -1;
        }
        else if (trackNumber == 1)
        {
            isMoving = true;
            moveDirection = 1;
            trackNumber = 0;
        }
    }

    public void RightMove()
    {
        if (isMoving) return; 

        if (trackNumber == 0)
        {
            isMoving = true;
            moveDirection = 2;
            trackNumber = 1;
        }
        else if (trackNumber == -1)
        {
            isMoving = true;
            moveDirection = 2;
            trackNumber = 0;
        }
    }
    public void Jump()
    {
        bool isPhysicallyGrounded = !isJumping && !isFalling;
        bool isVisuallyRunning = animator.GetCurrentAnimatorStateInfo(0).IsName("Running") && !animator.IsInTransition(0);
        
        if (isPhysicallyGrounded && isVisuallyRunning)
        {
            isJumping = true;
            
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }
    public void TriggerDeathAnimation(string obstacleTag)
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