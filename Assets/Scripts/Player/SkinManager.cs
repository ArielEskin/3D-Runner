using UnityEngine;

public class SkinManager : MonoBehaviour
{
    [Header("Assign your skins here in the Inspector")]
    [Tooltip("Drag the child skin GameObjects here. The default skin should be element 0, the zombie element 1, etc.")]
    public GameObject[] skins;

    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        
        int selectedSkinIndex = PlayerPrefs.GetInt("SelectedSkin", 0);
        if (ProfileManager.instance != null &&
            ProfileManager.instance.activeProfile != null)
        {
            selectedSkinIndex =
                ProfileManager.instance.activeProfile.selectedSkinIndex;
        }

        // Safety check
        if (selectedSkinIndex < 0 || selectedSkinIndex >= skins.Length)
        {
            selectedSkinIndex = 0;
        }

        // Loop through all skins and enable/disable them accordingly
        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i] != null)
            {
                if (i == selectedSkinIndex)
                {
                    skins[i].SetActive(true);
                    
                    // Grab its Animator and pass it to PlayerMovement
                    Animator activeAnimator = skins[i].GetComponent<Animator>();
                    if (activeAnimator != null && playerMovement != null)
                    {
                        playerMovement.SetAnimator(activeAnimator);
                    }
                    else
                    {
                        Debug.LogWarning("SkinManager: The active skin does not have an Animator component!");
                    }
                }
                else
                {
                    skins[i].SetActive(false); // Turn OFF all other skins
                }
            }
        }
    }
}
