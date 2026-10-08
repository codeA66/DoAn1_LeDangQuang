using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Hat nang luong Shadow roi ra khi quai chet: bung ra mot thoang roi bay hut ve nguc nguoi choi,
    /// cham toi thi cong vao <see cref="PlayerShadow"/>.
    /// Pool tao san trong Awake (dat trong LevelEssentials/GameSystems), khong Instantiate khi choi.
    /// Nguoi choi chua mo khoa Shadow thi khong roi hat.
    /// </summary>
    public class SoulOrbPool : MonoBehaviour
    {
        static SoulOrbPool instance;

        [SerializeField] Sprite orbSprite;
        [Tooltip("Quang sang mo quanh hat (vd halo_radial). De trong thi bo qua.")]
        [SerializeField] Sprite glowSprite;
        [SerializeField] Color orbColor = new Color(0.72f, 1f, 0.94f, 1f);        // #B8FFF0
        [SerializeField] Color glowColor = new Color(0.27f, 0.86f, 0.77f, 0.55f); // #45DCC4
        [SerializeField] float glowScale = 0.9f;
        [SerializeField] int sortingOrder = 25;
        [Min(1)] [SerializeField] int poolSize = 24;

        [Header("Chuyen dong")]
        [Tooltip("Van toc bung ra luc dau (min, max).")]
        [SerializeField] Vector2 burstSpeed = new Vector2(3f, 6f);
        [Tooltip("Giay bung ra truoc khi bat dau bay ve nguoi choi.")]
        [SerializeField] float burstTime = 0.35f;
        [Tooltip("Gia toc hut ve nguoi choi (unit/s^2) va toc do toi da.")]
        [SerializeField] float homingAccel = 60f;
        [SerializeField] float maxSpeed = 22f;
        [Tooltip("Cach nguc nguoi choi bao nhieu thi tinh la da nhat.")]
        [SerializeField] float collectDistance = 0.5f;
        [Tooltip("Nguc nguoi choi, tinh tu chan.")]
        [SerializeField] float chestHeight = 0.9f;

        struct Orb
        {
            public Transform transform;
            public Vector2 velocity;
            public float age;
            public bool alive;
        }

        Orb[] orbs;
        PlayerShadow shadow;

        void Awake()
        {
            instance = this;
            orbs = new Orb[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject("SoulOrb");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = orbSprite;
                sr.color = orbColor;
                sr.sortingOrder = sortingOrder;
                if (glowSprite)
                {
                    var glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
                    glow.transform.SetParent(go.transform, false);
                    glow.transform.localScale = Vector3.one * glowScale;
                    glow.sprite = glowSprite;
                    glow.color = glowColor;
                    glow.sortingOrder = sortingOrder - 1;
                }
                go.SetActive(false);
                orbs[i].transform = go.transform;
            }
        }

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p) shadow = p.GetComponent<PlayerShadow>();
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        /// <summary>Tha <paramref name="count"/> hat tai <paramref name="position"/>.</summary>
        public static void Spawn(Vector2 position, int count)
        {
            if (!instance || count <= 0) return;
            instance.SpawnInternal(position, count);
        }

        void SpawnInternal(Vector2 position, int count)
        {
            if (!shadow || !shadow.IsUnlocked) return;
            for (int n = 0; n < count; n++)
            {
                int i = FindFree();
                if (i < 0) return;
                float ang = Random.Range(20f, 160f) * Mathf.Deg2Rad;   // bung len tren
                orbs[i].velocity = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * Random.Range(burstSpeed.x, burstSpeed.y);
                orbs[i].age = 0f;
                orbs[i].alive = true;
                orbs[i].transform.position = position;
                orbs[i].transform.gameObject.SetActive(true);
            }
        }

        int FindFree()
        {
            for (int i = 0; i < orbs.Length; i++)
                if (!orbs[i].alive) return i;
            return -1;
        }

        void Update()
        {
            if (!shadow) return;
            float dt = Time.deltaTime;
            Vector2 target = (Vector2)shadow.transform.position + Vector2.up * chestHeight;

            for (int i = 0; i < orbs.Length; i++)
            {
                if (!orbs[i].alive) continue;
                ref var o = ref orbs[i];
                o.age += dt;
                Vector2 pos = o.transform.position;

                if (o.age < burstTime)
                {
                    o.velocity *= 1f - 4f * dt;   // cham dan
                }
                else
                {
                    Vector2 to = target - pos;
                    float dist = to.magnitude;
                    if (dist < collectDistance)
                    {
                        Collect(i);
                        continue;
                    }
                    // Lai van toc ve phia nguoi choi, cang lau cang manh de khong quay vong mai.
                    float steer = homingAccel * (1f + o.age);
                    o.velocity = Vector2.MoveTowards(o.velocity, to / dist * maxSpeed, steer * dt);
                }
                o.transform.position = pos + o.velocity * dt;
            }
        }

        void Collect(int i)
        {
            orbs[i].alive = false;
            orbs[i].transform.gameObject.SetActive(false);
            shadow.AddEnergy(shadow.Config.energyPerOrb);
        }
    }
}
