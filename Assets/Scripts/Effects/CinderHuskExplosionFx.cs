using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace LuHanh.Effects
{
    /// <summary>Bo VFX lua dat san, tai su dung: tu luc, bung lua va loi truy duoi.</summary>
    public sealed class CinderHuskExplosionFx : MonoBehaviour
    {
        [Tooltip("Hat hut vao loi khi tu luc.")]
        [SerializeField] ParticleSystem charge;
        [FormerlySerializedAs("ring")]
        [Tooltip("Lua pixel nhieu lop, dung mesh ve san trong Editor.")]
        [SerializeField] ParticleSystem flames;
        [Tooltip("Vo lua do cam, phat dong bo voi lop lua giua.")]
        [SerializeField] ParticleSystem outerFlames;
        [Tooltip("Loi lua nong sang, phat dong bo voi lop lua giua.")]
        [SerializeField] ParticleSystem hotFlames;
        [Tooltip("Tan lua pixel.")]
        [SerializeField] ParticleSystem sparks;
        [Tooltip("Khoi tim den sau lua.")]
        [SerializeField] ParticleSystem smoke;
        [Tooltip("Duoi lua cua loi truy duoi.")]
        [SerializeField] ParticleSystem coreTrail;
        [Tooltip("Loi than va khe lua dat san; khong sinh object khi choi.")]
        [SerializeField] GameObject core;
        [Tooltip("Khe lua chop nhanh dan, vo than van luon nhin thay.")]
        [SerializeField] Renderer coreGlow;
        [Tooltip("Den cam cua loi. De trong neu khong can den.")]
        [SerializeField] Light2D coreLight;
        [Tooltip("Vo loi than va khe lua lien mach, dung shader pixel.")]
        [SerializeField] CinderHuskFlameVisual coreVisual;
        [Tooltip("Khoi lua no va song xung kich tao san.")]
        [SerializeField] CinderHuskFlameVisual blastVisual;
        [Tooltip("Khoang cach hai dot hat tu luc, giay.")]
        [SerializeField, Min(0.01f)] float chargeInterval = 0.045f;
        [Tooltip("So ngon lua cho mot lan bung.")]
        [SerializeField, Range(16, 160)] int flameCount = 72;
        [Tooltip("So tan lua cho mot lan bung.")]
        [SerializeField, Range(4, 96)] int sparkCount = 44;
        [Tooltip("Mau nguy hiem theo bang mau project.")]
        [SerializeField] Color dangerColor = new Color(234f / 255f, 101f / 255f, 71f / 255f);

        bool charging;
        float chargeTimer;
        float chargeAngle;
        float corePhase;
        float trailTimer;

        void Awake()
        {
            HideCore();
        }

        public void BeginCharge()
        {
            StopCharge();
            charging = true;
            chargeTimer = 0f;
        }

        public void StopCharge()
        {
            charging = false;
            Clear(charge);
        }

        void Update()
        {
            if (!charging || !charge) return;
            chargeTimer -= Time.deltaTime;
            if (chargeTimer > 0f) return;
            chargeTimer = chargeInterval;
            EmitChargeStep();
        }

        public void EmitChargeStep()
        {
            if (!charge) return;
            if (charge.isStopped) charge.Play();
            chargeAngle += 2.399963f;
            for (int i = 0; i < 3; i++)
            {
                float angle = chargeAngle + i * Mathf.PI * 2f / 3f;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                charge.Emit(new ParticleSystem.EmitParams
                {
                    position = direction * 0.8f,
                    velocity = -direction * 2.2f,
                    startLifetime = 0.32f,
                    startSize = 0.11f,
                    startColor = dangerColor
                }, 1);
            }
        }

        public void PlayBlast(float radius)
        {
            StopCharge();
            HideCore();
            Clear(flames);
            Clear(outerFlames);
            Clear(hotFlames);
            Clear(sparks);
            Clear(smoke);
            Clear(coreTrail);
            float scale = Mathf.Max(0.1f, radius) / 2.5f;
            if (blastVisual) blastVisual.PlayBlast(radius);
            if (flames && !blastVisual)
            {
                flames.Play();
                if (outerFlames) outerFlames.Play();
                if (hotFlames) hotFlames.Play();
                for (int i = 0; i < flameCount; i++)
                {
                    float angle = i * Mathf.PI * 2f / flameCount + Random.Range(-0.12f, 0.12f);
                    var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                    float distance = Mathf.Sqrt(Random.value) * radius * 0.62f;
                    var particle = new ParticleSystem.EmitParams
                    {
                        position = direction * distance,
                        velocity = direction * Random.Range(0.9f, 2.1f) * scale + Vector3.up * 0.65f * scale,
                        startLifetime = Random.Range(0.32f, 0.58f),
                        startSize = Random.Range(0.55f, 1.15f) * scale,
                        startColor = dangerColor
                    };
                    if (outerFlames) outerFlames.Emit(particle, 1);
                    particle.startSize *= 0.7f;
                    particle.startColor = new Color(1f, 0.56f, 0.38f);
                    flames.Emit(particle, 1);
                    particle.startSize *= 0.5f;
                    particle.startColor = new Color(1f, 0.82f, 0.74f);
                    if (hotFlames) hotFlames.Emit(particle, 1);
                }
            }
            if (sparks)
            {
                sparks.Play();
                for (int i = 0; i < sparkCount; i++)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                    sparks.Emit(new ParticleSystem.EmitParams
                    {
                        position = direction * Random.Range(0f, radius * 0.4f),
                        velocity = direction * Random.Range(3f, 7f) * scale,
                        startLifetime = Random.Range(0.2f, 0.55f),
                        startSize = Random.Range(0.055f, 0.13f) * scale,
                        startColor = i % 3 == 0 ? new Color(1f, 0.68f, 0.53f) : dangerColor
                    }, 1);
                }
            }
            if (smoke)
            {
                smoke.Play();
                for (int i = 0; i < 24; i++)
                {
                    Vector2 direction = Random.insideUnitCircle;
                    smoke.Emit(new ParticleSystem.EmitParams
                    {
                        position = (Vector3)(direction * radius * 0.55f),
                        velocity = new Vector3(direction.x * scale, Random.Range(0.5f, 1.5f) * scale, 0),
                        startLifetime = Random.Range(0.45f, 0.75f),
                        startSize = Random.Range(0.6f, 1.1f) * scale,
                        startColor = new Color(48f / 255f, 35f / 255f, 69f / 255f, 0.7f)
                    }, 1);
                }
            }
        }

        public void BeginCore()
        {
            StopCharge();
            corePhase = 0f;
            trailTimer = 0f;
            if (core) core.SetActive(true);
            ApplyCorePulse(true, 0f);
        }

        public void UpdateCoreWarning(float progress, float deltaTime, Vector2 velocity)
        {
            corePhase += deltaTime / Mathf.Lerp(0.32f, 0.065f, Mathf.Clamp01(progress));
            ApplyCorePulse(corePhase % 1f < 0.58f, progress);
            trailTimer -= deltaTime;
            if (trailTimer > 0f) return;
            trailTimer = 0.04f;
            EmitCoreTrail(velocity);
        }

        public void PreviewCore(float progress, float phase)
        {
            if (core) core.SetActive(true);
            ApplyCorePulse(phase % 1f < 0.58f, progress);
        }

        void ApplyCorePulse(bool bright, float progress)
        {
            if (coreGlow) coreGlow.enabled = bright;
            if (coreVisual) coreVisual.SetWarning(bright);
            if (coreLight) coreLight.intensity = bright ? Mathf.Lerp(0.6f, 1.4f, progress) : 0.1f;
        }

        public void EmitCoreTrail(Vector2 velocity)
        {
            if (!coreTrail) return;
            if (coreTrail.isStopped) coreTrail.Play();
            Vector2 direction = velocity.sqrMagnitude > 0.01f ? -velocity.normalized : Vector2.down;
            for (int i = 0; i < 3; i++)
                coreTrail.Emit(new ParticleSystem.EmitParams
                {
                    position = new Vector3(Random.Range(-0.14f, 0.14f), -0.08f, 0),
                    velocity = (Vector3)direction * Random.Range(0.6f, 1.4f) + Vector3.up * 0.45f,
                    startLifetime = Random.Range(0.18f, 0.34f),
                    startSize = Random.Range(0.04f, 0.08f),
                    startColor = dangerColor
                }, 1);
        }

        public void EmitFireballFrame()
        {
            if (!flames) return;
            flames.Play();
            if (outerFlames) outerFlames.Play();
            if (hotFlames) hotFlames.Play();
            for (int i = 0; i < 6; i++)
            {
                var p = new ParticleSystem.EmitParams
                {
                    position = (Vector3)(Random.insideUnitCircle * 0.2f),
                    velocity = Vector3.up * Random.Range(0.2f, 0.6f),
                    startLifetime = Random.Range(0.12f, 0.22f),
                    startSize = Random.Range(0.4f, 0.6f),
                    startColor = dangerColor
                };
                if (outerFlames) outerFlames.Emit(p, 1);
                p.startSize *= 0.7f;
                flames.Emit(p, 1);
                p.startSize *= 0.5f;
                if (hotFlames) hotFlames.Emit(p, 1);
            }
        }
        public void EmitWallImpact(Vector2 worldNormal)
        {
            if (!sparks) return;
            sparks.Play();
            Vector2 normal = transform.InverseTransformVector(worldNormal).normalized;
            Vector2 tangent = new Vector2(-normal.y, normal.x);
            for (int i = 0; i < 24; i++)
            {
                float angle = Random.Range(-1.1f, 1.1f);
                Vector2 direction = normal * Mathf.Cos(angle) + tangent * Mathf.Sin(angle);
                sparks.Emit(new ParticleSystem.EmitParams
                {
                    position = (Vector3)(normal * 0.08f),
                    velocity = (Vector3)(direction * Random.Range(3f, 7f)),
                    startLifetime = Random.Range(0.18f, 0.42f),
                    startSize = Random.Range(0.035f, 0.07f),
                    startColor = dangerColor
                }, 1);
            }
        }

        public void HideCore()
        {
            if (core) core.SetActive(false);
            if (coreLight) coreLight.intensity = 0f;
        }

        static void Clear(ParticleSystem system)
        {
            if (system) system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        void OnDisable() => ResetEffects();

        public void ResetEffects()
        {
            if (blastVisual) blastVisual.ClearBlast();
            StopCharge();
            HideCore();
            Clear(flames);
            Clear(outerFlames);
            Clear(hotFlames);
            Clear(sparks);
            Clear(smoke);
            Clear(coreTrail);
        }
    }
}
