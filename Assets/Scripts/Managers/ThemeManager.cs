using UnityEngine;

// This negative execution order guarantees this Awake() runs BEFORE ObjectPooler.Awake() and TrackPooler.Awake()
[DefaultExecutionOrder(-50)]
public class ThemeManager : MonoBehaviour
{
    [Header("Available Themes")]
    [Tooltip("Drag all your ThemeData assets here")]
    public ThemeData[] availableThemes;
    
    [Header("Pooler References")]
    public TrackPooler trackPooler;
    public ObjectPooler objectPooler;

    private void Awake() // Finds the selected theme from the active profile and applies it before anything else spawns
    {
        string selectedID = "Earth";
        if (ProfileManager.instance != null && ProfileManager.instance.activeProfile != null)
        {
            PlayerProfileData profile =
                ProfileManager.instance.activeProfile;
            bool isUnlocked = profile.unlockedThemes != null &&
                              profile.unlockedThemes.Contains(
                                  profile.selectedThemeID
                              );
            selectedID = isUnlocked
                ? profile.selectedThemeID
                : "Earth";
        }

        ThemeData currentTheme = null;
        foreach (var theme in availableThemes)
        {
            if (theme.themeID == selectedID)
            {
                currentTheme = theme;
                break;
            }
        }

        // Safety fallback to the first theme if the ID wasn't found
        if (currentTheme == null && availableThemes.Length > 0)
        {
            currentTheme = availableThemes[0];
            Debug.LogWarning("Theme not found. Falling back to default.");
        }

        if (currentTheme != null)
        {
            ApplyTheme(currentTheme);
        }
    }

    private void ApplyTheme(ThemeData theme) // Overrides the skybox and object pooler prefabs with the theme's specific assets
    {
        if (theme.skyboxMaterial != null)
        {
            RenderSettings.skybox = theme.skyboxMaterial;
        }
        if (trackPooler != null && theme.trackPrefab != null)
        {
            trackPooler.trackPrefab = theme.trackPrefab;
        }
        if (objectPooler != null)
        {
            foreach (var pool in objectPooler.pools)
            {
                if (pool.tag == "Tree" && theme.treePrefab != null) 
                    pool.prefab = theme.treePrefab;
                    
                else if (pool.tag == "Bush" && theme.bushPrefab != null) 
                    pool.prefab = theme.bushPrefab;
                    
                else if (pool.tag == "LowObstacle" && theme.lowObstaclePrefab != null) 
                    pool.prefab = theme.lowObstaclePrefab;
                    
                else if (pool.tag == "HighObstacle" && theme.highObstaclePrefab != null) 
                    pool.prefab = theme.highObstaclePrefab;
            }
        }
        
        Debug.Log("Successfully applied theme visuals for: " + theme.themeID);
    }
}
