using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private PlayerMovement playerMovement;
    void Start()
    {
        
    }

    void Update()
    {
        transform.Translate(Vector3.forward * (playerMovement.moveSpeed * Time.deltaTime),Space.World);
    }
}
