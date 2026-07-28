using UnityEngine;

public class Coin : MonoBehaviour
{
    // ==========references================================================================================================================================================

    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private float flySpeed = 25f; // How fast they fly to the player with magnet active
    
    [Header("Transparency Settings")]
    [SerializeField] private float targetAlpha = 0.3f; // How see-through it gets
    [SerializeField] private float fadeSpeed = 8f;     // How fast it fades

    private Transform playerTransform;
    
    // Components for fading
    private MeshRenderer meshRenderer;
    private Color originalColor;
    private bool isFading = false;
    
    // ==========================================================================================================================================================

    private void Awake() // Caches the renderer and locates the player target
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    private void Start() // Caches the renderer and locates the player target.
    {
        // Find the player once so we know who to fly towards and check distance against
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void OnEnable() // Resets the coin's transparency back to 100% solid every time it is pulled from the object pool
    {
        isFading = false;
        if (meshRenderer != null)
        {
            Color resetColor = originalColor;
            resetColor.a = 1f;
            meshRenderer.material.color = resetColor;
        }
    }

    private void Update() // Pulls the coin towards the player if the magnet is active, or fades it out if the player runs past it
    {
        if (playerTransform == null) return;

        // ==========================================
        // MAGNET LOGIC
        // ==========================================
        if (GameManager.gameManager.isMagnetActive)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distance <= GameManager.gameManager.magnetRadius)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position + Vector3.up, flySpeed * Time.deltaTime);
            }
        }

        // ==========================================
        // TRANSPARENCY LOGIC
        // ==========================================
        if (playerTransform.position.z > transform.position.z) { isFading = true; }
        if (isFading && meshRenderer != null)
        {
            Color currentColor = meshRenderer.material.color;
            float newAlpha = Mathf.Lerp(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            meshRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
        }
    }

    private void OnTriggerEnter(Collider other) // Adds to the score, plays a sound, and returns the coin to the pool upon collision
    {
        if (other.CompareTag("Player"))
        {
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlaySound3D("PickUpCoin", transform.position); 
            }
            
            GameManager.gameManager.AddCoin(coinValue); 
            ObjectPooler.Instance.ReturnToPool(gameObject); 
        }
    }
}