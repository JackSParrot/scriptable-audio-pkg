using System.Collections.Generic;
using UnityEngine;

#if ADDRESSABLE_ASSETS
using UnityEngine.AddressableAssets;
#endif

[System.Serializable]
public class SFXData
{
    public string ClipName;
#if ADDRESSABLE_ASSETS
    public AssetReference ReferencedClip;
#else
    public AudioClip Clip = null;
#endif
    [Range(0f, 1f)]
    public float Volume = 1f;
    [Range(.3f, 3f)]
    public float Pitch = 1f;
    public bool Loop = false;
}

[CreateAssetMenu(fileName = "New ClipStorer", menuName = "JackSParrot/Services/Audio/ClipStorer", order = 1)]
public class AudioClipsStorer : ScriptableObject
{
    [SerializeField] private List<SFXData> _clips = new List<SFXData>();

    public SFXData GetClipByName(string clipName)
    {
        foreach (var clip in _clips)
        {
            if (string.Equals(clip.ClipName, clipName, System.StringComparison.InvariantCultureIgnoreCase))
            {
                return clip;
            }
        }
        Debug.LogError($"Trying to get a nonexistent audio clip: {clipName}");
        return null;
    }
}
