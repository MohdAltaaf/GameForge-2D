using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerBlink : MonoBehaviour
{
    [Header("Frames")]
    [Tooltip("Element 0 = eyes open (idle). The rest are the blink frames, played in order.")]
    [SerializeField] Sprite[] frames;
    [SerializeField] float frameTime = 0.06f;

    [Header("Timing")]
    [SerializeField] Vector2 blinkInterval = new Vector2(2f, 5f);   // random wait between blinks
    [Range(0f, 1f)] [SerializeField] float doubleBlinkChance = 0.15f;

    [Header("Facing")]
    [SerializeField] bool flipWithMovement = true;
    [Tooltip("Tick if the sprite is drawn looking right (yours is).")]
    [SerializeField] bool spriteFacesRight = true;
    [SerializeField] float flipThreshold = 0.1f;

    SpriteRenderer sr;
    Rigidbody2D rb;
    PlayerController player;   // null on clones, which is fine

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();
    }

    void OnEnable()
    {
        if (frames == null || frames.Length == 0) return;
        sr.sprite = frames[0];
        StartCoroutine(BlinkLoop());
    }

    void Update()
    {
        if (player != null) sr.flipY = player.GravityInverted;

        if (!flipWithMovement || rb == null) return;

        float vx = rb.linearVelocity.x;
        if (vx > flipThreshold)       sr.flipX = !spriteFacesRight;
        else if (vx < -flipThreshold) sr.flipX = spriteFacesRight;
        // standing still keeps the last direction
    }

    IEnumerator BlinkLoop()
    {
        // clones start at slightly different times so they don't blink in sync
        yield return new WaitForSeconds(Random.Range(0f, blinkInterval.x));

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(blinkInterval.x, blinkInterval.y));
            yield return Blink();

            if (Random.value < doubleBlinkChance)
            {
                yield return new WaitForSeconds(0.12f);
                yield return Blink();
            }
        }
    }

    IEnumerator Blink()
    {
        for (int i = 1; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameTime);
        }
        sr.sprite = frames[0];
    }
}