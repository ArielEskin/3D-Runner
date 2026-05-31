using UnityEngine;

public class Obstacles : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered obstacle: " + other.name);

        if (other.CompareTag("Player"))
        {
                Debug.Log("GAME OVER");
                GameManager.gameManager.KillPlayer();
            
        }
    }
}
