using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] Transform target;
    [SerializeField] Vector2 offset = new Vector2(0f, 1.5f);
    [SerializeField] float smoothTime = 0.15f;

    [Header("Level bounds (world units)")]
    [SerializeField] bool useBounds = true;
    [SerializeField] Vector2 boundsMin = new Vector2(-10f, -5f);
    [SerializeField] Vector2 boundsMax = new Vector2(30f, 15f);

    Camera cam;
    Vector3 velocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        Snap();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = Clamp(Desired());
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }

    /// <summary>Jump straight to the target with no smoothing.</summary>
    public void Snap()
    {
        if (target == null) return;
        velocity = Vector3.zero;
        transform.position = Clamp(Desired());
    }

    Vector3 Desired()
    {
        return new Vector3(target.position.x + offset.x,
                           target.position.y + offset.y,
                           transform.position.z);
    }

    Vector3 Clamp(Vector3 p)
    {
        if (!useBounds) return p;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // if the level is smaller than the screen, just centre on it
        if (boundsMax.x - boundsMin.x <= halfW * 2f)
            p.x = (boundsMin.x + boundsMax.x) * 0.5f;
        else
            p.x = Mathf.Clamp(p.x, boundsMin.x + halfW, boundsMax.x - halfW);

        if (boundsMax.y - boundsMin.y <= halfH * 2f)
            p.y = (boundsMin.y + boundsMax.y) * 0.5f;
        else
            p.y = Mathf.Clamp(p.y, boundsMin.y + halfH, boundsMax.y - halfH);

        return p;
    }

    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((boundsMin.x + boundsMax.x) * 0.5f, (boundsMin.y + boundsMax.y) * 0.5f, 0f);
        Vector3 size = new Vector3(boundsMax.x - boundsMin.x, boundsMax.y - boundsMin.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}