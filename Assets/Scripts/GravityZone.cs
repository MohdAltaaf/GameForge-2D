using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GravityZone : MonoBehaviour
{
    public enum Mode
    {
        InvertWhileInside,   // upside down while in the zone, normal again when you leave
        FlipOnEnter          // every time you enter, gravity toggles (a gravity pad)
    }

    [SerializeField] Mode mode = Mode.InvertWhileInside;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerController>(out var player)) return;

        if (mode == Mode.InvertWhileInside) player.SetGravityDirection(-1f);
        else                                player.FlipGravity();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (mode != Mode.InvertWhileInside) return;
        if (!other.TryGetComponent<PlayerController>(out var player)) return;

        player.SetGravityDirection(1f);
    }

    void OnDrawGizmos()
    {
        // faint tint in the Scene view so zones are easy to see while building
        var col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}