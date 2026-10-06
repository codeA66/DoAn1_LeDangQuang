using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Vien dan bay thang (qua cau cua doi). Lay tu <see cref="ProjectilePool"/>, khong
    /// Instantiate/Destroy: trung nguoi choi, cham dat hoac het gio thi tra ve pool.
    ///
    /// Rigidbody2D Kinematic + collider trigger: trigger voi nguoi choi (Dynamic) chay
    /// binh thuong. Dat/tuong do bang OverlapCircle vi Kinematic khong nhan va cham
    /// voi collider tinh cua Tilemap.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Min(1)] public int damage = 1;
        [Tooltip("Song toi da bao lau (giay).")]
        public float lifetime = 4f;
        [Tooltip("Chi gay sat thuong cho collider mang tag nay.")]
        public string targetTag = "Player";
        [Tooltip("Cham layer nay thi vo.")]
        public LayerMask solidLayers = 1 << 8;
        [Tooltip("Ban kinh do va cham voi dat.")]
        public float solidCheckRadius = 0.15f;
        [Tooltip("Vet mo bay theo (World space). Xoa sach khi lay ra tu pool.")]
        public ParticleSystem trail;
        [Tooltip("Phan ung cua nguoi choi khi trung (de trong = mac dinh). Quai ban dat lai moi lan ban.")]
        public HitReaction reaction;

        Rigidbody2D body;
        ProjectilePool pool;
        float timer;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
        }

        /// <summary>Goi boi pool ngay sau khi lay ra.</summary>
        public void Launch(ProjectilePool owner, Vector2 position, Vector2 velocity)
        {
            pool = owner;
            timer = lifetime;
            transform.position = position;
            body.position = position;
            body.linearVelocity = velocity;
            if (trail)
            {
                trail.Clear();
                trail.Play();
            }
        }

        void FixedUpdate()
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0f)
            {
                Despawn(burst: false);
                return;
            }

            if (Physics2D.OverlapCircle(body.position, solidCheckRadius, solidLayers))
                Despawn(burst: true);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(targetTag)) return;

            var target = other.GetComponentInParent<IDamageable>();
            if (target == null) return;

            Vector2 v = body.linearVelocity;
            target.TakeHit(new HitInfo
            {
                damage = damage,
                direction = new Vector2(v.x >= 0f ? 1f : -1f, 0f),
                knockback = 0f,
                source = gameObject,
                reaction = reaction
            });
            Despawn(burst: true);
        }

        void Despawn(bool burst)
        {
            if (pool) pool.Return(this, burst);
            else gameObject.SetActive(false);
        }
    }
}
