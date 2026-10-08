using LuHanh.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Player
{
    /// <summary>
    /// Hieu ung dang Shadow, chi nghe su kien cua <see cref="PlayerShadow"/>, khong dung vao logic.
    /// 1. Tu luc (dang giu phim): hat sang bi hut vao, vet sang doc do don ve nguoi choi, ca than phat sang
    ///    xanh ngoc, den toa ra, camera rung - tat ca manh dan theo tien do giu.
    /// 2. Bien hinh: no - loe sang, hat bung ra, vet sang cong vut len quanh nguoi, camera giat manh.
    /// 3. Duy tri (dang Shadow): lua quy toi tim + khoi + lua xanh ngoc boc quanh than, bay nguoc len.
    /// Particle dat san trong prefab, chi bat/tat phat - khong tao gi khi choi.
    /// </summary>
    // Chay sau PlayerActionAnimation (100) de chep dung khung vua dat.
    [DefaultExecutionOrder(150)]
    [RequireComponent(typeof(PlayerShadow))]
    public class PlayerShadowFx : MonoBehaviour
    {
        static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");

        [Header("Tu luc")]
        [Tooltip("Hat bi hut vao (Shape Circle, Start Speed am de bay vao tam).")]
        [SerializeField] ParticleSystem chargeParticles;
        [Tooltip("So hat/giay luc moi giu va luc sap day.")]
        [SerializeField] Vector2 chargeRate = new Vector2(15f, 90f);
        [Tooltip("Vet sang doc do don ve nguoi choi (Stretched Billboard), phat cung luc tu luc.")]
        [SerializeField] ParticleSystem chargeStreaks;
        [SerializeField] Vector2 streakRate = new Vector2(6f, 40f);
        [Tooltip("Camera rung tang dan theo tien do giu (unit).")]
        [SerializeField] float chargeShake = 0.12f;
        [Tooltip("Lop phu phat sang (SpriteRenderer, material LuHanh/SpriteFlash), chep hinh tu Visual.")]
        [SerializeField] SpriteRenderer glowOverlay;
        [SerializeField] SpriteRenderer visual;
        [SerializeField] Color glowColor = new Color(0.27f, 0.86f, 0.77f, 1f);   // #45DCC4
        [Tooltip("Do dam toi da cua lop sang luc giu day.")]
        [Range(0f, 1f)] [SerializeField] float glowMaxAlpha = 0.75f;
        [SerializeField] Light2D glowLight;
        [SerializeField] float glowLightMax = 2.2f;
        [Tooltip("Do sang den khi dang Shadow.")]
        [SerializeField] float activeLight = 0.9f;

        [Header("Bien hinh")]
        [Tooltip("Bung ra luc vua bien hinh (Emit mot lan).")]
        [SerializeField] ParticleSystem burst;
        [SerializeField] int burstCount = 40;
        [Tooltip("Cac vet sang cong vut len quanh nguoi luc no (Emit mot lan).")]
        [SerializeField] ParticleSystem flares;
        [SerializeField] int flareCount = 10;
        [SerializeField] float burstShake = 0.45f;
        [SerializeField] float burstShakeTime = 0.45f;

        [Header("Duy tri")]
        [SerializeField] ParticleSystem smoke;
        [SerializeField] ParticleSystem flame;
        [Tooltip("Lua quy: luoi lua toi tim keo dai bay len, bao lay than.")]
        [SerializeField] ParticleSystem demonFlame;

        PlayerShadow shadow;
        MaterialPropertyBlock block;
        float hold;
        float flashAfterBegin;

        void Awake()
        {
            shadow = GetComponent<PlayerShadow>();
            block = new MaterialPropertyBlock();
            SetGlow(0f);
            if (glowLight) glowLight.enabled = false;
            Stop(chargeParticles); Stop(chargeStreaks); Stop(smoke); Stop(flame); Stop(demonFlame);
        }

        void OnEnable()
        {
            shadow.HoldChanged += OnHold;
            shadow.Started += OnStarted;
            shadow.Ended += OnEnded;
        }

        void OnDisable()
        {
            shadow.HoldChanged -= OnHold;
            shadow.Started -= OnStarted;
            shadow.Ended -= OnEnded;
        }

        void OnHold(float value)
        {
            hold = value;
            CameraShake.Hold(value * value * chargeShake);
            Charge(chargeParticles, chargeRate, value);
            Charge(chargeStreaks, streakRate, value);
        }

        void OnStarted()
        {
            Stop(chargeParticles);
            Stop(chargeStreaks);
            CameraShake.Hold(0f);
            CameraShake.Kick(burstShake, burstShakeTime);
            if (burst) burst.Emit(burstCount);
            if (flares) flares.Emit(flareCount);
            if (smoke) smoke.Play();
            if (flame) flame.Play();
            if (demonFlame) demonFlame.Play();
            flashAfterBegin = 1f;   // loe sang roi tat dan
        }

        void OnEnded()
        {
            Stop(smoke);
            Stop(flame);
            Stop(demonFlame);
        }

        static void Charge(ParticleSystem ps, Vector2 rate, float value)
        {
            if (!ps) return;
            var em = ps.emission;
            em.rateOverTime = Mathf.Lerp(rate.x, rate.y, value);
            if (value > 0f && !ps.isEmitting) ps.Play();
            else if (value <= 0f) Stop(ps);
        }

        void LateUpdate()
        {
            // Lop sang: theo tien do giu; vua bien hinh thi loe day roi tat nhanh.
            if (flashAfterBegin > 0f) flashAfterBegin = Mathf.Max(0f, flashAfterBegin - Time.deltaTime * 3f);
            float a = Mathf.Max(hold * hold, flashAfterBegin) * glowMaxAlpha;
            SetGlow(a);

            if (glowLight)
            {
                float target = shadow.IsActive ? activeLight : 0f;
                target = Mathf.Max(target, hold * glowLightMax, flashAfterBegin * glowLightMax);
                glowLight.enabled = target > 0.01f;
                glowLight.intensity = target;
            }
        }

        void SetGlow(float alpha)
        {
            if (!glowOverlay) return;
            glowOverlay.enabled = alpha > 0.005f;
            if (!glowOverlay.enabled || !visual) return;
            // Chep khung hinh sau Animator/PlayerActionAnimation de lop sang khop dang nhan vat.
            glowOverlay.sprite = visual.sprite;
            glowOverlay.flipX = visual.flipX;
            var c = glowColor;
            c.a = alpha;
            block.SetColor(FlashColorId, c);
            glowOverlay.SetPropertyBlock(block);
        }

        static void Stop(ParticleSystem ps)
        {
            if (ps) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
