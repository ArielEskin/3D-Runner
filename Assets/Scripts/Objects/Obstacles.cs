using UnityEngine;

public class Obstacles : MonoBehaviour
{
    // ==========references================================================================================================================================================

    [Header("Transparency Settings")]
    [SerializeField] private bool canBecomeTransparent = false; 
    [SerializeField] private float targetAlpha = 0.3f;
    [SerializeField] private float fadeSpeed = 8f;

    private Transform playerTransform;
    private MeshRenderer meshRenderer;
    private Color originalColor;
    private bool isFading = false;
    
    // ==========================================================================================================================================================

    private void Awake() // Caches references 
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    private void Start() // Find the player once so we don't have to search for them every frame
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void OnEnable() // Reset the obstacle to fully solid every time it spawns
    {
        isFading = false;
        if (meshRenderer != null)
        {
            Color resetColor = originalColor;
            resetColor.a = 1f;
            meshRenderer.material.color = resetColor;
        }
    }

    private void Update() // Fades tall obstacles to become transparent when they are close to the camera
    {
        if (!canBecomeTransparent || playerTransform == null || meshRenderer == null) return;
        
        if (playerTransform.position.z > transform.position.z)
        {
            isFading = true;
        }
        
        if (isFading)
        {
            Color currentColor = meshRenderer.material.color;
            float newAlpha = Mathf.Lerp(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            meshRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
        }
    }

    private void OnTriggerEnter(Collider other) // Tells the GameManager to kill the player unless they have an active invincibility buff
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.gameManager.isInvincible) return;

            if (gameObject.name.Contains("Low"))
            {
                GameManager.gameManager.KillPlayer("LowObstacle");
            }
            else
            {
                GameManager.gameManager.KillPlayer("HighObstacle");
            }
        }
    }
}