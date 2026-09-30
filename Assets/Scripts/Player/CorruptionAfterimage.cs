using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "Bong nhan vat lech" (GDD muc 10): nha lai cac ban sao mo dan phia sau nhan vat.
/// Cang Nhiem cao thi bong ra cang day, lech cang xa va cang tim.
/// Cung dung lai duoc cho luc dich chuyen bang Canh Cua qua <see cref="Burst"/>.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CorruptionAfterimage : MonoBehaviour
{
    [Header("Nhip nha bong")]
    [Tooltip("Giay giua hai bong khi Nhiem = 100. Nhiem thap hon thi thua hon.")]
    public float minInterval = 0.05f;
    [Tooltip("Nhiem toi thieu (0-1) de bat dau co bong.")]
    [Range(0f, 1f)] public float threshold = 0.35f;

    [Header("Hinh thuc")]
    public float lifetime = 0.45f;
    public Color tint = new Color(0.55f, 0.20f, 0.78f);
    [Tooltip("Do lech toi da so voi nhan vat, theo unit.")]
    public float maxOffset = 0.18f;
    [Range(0f, 1f)] public float startAlpha = 0.5f;
    [Tooltip("Ve sau nhan vat bao nhieu bac sorting order.")]
    public int sortingOffset = -1;

    [Header("Chi dung bong khi nhan vat dang di chuyen")]
    public bool requireMovement = true;
    public float movementThreshold = 0.05f;

    SpriteRenderer source;
    float timer;
    Vector3 lastPosition;

    readonly List<Ghost> ghosts = new();
    readonly Stack<Ghost> pool = new();

    class Ghost
    {
        public SpriteRenderer renderer;
        public float age;
        public float life;
        public Color tint;
    }

    void Awake()
    {
        source = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        AgeGhosts();

        var sys = CorruptionSystem.Instance;
        float corruption = sys != null ? sys.Normalised : 0f;

        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;

        if (corruption < threshold) return;
        if (requireMovement && delta.magnitude < movementThreshold * Time.deltaTime * 60f) return;

        // Tu nguong tro len, nhip nha bong siet dan lai.
        float t = Mathf.InverseLerp(threshold, 1f, corruption);
        float interval = Mathf.Lerp(minInterval * 4f, minInterval, t);

        timer += Time.deltaTime;
        if (timer < interval) return;
        timer = 0f;

        // Bong lech nguoc huong di chuyen -> doc ra la ve duoi chan nhan vat.
        Vector3 dir = delta.sqrMagnitude > 1e-6f ? -delta.normalized : Vector3.left;
        Spawn(dir * (maxOffset * t), t);
    }

    /// <summary>Ban mot loat bong ngay lap tuc - goi khi dich chuyen bang Canh Cua.</summary>
    public void Burst(Vector3 from, Vector3 to, int count = 6)
    {
        for (int i = 0; i < count; i++)
        {
            float f = i / (float)Mathf.Max(1, count - 1);
            Spawn(Vector3.Lerp(from, to, f) - transform.position, 1f);
        }
    }

    void Spawn(Vector3 offset, float strength)
    {
        if (source.sprite == null) return;

        var g = pool.Count > 0 ? pool.Pop() : NewGhost();
        var tr = g.renderer.transform;
        tr.SetPositionAndRotation(transform.position + offset, transform.rotation);
        tr.localScale = transform.lossyScale;

        g.renderer.enabled = true;
        g.renderer.sprite = source.sprite;
        g.renderer.flipX = source.flipX;
        g.renderer.flipY = source.flipY;
        g.renderer.sortingLayerID = source.sortingLayerID;
        g.renderer.sortingOrder = source.sortingOrder + sortingOffset;

        g.age = 0f;
        g.life = lifetime;
        g.tint = new Color(tint.r, tint.g, tint.b, startAlpha * strength);
        g.renderer.color = g.tint;

        ghosts.Add(g);
    }

    Ghost NewGhost()
    {
        var go = new GameObject("Afterimage");
        go.transform.SetParent(null);   // o world space de bong dung yen khi nhan vat chay tiep
        return new Ghost { renderer = go.AddComponent<SpriteRenderer>() };
    }

    void AgeGhosts()
    {
        for (int i = ghosts.Count - 1; i >= 0; i--)
        {
            var g = ghosts[i];
            g.age += Time.deltaTime;

            float k = g.age / g.life;
            if (k >= 1f)
            {
                g.renderer.enabled = false;
                ghosts.RemoveAt(i);
                pool.Push(g);
                continue;
            }

            var c = g.tint;
            c.a = g.tint.a * (1f - k) * (1f - k);   // mo nhanh luc dau, vuot duoi cuoi
            g.renderer.color = c;
        }
    }

    void OnDestroy()
    {
        foreach (var g in ghosts) if (g.renderer) Destroy(g.renderer.gameObject);
        foreach (var g in pool) if (g.renderer) Destroy(g.renderer.gameObject);
    }
}
