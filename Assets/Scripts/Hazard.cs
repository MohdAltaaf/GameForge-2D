using UnityEngine;

/// <summary>
/// Put this on anything deadly: spikes, a kill zone under the level, a saw, etc.
/// Needs a Collider2D set to Is Trigger. Only the real player dies, clones are unharmed.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    LoopManager loopManager;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Start()
    {
        loopManager = FindFirstObjectByType<LoopManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (loopManager == null) return;
        if (!other.TryGetComponent<PlayerController>(out _)) return;

        loopManager.KillPlayer();
    }
    
}


