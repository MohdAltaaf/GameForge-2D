using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    bool triggered;

    void Reset()
    {
        // make the collider a trigger as soon as the script is added
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        // only the real player counts; clones have no PlayerController
        if (!other.TryGetComponent<PlayerController>(out _)) return;

        triggered = true;
        SceneLoader.LoadNext();
    }
   

}


