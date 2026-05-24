using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float xPos;
    [SerializeField] float yPos;
    [SerializeField] float zPos;
    [SerializeField] int trackNumber = 1;
    [SerializeField] private int sideSpeed = 9;
    [SerializeField] private bool isMoving;
    [SerializeField] int moveDirection; //  (1=Left) (2=Right)
    
    void Update()
    {
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime),Space.World);
        xPos = gameObject.transform.position.x;
        zPos = gameObject.transform.position.z;

        if (isMoving && moveDirection == 1) // Moving Left
        {
            transform.Translate(Vector3.left * (sideSpeed * Time.deltaTime),Space.World);
            if (xPos <= trackNumber)
            {
                isMoving = false;
                moveDirection = 0;
                transform.position = new Vector3(trackNumber, yPos, zPos);
            }
        }
        if (isMoving && moveDirection == 2) // Moving Right
        {
            transform.Translate(Vector3.left * (sideSpeed * Time.deltaTime * -1),Space.World);
            if (xPos >= trackNumber)
            {
                isMoving = false;
                moveDirection = 0;
                transform.position = new Vector3(trackNumber, yPos, zPos);
            }
        }
    }
    
    public void LeftMove()
    {
        if (trackNumber == 1)
        {
            isMoving = true;
            moveDirection = 1;
            trackNumber = 0;
        }
        if (trackNumber == 2)
        {
            isMoving = true;
            moveDirection = 1;
            trackNumber = 1;
        }
    }

    public void RightMove()
    {
        if (trackNumber == 1)
        {
            isMoving = true;
            moveDirection = 2;
            trackNumber = 2;
        }
        if (trackNumber == 0)
        {
            isMoving = true;
            moveDirection = 2;
            trackNumber = 1;
        }
    }
}
