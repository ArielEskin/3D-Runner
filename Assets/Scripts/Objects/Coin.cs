using UnityEngine;

public class Coin : MonoBehaviour
{
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

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    private void Start()
    {
        // Find the player once so we know who to fly towards and check distance against
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void OnEnable()
    {
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
        if (playerTransform == null) return;

        // ==========================================
        // MAGNET LOGIC
        // ==========================================
        if (GameManager.gameManager.isMagnetActive)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distance <= GameManager.gameManager.magnetRadius)
            {
                // Fly smoothly towards the player's center
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position + Vector3.up, flySpeed * Time.deltaTime);
            }
        }

        // ==========================================
        // TRANSPARENCY LOGIC
        // ==========================================
        if (playerTransform.position.z > transform.position.z)
        {
            isFading = true;
        }

        // Smoothly fade the material's alpha over time
        if (isFading && meshRenderer != null)
        {
            Color currentColor = meshRenderer.material.color;
            float newAlpha = Mathf.Lerp(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            meshRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Safety net so the game doesn't crash when testing the scene directly
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlaySound3D("PickUpCoin", transform.position); 
            }
            
            GameManager.gameManager.AddCoin(coinValue); 
            ObjectPooler.Instance.ReturnToPool(gameObject); 
        }
    }
}