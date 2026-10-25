using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoopManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerController player;
    [SerializeField] Transform startPoint;
    [SerializeField] GameObject clonePrefab;

    [Header("Rules")]
    [SerializeField] int maxClones = 1;
    [Tooltip("Looping this close to the start is ignored (the clone would spawn on top of you).")]
    [SerializeField] float minDistanceFromStart = 1f;

    readonly Queue<GameObject> clones = new Queue<GameObject>();

    void Start()
    {
        player.TeleportTo(startPoint.position);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.eKey.wasPressedThisFrame) Loop();
        if (kb.rKey.wasPressedThisFrame) RestartLevel();
    }

    /// <summary>Leave a frozen body where the player stands, send the player back to the start.</summary>
    public void Loop()
    {
        if (Vector2.Distance(player.transform.position, startPoint.position) < minDistanceFromStart)
            return;

        GameObject clone = Instantiate(clonePrefab, player.transform.position, Quaternion.identity);

        // the clone keeps the direction you were facing (and whether you were upside down)
        var src = player.GetComponentInChildren<SpriteRenderer>();
        var dst = clone.GetComponentInChildren<SpriteRenderer>();
        if (src != null && dst != null)
        {
            dst.flipX = src.flipX;
            dst.flipY = src.flipY;
        }

        clones.Enqueue(clone);

        // over the cap: the oldest clone disappears
        while (clones.Count > maxClones)
            Destroy(clones.Dequeue());

        player.TeleportTo(startPoint.position);
    }

    /// <summary>Death: back to the start, clones stay. Call this from hazards later.</summary>
    public void KillPlayer()
    {
        player.TeleportTo(startPoint.position);
    }

    /// <summary>Full restart: clears every clone and resets the player.</summary>
    public void RestartLevel()
    {
        while (clones.Count > 0)
            Destroy(clones.Dequeue());

        player.TeleportTo(startPoint.position);
    }

    /// <summary>Levels can raise or lower the cap as they teach new things.</summary>
    public void SetMaxClones(int value)
    {
        maxClones = Mathf.Max(1, value);
        while (clones.Count > maxClones)
            Destroy(clones.Dequeue());
    }
}