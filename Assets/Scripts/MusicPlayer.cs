using UnityEngine;

/// <summary>
/// Plays one clip on loop for the entire game, across every scene, without any scene setup.
/// Put your audio file at:  Assets/Resources/Music.wav  (or .ogg / .mp3, the name must be "Music").
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    const string ClipName = "Music";
    const float Volume = 0.6f;

    static MusicPlayer instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (instance != null) return;

        var clip = Resources.Load<AudioClip>(ClipName);
        if (clip == null)
        {
            Debug.LogWarning($"MusicPlayer: no audio clip named '{ClipName}' found in a Resources folder.");
            return;
        }

        var go = new GameObject("MusicPlayer");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<MusicPlayer>();

        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.volume = Volume;
        source.playOnAwake = false;
        source.spatialBlend = 0f;              // plain 2D sound, same volume everywhere
        source.ignoreListenerPause = true;     // keeps playing even if AudioListener.pause is used
        source.Play();
    }
}