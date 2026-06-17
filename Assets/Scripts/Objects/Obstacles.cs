using UnityEngine;

public class Obstacles : MonoBehaviour
{
    [Header("Transparency Settings")]
    [Tooltip("Check this box ONLY for High Obstacles in the Inspector!")]
    [SerializeField] private bool canBecomeTransparent = false; 
    [SerializeField] private float targetAlpha = 0.3f; // How see-through it gets (0 = invisible, 1 = solid)
    [SerializeField] private float fadeSpeed = 8f;     // How fast it fades

    private Transform playerTransform;
    private MeshRenderer meshRenderer;
    private Color originalColor;
    private bool isFading = false;

    private void Awake()
    {
        // Grab the 3D mesh (checks children just in case your model is inside an empty game object)
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    private void Start()
    {
        // Find the player once so we don't have to search for them every frame
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void OnEnable()
    {
        // CRITICAL FOR OBJECT POOLING: Reset the obstacle to fully solid every time it spawns!
        isFading = false;
        if (meshRenderer != null)
        {
            Color resetColor = originalColor;
            resetColor.a = 1f; // Force Alpha back to 100%
            meshRenderer.material.color = resetColor;
        }
    }

    private void Update()
    {
        // If this isn't a high obstacle, or we are missing references, do nothing
        if (!canBecomeTransparent || playerTransform == null || meshRenderer == null) return;

        // Trigger the fade the moment the player runs past the obstacle's exact Z coordinate
        if (playerTransform.position.z > transform.position.z)
        {
            isFading = true;
        }

        // Smoothly fade the material's alpha over time
        if (isFading)
        {
            Color currentColor = meshRenderer.material.color;
            float newAlpha = Mathf.Lerp(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            meshRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
        }
    }

    private void OnTriggerEnter(Collider other) // If player hits Obstacle ----> DIE
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.gameManager.isInvincible) return; // Invincibility power ups active

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