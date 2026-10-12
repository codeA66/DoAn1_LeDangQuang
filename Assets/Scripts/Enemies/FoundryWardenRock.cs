using LuHanh.Combat;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>Một viên đá đặt sẵn: parabol, quét va chạm, vỡ không gây damage lan.</summary>
    public sealed class FoundryWardenRock : MonoBehaviour
    {
        [Tooltip("Sprite đá đặt sẵn, tắt khi ở trong pool.")]
        [SerializeField] SpriteRenderer visual;
        [Tooltip("Bụi bay World space theo viên đá, đặt sẵn.")]
        [SerializeField] ParticleSystem trail;
        [Tooltip("Bụi và mảnh đá khi chạm, đặt sẵn; không collider/damage.")]
        [SerializeField] ParticleSystem[] impact;
        [Tooltip("Collider tròn trigger của đá; radius khớp đường kính trong config.")]
        [SerializeField] CircleCollider2D bodyCollider;
        [Tooltip("Vòng xung kích trang trí đặt sẵn tại điểm chạm; không gây sát thương.")]
        [SerializeField] MeshRenderer impactRing;
        [Tooltip("Thời gian vòng xung kích tan, giây; chỉ đổi hình ảnh.")]
        [SerializeField, Min(0.01f)] float impactDuration = 0.3f;
        readonly RaycastHit2D[] hits = new RaycastHit2D[32];
        FoundryWardenConfig config;
        Collider2D targetBody;
        IDamageable target;
        GameObject owner;
        ContactFilter2D filter;
        Vector2 start, destination, position;
        float elapsed;
        bool flying;
        float impactStarted = -1f;
        MaterialPropertyBlock impactProperties;
        static readonly int ColorId = Shader.PropertyToID("_BaseColor");
        static readonly int KindId = Shader.PropertyToID("_FxKind");
        static readonly int ProgressId = Shader.PropertyToID("_Progress");
        static readonly int DirectionId = Shader.PropertyToID("_Direction");

        // Giữ đích cố định. Gia tốc y không đổi vì đây là đa thức bậc hai theo thời gian.
        public static Vector2 ArcPoint(Vector2 from, Vector2 to, float progress, float height) =>
            Vector2.LerpUnclamped(from, to, progress) + Vector2.up * (4f * height * progress * (1f - progress));

        public void Launch(FoundryWardenConfig settings, Vector2 from, Collider2D player,
            IDamageable damageable, GameObject source)
        {
            Clear();
            config = settings;
            targetBody = player;
            target = damageable;
            owner = source;
            start = position = from;
            destination = player.bounds.center;
            elapsed = 0f;
            filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(config.groundLayers.value | (1 << player.gameObject.layer));
            flying = true;
            if (bodyCollider) { bodyCollider.radius = config.RockDiameter * 0.5f; bodyCollider.enabled = true; }
            transform.position = new Vector3(from.x, from.y, 0f);
            if (visual)
            {
                visual.enabled = true;
                float diameter = Mathf.Max(visual.sprite.bounds.size.x, visual.sprite.bounds.size.y);
                visual.transform.localScale = Vector3.one * (config.RockDiameter / diameter);
            }
            if (trail) trail.Play();
        }

        void FixedUpdate()
        {
            if (!flying) return;
            elapsed += Time.fixedDeltaTime;
            Vector2 next = ArcPoint(start, destination, elapsed / config.RockFlightDuration, config.RockArcHeight);
            Vector2 delta = next - position;
            float distance = delta.magnitude;
            int count = Physics2D.CircleCast(position, config.RockDiameter * 0.5f,
                distance > 0f ? delta / distance : Vector2.down, filter, hits, distance);
            int nearest = -1;
            float best = float.PositiveInfinity;
            bool playerHit = false;
            for (int i = 0; i < count; i++)
            {
                var collider = hits[i].collider;
                if (!collider) continue;
                bool isPlayer = targetBody && targetBody.enabled && targetBody.gameObject.activeInHierarchy &&
                    (collider == targetBody || (targetBody.attachedRigidbody && collider.attachedRigidbody == targetBody.attachedRigidbody));
                bool isGround = (config.groundLayers.value & (1 << collider.gameObject.layer)) != 0;
                if ((!isPlayer && !isGround) || hits[i].distance >= best) continue;
                nearest = i;
                best = hits[i].distance;
                playerHit = isPlayer;
            }
            if (nearest >= 0)
            {
                position = hits[nearest].centroid;
                transform.position = new Vector3(position.x, position.y, 0f);
                // Tắt trạng thái trước TakeHit vì Player có thể kích hoạt reset ngay trong lời gọi.
                flying = false;
                if (bodyCollider) bodyCollider.enabled = false;
                if (visual) visual.enabled = false;
                if (trail) trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                PlayImpact(hits[nearest].point);
                if (playerHit && target != null)
                    target.TakeHit(new HitInfo { damage = config.AttackDamage, source = owner,
                        direction = delta.normalized, reaction = config.hitReaction, knockback = 0f });
                return;
            }
            position = next;
            transform.position = new Vector3(next.x, next.y, 0f);
            // Qua mốc đích mà Player đã né, đá tiếp tục rơi cùng parabol cho tới khi chạm Ground.
        }

        public void Clear()
        {
            flying = false;
            impactStarted = -1f;
            if (bodyCollider) bodyCollider.enabled = false;
            if (impactRing) impactRing.enabled = false;
            if (visual) visual.enabled = false;
            if (trail) trail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (impact != null) foreach (var ps in impact) if (ps) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void PlayImpact(Vector2 point)
        {
            impactStarted = Time.time;
            if (impact != null) for (int i = 0; i < impact.Length; i++)
            {
                var ps = impact[i];
                if (!ps) continue;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.transform.position = new Vector3(point.x, point.y, 0f);
                var emission = ps.emission;
                emission.enabled = false;
                ps.Play();
                bool chunks = i > 0;
                var particle = new ParticleSystem.EmitParams
                {
                    position = new Vector3(point.x, point.y, 0f),
                    applyShapeToPosition = false,
                    startColor = chunks ? new Color(.61f, .51f, .58f, 1f) : new Color(.5f, .43f, .56f, .6f)
                };
                for (int n = 0; n < (chunks ? 20 : 14); n++)
                {
                    float angle = Random.Range(0f, Mathf.PI);
                    float speed = Random.Range(chunks ? 2f : .7f, chunks ? 5f : 2f);
                    particle.velocity = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, 0f);
                    particle.startSize = Random.Range(chunks ? .05f : .12f, chunks ? .14f : .32f);
                    particle.startLifetime = Random.Range(.25f, chunks ? .55f : .45f);
                    ps.Emit(particle, 1);
                }
            }
            if (impactRing)
            {
                impactRing.transform.position = new Vector3(point.x, point.y, -0.1f);
                impactRing.transform.rotation = Quaternion.identity;
                impactRing.transform.localScale = new Vector3(3f, 1.5f, 1f);
                impactRing.enabled = true;
                DrawImpact(0f);
            }
        }
        void Update()
        {
            if (impactStarted < 0f || !impactRing) return;
            float progress = (Time.time - impactStarted) / impactDuration;
            if (progress >= 1f) { impactRing.enabled = false; impactStarted = -1f; return; }
            DrawImpact(progress);
        }
        void DrawImpact(float progress)
        {
            impactProperties ??= new MaterialPropertyBlock();
            impactProperties.SetColor(ColorId, new Color(0.918f, 0.396f, 0.278f, 1f - progress));
            impactProperties.SetFloat(KindId, 3f);
            impactProperties.SetFloat(ProgressId, progress);
            impactProperties.SetFloat(DirectionId, 1f);
            impactRing.SetPropertyBlock(impactProperties);
        }
        void OnDisable() => Clear();
    }
}
