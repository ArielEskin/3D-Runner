using UnityEngine;

[System.Serializable]
public struct SoundEffect // Inspector struct for soundsEffect
{
    public string groupID;
    public AudioClip[] clips;
}

public class SoundLibrary : MonoBehaviour
{
    public SoundEffect[] soundEffect;

    public AudioClip GetClipFromName(string name)
    {
        foreach (var soundEffect in soundEffect)
        {
            if (soundEffect.groupID == name)
            {
                return soundEffect.clips[Random.Range(0, soundEffect.clips.Length)];
            }            
        }
        return null;
    }
}
