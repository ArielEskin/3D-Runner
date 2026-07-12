using UnityEngine;

[CreateAssetMenu(fileName = "ThemeData", menuName = "Scriptable Objects/ThemeData")]
public class ThemeData : ScriptableObject
{
    public string themeID;
    public Material skyboxMaterial;
    
    [Header("Pooler Overrides")]
    public GameObject trackPrefab;
    public GameObject treePrefab;
    public GameObject bushPrefab;
    public GameObject lowObstaclePrefab;
    public GameObject highObstaclePrefab;
}
