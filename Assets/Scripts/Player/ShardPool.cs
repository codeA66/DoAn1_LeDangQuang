using LuHanh.Core;
using LuHanh.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LuHanh.Player
{
    /// <summary>
    /// Tien Shards: quai chet bung ra cac manh Shard, bay hut ve nguoi choi va cong vao <see cref="ShardWallet"/>.
    /// Nguoi choi chet: toan bo Shards roi lai thanh mot tui o cho an toan gan nhat (kieu Hollow Knight);
    /// cham tui thi lay lai, chet lan nua truoc khi lay thi tui cu mat han. Tui duoc luu trong file save.
    /// Kem bo dem Shards o goc tren ben phai man hinh. Dat trong LevelEssentials/GameSystems.
    /// </summary>
    public class ShardPool : MonoBehaviour
    {
        static ShardPool instance;

        [Header("Hinh (tam, cho art)")]
        [SerializeField] Sprite shardSprite;
        [SerializeField] Color shardColor = new Color(0.75f, 0.85f, 0.95f, 1f);   // #BFD8F2
        [SerializeField] float shardScale = 0.6f;
        [SerializeField] Sprite cacheSprite;
        [SerializeField] Color cacheColor = new Color(0.29f, 0.5f, 0.76f, 1f);    // #4A7FC1
        [SerializeField] float cacheScale = 1.6f;
        [SerializeField] int sortingOrder = 25;
        [Min(1)] [SerializeField] int poolSize = 40;

        [Header("Chuyen dong")]
        [SerializeField] Vector2 burstSpeed = new Vector2(3f, 7f);
        [SerializeField] float burstTime = 0.4f;
        [SerializeField] float homingAccel = 60f;
        [SerializeField] float maxSpeed = 24f;
        [SerializeField] float collectDistance = 0.5f;
        [SerializeField] float chestHeight = 0.9f;

        [Header("Tui tien khi chet")]
        [SerializeField] float cachePickupRadius = 1.2f;
        [SerializeField] float cacheHeight = 0.8f;

        [Header("Bo dem (goc tren ben phai)")]
        [SerializeField] Font font;
        [SerializeField] int fontSize = 40;
        [SerializeField] Vector2 counterOffset = new Vector2(-60f, -50f);

        struct Shard { public Transform transform; public Vector2 velocity; public float age; public bool alive; }

        Shard[] shards;
        Transform player;
        Transform cache;
        Text counter;
        float saveTimer = -1f;

        void Awake()
        {
            instance = this;
            shards = new Shard[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject("Shard");
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * shardScale;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = shardSprite; sr.color = shardColor; sr.sortingOrder = sortingOrder;
                go.SetActive(false);
                shards[i].transform = go.transform;
            }
            BuildCounter();
        }

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p) player = p.transform;
            var d = SaveGame.Data;
            if (d.lostShards > 0 && d.lostScene == SceneManager.GetActiveScene().name) ShowCache(d.lostPosition);
            ShardWallet.Changed += OnChanged;
            OnChanged(ShardWallet.Count);
        }

        void OnDestroy()
        {
            ShardWallet.Changed -= OnChanged;
            if (instance == this) instance = null;
            if (saveTimer >= 0f) SaveGame.Save();
        }

        /// <summary>Quai chet: tha <paramref name="count"/> Shards tai <paramref name="position"/>.</summary>
        public static void Spawn(Vector2 position, int count)
        {
            if (instance && count > 0) instance.SpawnInternal(position, count);
        }

        /// <summary>Nguoi choi chet: roi het Shards thanh tui tai <paramref name="position"/> (tui cu neu con thi mat).</summary>
        public static void DropOnDeath(Vector2 position)
        {
            int n = ShardWallet.TakeAll();
            var d = SaveGame.Data;
            if (n <= 0) { d.lostShards = 0; if (instance) instance.HideCache(); SaveGame.Save(); return; }
            d.lostShards = n;
            d.lostScene = SceneManager.GetActiveScene().name;
            d.lostPosition = position;
            SaveGame.Save();
            if (instance) instance.ShowCache(position);
        }

        void SpawnInternal(Vector2 position, int count)
        {
            for (int n = 0; n < count; n++)
            {
                int i = -1;
                for (int k = 0; k < shards.Length; k++) if (!shards[k].alive) { i = k; break; }
                if (i < 0) { ShardWallet.Add(count - n); MarkDirty(); return; }   // het pool: cong thang
                float ang = Random.Range(25f, 155f) * Mathf.Deg2Rad;
                shards[i].velocity = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * Random.Range(burstSpeed.x, burstSpeed.y);
                shards[i].age = 0f;
                shards[i].alive = true;
                shards[i].transform.position = position;
                shards[i].transform.gameObject.SetActive(true);
            }
        }

        void Update()
        {
            if (!player) return;
            float dt = Time.deltaTime;
            Vector2 target = (Vector2)player.position + Vector2.up * chestHeight;
            for (int i = 0; i < shards.Length; i++)
            {
                if (!shards[i].alive) continue;
                ref var s = ref shards[i];
                s.age += dt;
                Vector2 pos = s.transform.position;
                if (s.age < burstTime) s.velocity *= 1f - 4f * dt;
                else
                {
                    Vector2 to = target - pos;
                    float dist = to.magnitude;
                    if (dist < collectDistance)
                    {
                        s.alive = false;
                        s.transform.gameObject.SetActive(false);
                        ShardWallet.Add(1);
                        Sfx.Play(SfxId.ShardPickup, 0.5f);
                        MarkDirty();
                        continue;
                    }
                    s.velocity = Vector2.MoveTowards(s.velocity, to / dist * maxSpeed, homingAccel * (1f + s.age) * dt);
                }
                s.transform.position = pos + s.velocity * dt;
            }

            // Tui tien: nhap nho, cham la lay lai.
            if (cache)
            {
                var d = SaveGame.Data;
                cache.position = d.lostPosition + Vector2.up * (cacheHeight + Mathf.Sin(Time.time * 2f) * 0.15f);
                if (((Vector2)player.position + Vector2.up * chestHeight - (Vector2)cache.position).sqrMagnitude < cachePickupRadius * cachePickupRadius)
                {
                    int n = d.lostShards;
                    d.lostShards = 0;
                    HideCache();
                    SpawnInternal(cache.position, Mathf.Min(n, poolSize));
                    if (n > poolSize) ShardWallet.Add(n - poolSize);
                    SaveGame.Save();
                }
            }

            // Gom lan luu: toi da moi giay mot lan khi tien doi.
            if (saveTimer >= 0f && (saveTimer -= dt) < 0f) SaveGame.Save();
        }

        void MarkDirty() { if (saveTimer < 0f) saveTimer = 1f; }

        void ShowCache(Vector2 position)
        {
            if (!cache)
            {
                var go = new GameObject("ShardCache");
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * cacheScale;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = cacheSprite ? cacheSprite : shardSprite; sr.color = cacheColor; sr.sortingOrder = sortingOrder;
                cache = go.transform;
            }
            cache.gameObject.SetActive(true);
            cache.position = position + Vector2.up * cacheHeight;
        }

        void HideCache() { if (cache) cache.gameObject.SetActive(false); }

        void BuildCounter()
        {
            var go = new GameObject("ShardCounter", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(go, 40);
            counter = UIBuild.MakeText("Count", go.transform, font, fontSize, new Color(0.95f, 0.96f, 0.97f));
            var rt = counter.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = counterOffset;
            counter.alignment = TextAnchor.UpperRight;
            counter.gameObject.AddComponent<Outline>().effectColor = new Color(0.05f, 0.04f, 0.08f, 0.9f);
        }

        void OnChanged(int value) { if (counter) counter.text = "Shards  " + value; }
    }
}
