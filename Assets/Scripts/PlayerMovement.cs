using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] private int sideSpeed = 9;
    
    [SerializeField] int trackNumber = 0; // Tracks target X position (-1, 0, 1)
    [SerializeField] private bool isMoving;
    [SerializeField] int moveDirection; // (1=Left) (2=Right)
    
    void Update()
    {
        // Constant forward movement
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime), Space.World);

        if (isMoving) // 2. Handle horizontal movement
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
    }

    private void StopMoving() // Helper method to cleanly snap and stop
    {
        isMoving = false;
        moveDirection = 0;

        transform.position = new Vector3(trackNumber, transform.position.y, transform.position.z);
    }
    
    public void LeftMove()
    {
        if (isMoving) return; // Prevent pressing again mid-move

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
        if (isMoving) return; // Prevent pressing again mid-move

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
}