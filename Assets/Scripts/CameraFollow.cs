using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4;
    void Start()
    {
        
    }

    void Update()
    {
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime),Space.World);
    }
}
