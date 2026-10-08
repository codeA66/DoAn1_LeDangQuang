using LuHanh.Combat;
using LuHanh.Effects;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>Cau lua tao san trong prefab; quet va cham, tai su dung, khong sinh object khi choi.</summary>
    public sealed class CinderHuskFireball : MonoBehaviour
    {
        [Tooltip("VFX lua dat san trong object nay.")]
        [SerializeField] CinderHuskExplosionFx effects;
        [Tooltip("Khoi cau lua va duoi lua lien mach; duoi nam nguoc huong bay.")]
        [SerializeField] CinderHuskFlameVisual flameVisual;
        readonly RaycastHit2D[] hits = new RaycastHit2D[16];
        CinderHuskConfig config;
        Transform target;
        GameObject source;
        Vector2 direction;
        Vector3 worldPosition;
        float elapsed, emissionTimer;
        bool flying;
        bool impacting;
        float impactTimer;

        public bool Available => !flying && !impacting;

        public void Launch(CinderHuskConfig settings, Transform player, GameObject owner,
            Vector2 origin, float facingSign)
        {
            config = settings;
            target = player;
            source = owner;
            direction = facingSign >= 0f ? Vector2.right : Vector2.left;
            elapsed = emissionTimer = 0f;
            worldPosition = origin;
            transform.position = worldPosition;
            flying = true;
            impacting = false;
            gameObject.SetActive(true);
            if (flameVisual) flameVisual.SetDirection(facingSign);
            if (effects) { effects.ResetEffects(); effects.EmitFireballFrame(); }
        }

        void Update()
        {
            if (impacting)
            {
                impactTimer -= Time.deltaTime;
                if (impactTimer <= 0f) Cancel();
                return;
            }
            if (!flying || Time.deltaTime <= 0f) return;
            elapsed += Time.deltaTime;
            if (elapsed >= config.FireballLifetime) { Cancel(); return; }
            float distance = config.FireballSpeed * Time.deltaTime;
            var filter = new ContactFilter2D();
            filter.SetLayerMask(config.groundLayers.value | (target ? 1 << target.gameObject.layer : 0));
            filter.useTriggers = true;
            int count = Physics2D.CircleCast(worldPosition, config.FireballRadius,
                direction, filter, hits, distance);
            float nearest = float.MaxValue;
            Collider2D contact = null;
            RaycastHit2D contactHit = default;
            for (int i = 0; i < count; i++)
            {
                if (!hits[i].collider || hits[i].distance >= nearest) continue;
                contact = hits[i].collider;
                nearest = hits[i].distance;
                contactHit = hits[i];
            }
            if (contact)
            {
                flying = false; // No second hit if TakeHit synchronously resets the level.
                if (target && (contact.transform == target || contact.transform.IsChildOf(target)))
                {
                    var receiver = target.GetComponent<IDamageable>();
                    if (receiver == null) receiver = target.GetComponentInChildren<IDamageable>();
                    receiver?.TakeHit(new HitInfo { damage = config.FireballDamage, direction = direction,
                        knockback = 0f, source = source, reaction = config.hitReaction });
                }
                else if (flameVisual)
                {
                    Vector2 normal = contactHit.normal.sqrMagnitude > 0.01f ? contactHit.normal : -direction;
                    worldPosition = contactHit.point + normal * 0.03f;
                    transform.position = worldPosition;
                    if (effects) { effects.ResetEffects(); effects.EmitWallImpact(normal); }
                    impacting = true;
                    impactTimer = config.WallImpactDuration;
                    flameVisual.PlayWallImpact(normal, config.FireballRadius * config.WallImpactScale,
                        config.WallImpactDuration);
                    return;
                }
                Cancel();
                return;
            }
            worldPosition += (Vector3)(direction * distance);
            transform.position = worldPosition;
            emissionTimer -= Time.deltaTime;
            if (effects && emissionTimer <= 0f)
            {
                emissionTimer = 0.045f;
                effects.EmitFireballFrame();
                effects.EmitCoreTrail(effects.transform.InverseTransformVector(direction * config.FireballSpeed));
            }
        }

        void LateUpdate()
        {
            if (!flying && !impacting) return;
            // The owner flips to face Player; an already fired ball keeps its world path.
            transform.position = worldPosition;
            if (transform.parent)
                transform.localScale = new Vector3(Mathf.Sign(transform.parent.lossyScale.x), 1f, 1f);
        }

        public void Cancel()
        {
            flying = false;
            impacting = false;
            if (effects) effects.ResetEffects();
            gameObject.SetActive(false);
        }
        void OnDisable() { flying = impacting = false; }
    }
}
