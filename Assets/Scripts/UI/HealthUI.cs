using System.Collections.Generic;
using LuHanh.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace LuHanh.UI
{
    /// <summary>
    /// Hang mask mau kieu Hollow Knight o goc man hinh. Tao du so mask mot lan luc dau,
    /// sau do chi doi sprite khi mau doi (nghe su kien, khong kiem tra moi frame).
    /// Mask vua mat thi nay to len roi thu ve - cam giac "vo".
    /// </summary>
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] Health health;
        [Tooltip("Image mau (tat san); duoc nhan ban thanh tung mask.")]
        [SerializeField] Image maskTemplate;
        [SerializeField] Sprite fullSprite;
        [SerializeField] Sprite emptySprite;
        [SerializeField] float popScale = 1.4f;
        [SerializeField] float popTime = 0.2f;

        [Header("Mask vo khi mat mau")]
        [Tooltip("So manh vo moi lan mat 1 mask.")]
        [SerializeField] int shardCount = 7;
        [SerializeField] Color shardColor = new Color(1f, 0.97f, 0.88f, 1f);
        [Tooltip("Canh manh vo (pixel man hinh, min - max).")]
        [SerializeField] Vector2 shardSize = new Vector2(6f, 12f);
        [Tooltip("Van toc bung ra (pixel/giay, min - max).")]
        [SerializeField] Vector2 shardSpeed = new Vector2(180f, 380f);
        [SerializeField] float shardGravity = 1400f;
        [SerializeField] float shardLifetime = 0.55f;

        struct Shard
        {
            public RectTransform rect;
            public Image image;
            public Vector2 velocity;
            public float life;
            public float spin;
        }

        Shard[] shards;
        int nextShard;

        readonly List<Image> masks = new();
        int shown = -1;
        int poppingIndex = -1;
        float popTimer;

        void Start()
        {
            // HUD nam trong prefab LevelEssentials khong giu duoc tham chieu toi Player trong scene.
            if (!health)
            {
                var player = GameObject.FindWithTag("Player");
                if (player) health = player.GetComponent<Health>();
            }
            if (!health || !maskTemplate) return;
            maskTemplate.gameObject.SetActive(false);
            CreateShards();
            Build(health.Max);
            Refresh(health.Current, health.Max);
            health.Changed += Refresh;
        }

        void OnDestroy()
        {
            if (health) health.Changed -= Refresh;
        }

        void Build(int count)
        {
            while (masks.Count < count)
            {
                var img = Instantiate(maskTemplate, maskTemplate.transform.parent);
                img.gameObject.SetActive(true);
                masks.Add(img);
            }
            for (int i = 0; i < masks.Count; i++) masks[i].gameObject.SetActive(i < count);
        }

        void Refresh(int current, int max)
        {
            Build(max);
            // Mat mau: mask cuoi cung vua mat se nay len.
            if (shown >= 0 && current < shown)
            {
                poppingIndex = current;
                popTimer = popTime;
                for (int i = current; i < shown && i < masks.Count; i++) Shatter(masks[i].rectTransform);
            }
            shown = current;

            for (int i = 0; i < max; i++)
                masks[i].sprite = i < current ? fullSprite : emptySprite;
        }

        // Manh vo tao san 3 dot, dat thang duoi Canvas (khong nam trong HorizontalLayoutGroup).
        void CreateShards()
        {
            var canvas = GetComponentInParent<Canvas>();
            var parent = canvas ? canvas.transform : transform;
            shards = new Shard[Mathf.Max(1, shardCount * 3)];
            for (int i = 0; i < shards.Length; i++)
            {
                var go = new GameObject("MaskShard", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(parent, false);
                var img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.enabled = false;
                shards[i] = new Shard { rect = (RectTransform)go.transform, image = img };
            }
        }

        void Shatter(RectTransform mask)
        {
            if (shards == null) return;
            for (int i = 0; i < shardCount; i++)
            {
                ref var s = ref shards[nextShard];
                nextShard = (nextShard + 1) % shards.Length;
                // Bung ra moi phia, thien ve phia tren de manh vang len roi roi xuong.
                float a = Random.Range(20f, 160f) * Mathf.Deg2Rad;
                if (Random.value < 0.3f) a = Random.Range(-30f, 210f) * Mathf.Deg2Rad;
                s.velocity = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Random.Range(shardSpeed.x, shardSpeed.y);
                s.life = shardLifetime;
                s.spin = Random.Range(-720f, 720f);
                float size = Random.Range(shardSize.x, shardSize.y);
                s.rect.sizeDelta = new Vector2(size, size);
                s.rect.position = mask.position + (Vector3)(Random.insideUnitCircle * mask.rect.width * 0.25f);
                s.rect.rotation = Quaternion.identity;
                s.rect.SetAsLastSibling();
                s.image.color = shardColor;
                s.image.enabled = true;
            }
        }

        void UpdateShards(float dt)
        {
            if (shards == null) return;
            for (int i = 0; i < shards.Length; i++)
            {
                ref var s = ref shards[i];
                if (s.life <= 0f) continue;
                s.life -= dt;
                if (s.life <= 0f) { s.image.enabled = false; continue; }
                s.velocity.y -= shardGravity * dt;
                s.rect.position += (Vector3)(s.velocity * dt);
                s.rect.Rotate(0f, 0f, s.spin * dt);
                var c = shardColor;
                c.a = Mathf.Clamp01(s.life / (shardLifetime * 0.5f));
                s.image.color = c;
            }
        }

        void Update()
        {
            // Dung thoi gian thuc: van chay luc game dang khung hinh.
            UpdateShards(Time.unscaledDeltaTime);
            if (poppingIndex < 0) return;

            popTimer -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(popTimer / popTime);
            masks[poppingIndex].transform.localScale = Vector3.one * Mathf.Lerp(1f, popScale, k);
            if (popTimer <= 0f) poppingIndex = -1;
        }
    }
}
