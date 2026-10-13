using System.Collections.Generic;
using LuHanh.Effects;
using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Qua cau nang luong cua nguoi choi: bay ngang, xuyen qua quai (moi muc tieu trung mot lan),
    /// trung dat / tuong thi no va tan. Hinh tu lat frame (khong can Animator).
    /// Ban Shadow: to hon (scale) va dung bo frame rieng, sat thuong nhan theo Shadow.
    /// </summary>
    public class OrbProjectile : MonoBehaviour
    {
        [System.Serializable]
        public class Look
        {
            public Sprite[] fly;
            public Sprite[] hit;
            [Tooltip("Ban kinh vung trung (unit).")]
            public float radius = 0.3f;
            [Tooltip("Vet duoi dat sau qua cau, tinh tu tam loi (unit, huong ban sang phai).")]
            public Vector2 trailOffset = new Vector2(-1f, 0f);
            [Tooltip("Hieu ung no tai cho moi muc tieu bi trung (prefab FrameAnimation, tu huy).")]
            public FrameAnimation impact;
        }

        [SerializeField] SpriteRenderer sprite;
        [Tooltip("Vet duoi ve tay (OrbTrail), lap lai trong luc bay; no thi tat.")]
        [SerializeField] FrameAnimation trail;
        [SerializeField] Look normal = new Look();
        [SerializeField] Look shadow = new Look { radius = 0.6f };
        [SerializeField] float speed = 16f;
        [SerializeField] float maxDistance = 11f;
        [SerializeField] int damage = 2;
        [SerializeField] float knockback = 6f;
        [SerializeField] float fps = 14f;
        [Tooltip("So khung/giay luc no (bat nhanh roi tan).")]
        [SerializeField] float hitFps = 20f;
        [Tooltip("Layer cua quai / vat danh duoc (IDamageable).")]
        [SerializeField] LayerMask hitLayers;
        [Tooltip("Layer dat / tuong: cham la no.")]
        [SerializeField] LayerMask groundLayers;

        readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
        readonly Collider2D[] overlaps = new Collider2D[8];
        Look look;
        Vector2 direction;
        float travelled, frameTimer, damageScale;
        int frame;
        bool exploding;
        GameObject owner;

        /// <summary>Ban ra theo huong ngang <paramref name="dirX"/> (-1 / 1).</summary>
        public void Launch(float dirX, bool shadowForm, float damageMultiplier, GameObject source)
        {
            look = shadowForm ? shadow : normal;
            direction = new Vector2(Mathf.Sign(dirX), 0f);
            damageScale = damageMultiplier;
            owner = source;
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * direction.x;
            transform.localScale = s;
            if (sprite && look.fly.Length > 0) sprite.sprite = look.fly[0];
            if (trail)
            {
                trail.transform.localPosition = look.trailOffset;
                trail.Play();
            }
        }

        void Update()
        {
            if (look == null || !sprite) return;
            var frames = exploding ? look.hit : look.fly;
            if (frames == null || frames.Length == 0) { if (exploding) Destroy(gameObject); return; }
            float rate = exploding ? hitFps : fps;
            frameTimer += Time.deltaTime;
            if (frameTimer < 1f / rate) return;
            frameTimer -= 1f / rate;
            frame++;
            if (exploding && frame >= frames.Length) { Destroy(gameObject); return; }
            sprite.sprite = frames[frame % frames.Length];
        }

        void FixedUpdate()
        {
            if (look == null || exploding) return;
            float step = speed * Time.fixedDeltaTime;
            Vector2 pos = transform.position;

            // Cham dat / tuong tren quang duong buoc nay thi no tai cho do.
            var wall = Physics2D.CircleCast(pos, look.radius * 0.5f, direction, step, groundLayers);
            if (wall.collider && !wall.collider.usedByEffector)
            {
                transform.position = wall.centroid;
                Explode();
                return;
            }

            pos += direction * step;
            transform.position = pos;
            travelled += step;
            DamageOverlaps(pos);
            if (travelled >= maxDistance) Explode();
        }

        void DamageOverlaps(Vector2 pos)
        {
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = hitLayers, useTriggers = true };
            int n = Physics2D.OverlapCircle(pos, look.radius, filter, overlaps);
            for (int i = 0; i < n; i++)
            {
                var target = overlaps[i].GetComponentInParent<IDamageable>();
                if (target == null || !hitTargets.Add(target)) continue;
                SpawnImpact(overlaps[i].bounds.center);
                target.TakeHit(new HitInfo
                {
                    damage = Mathf.RoundToInt(damage * damageScale),
                    direction = direction,
                    knockback = knockback,
                    source = owner
                });
            }
        }

        void SpawnImpact(Vector2 at)
        {
            if (!look.impact) return;
            var fx = Instantiate(look.impact, at, Quaternion.identity);
            var sc = fx.transform.localScale;
            sc.x = Mathf.Abs(sc.x) * direction.x;
            fx.transform.localScale = sc;
            fx.Play();
            Destroy(fx.gameObject, fx.Duration + 0.05f);
        }

        void Explode()
        {
            exploding = true;
            if (trail) trail.Stop();
            frame = 0;
            frameTimer = 0f;
            if (sprite && look.hit != null && look.hit.Length > 0) sprite.sprite = look.hit[0];
            else Destroy(gameObject);
        }
    }
}
