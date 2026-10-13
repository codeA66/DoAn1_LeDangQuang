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
        [Tooltip("Hinh manh ve tay (HealShard), moi manh lay ngau nhien. De trong = o vuong mau tron.")]
        [SerializeField] Sprite[] shardSprites;
        [SerializeField] Color shardColor = new Color(1f, 0.97f, 0.88f, 1f);
        [Tooltip("Canh manh vo (pixel man hinh, min - max).")]
        [SerializeField] Vector2 shardSize = new Vector2(6f, 12f);
        [Tooltip("Van toc bung ra (pixel/giay, min - max).")]
        [SerializeField] Vector2 shardSpeed = new Vector2(180f, 380f);
        [SerializeField] float shardGravity = 1400f;
        [SerializeField] float shardLifetime = 0.55f;

        [Header("Mask lien lai khi hoi mau")]
        [Tooltip("Thoi gian manh vo bay lai ghep thanh mask.")]
        [SerializeField] float assembleTime = 0.2f;
        [Tooltip("Manh xuat phat cach tam mask bao nhieu lan be rong mask.")]
        [SerializeField] float assembleRadius = 1.3f;

        struct Shard
        {
            public RectTransform rect;
            public Image image;
            public Vector2 velocity;
            public float life;
            public float spin;
            // Che do ghep lai: bay tu from toi to trong assembleTime (khong trong luc).
            public bool assemble;
            public Vector3 from, to;
        }

        Shard[] shards;
        int nextShard;

        readonly List<Image> masks = new();
        int shown = -1;
        int poppingIndex = -1;
        readonly List<(int index, float timer)> assembling = new();
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
            // Mask dang nay do chua xong thi tra ve co thuong truoc khi nay mask khac.
            if (poppingIndex >= 0 && poppingIndex < masks.Count) masks[poppingIndex].transform.localScale = Vector3.one;
            // Mat mau: mask cuoi cung vua mat se nay len.
            if (shown >= 0 && current < shown)
            {
                poppingIndex = current;
                popTimer = popTime;
                for (int i = current; i < shown && i < masks.Count; i++) Shatter(masks[i].rectTransform);
            }
            // Hoi mau: manh vo bay lai ghep thanh mask, ghep xong mask moi day va nay len.
            else if (shown >= 0 && current > shown)
            {
                for (int i = shown; i < current && i < masks.Count; i++)
                {
                    Assemble(masks[i].rectTransform);
                    assembling.Add((i, assembleTime));
                }
            }
            shown = current;

            for (int i = 0; i < max; i++)
                masks[i].sprite = i < current && !IsAssembling(i) ? fullSprite : emptySprite;
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
                s.assemble = false;
                s.spin = Random.Range(-720f, 720f);
                float size = Random.Range(shardSize.x, shardSize.y);
                s.rect.sizeDelta = new Vector2(size, size);
                PickShardSprite(s.image);
                s.rect.position = mask.position + (Vector3)(Random.insideUnitCircle * mask.rect.width * 0.25f);
                s.rect.rotation = Quaternion.identity;
                s.rect.SetAsLastSibling();
                s.image.color = shardColor;
                s.image.enabled = true;
            }
        }

        void PickShardSprite(Image image)
        {
            if (shardSprites == null || shardSprites.Length == 0) return;
            image.sprite = shardSprites[Random.Range(0, shardSprites.Length)];
        }

        bool IsAssembling(int index)
        {
            foreach (var a in assembling) if (a.index == index) return true;
            return false;
        }

        /// <summary>Nguoc voi Shatter: manh hien ra quanh mask roi bay vao tam.</summary>
        void Assemble(RectTransform mask)
        {
            if (shards == null) return;
            float r = mask.rect.width * assembleRadius;
            for (int i = 0; i < shardCount; i++)
            {
                ref var s = ref shards[nextShard];
                nextShard = (nextShard + 1) % shards.Length;
                float a = (i / (float)shardCount + Random.Range(-0.05f, 0.05f)) * Mathf.PI * 2f;
                s.assemble = true;
                s.from = mask.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * r * Random.Range(0.8f, 1.1f);
                s.to = mask.position + (Vector3)(Random.insideUnitCircle * mask.rect.width * 0.15f);
                s.life = assembleTime;
                s.spin = Random.Range(-540f, 540f);
                float size = Random.Range(shardSize.x, shardSize.y);
                s.rect.sizeDelta = new Vector2(size, size);
                PickShardSprite(s.image);
                s.rect.position = s.from;
                s.rect.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                s.rect.SetAsLastSibling();
                var c = shardColor; c.a = 0f;
                s.image.color = c;
                s.image.enabled = true;
            }
        }

        void UpdateAssembling(float dt)
        {
            for (int i = assembling.Count - 1; i >= 0; i--)
            {
                var (index, timer) = assembling[i];
                timer -= dt;
                if (timer > 0f) { assembling[i] = (index, timer); continue; }
                assembling.RemoveAt(i);
                if (index >= masks.Count) continue;
                if (index < shown) masks[index].sprite = fullSprite;
                if (poppingIndex >= 0 && poppingIndex < masks.Count) masks[poppingIndex].transform.localScale = Vector3.one;
                poppingIndex = index;
                popTimer = popTime;
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
                if (s.assemble)
                {
                    // Nhanh dan khi toi gan (ease-in), hien dan.
                    float k = 1f - Mathf.Clamp01(s.life / assembleTime);
                    s.rect.position = Vector3.Lerp(s.from, s.to, k * k);
                    s.rect.Rotate(0f, 0f, s.spin * dt);
                    var ca = shardColor;
                    ca.a = Mathf.Clamp01(k * 3f);
                    s.image.color = ca;
                    if (s.life - dt <= 0f) s.assemble = false;
                    continue;
                }
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
            UpdateAssembling(Time.unscaledDeltaTime);
            if (poppingIndex < 0) return;

            popTimer -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(popTimer / popTime);
            masks[poppingIndex].transform.localScale = Vector3.one * Mathf.Lerp(1f, popScale, k);
            if (popTimer <= 0f) poppingIndex = -1;
        }
    }
}
