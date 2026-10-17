using LuHanh.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Level
{
    /// <summary>
    /// Cua song sat kieu Soul Knight: sap tu tran xuong khi phong khoa, rut len khi don xong.
    /// Goc object = mep tren cua o cua; cua keo dai xuong <see cref="height"/> unit.
    /// Hinh (SpriteRenderer Tiled) va collider doi kich thuoc theo do dong, khong sinh/huy gi.
    /// </summary>
    public class Gate : MonoBehaviour
    {
        [Tooltip("SpriteRenderer song sat, Draw Mode = Tiled, pivot giua.")]
        [SerializeField] SpriteRenderer bars;
        [Tooltip("Collider chan nguoi choi (layer Ground de dung/va duoc).")]
        [SerializeField] BoxCollider2D blocker;
        [Tooltip("Den bao: do khi khoa, xanh ngoc khi mo roi tat dan. De trong thi bo qua.")]
        [SerializeField] Light2D signal;
        [SerializeField] float width = 1f;
        [SerializeField] float height = 5f;
        [Tooltip("Giay de sap xuong / rut len.")]
        [SerializeField] float moveTime = 0.18f;
        [SerializeField] Color lockedColor = new Color(0.92f, 0.4f, 0.28f);   // #EA6547
        [SerializeField] Color openColor = new Color(0.27f, 0.86f, 0.77f);    // #45DCC4
        [SerializeField] float signalIntensity = 1.6f;
        [Tooltip("Giay den xanh tat dan sau khi mo.")]
        [SerializeField] float openGlowTime = 0.8f;
        [Tooltip("Dong san tu dau (vd cua duong tat, mo bang can gat).")]
        [SerializeField] bool startClosed;

        [Header("Sap manh (cua phong boss)")]
        [Tooltip("Bui bung ra o chan cua luc sap cham dat (Emit). De trong = khong co.")]
        [SerializeField] ParticleSystem slamDust;
        [SerializeField] int slamDustCount = 28;
        [Tooltip("Do rung camera luc sap cham dat (unit). 0 = khong rung.")]
        [SerializeField] float slamShake;
        [SerializeField] float slamShakeTime = 0.35f;

        float amount;          // 0 = mo, 1 = dong
        float target;
        float glowTimer;

        public bool IsClosed => target > 0.5f;

        void Awake()
        {
            target = startClosed ? 1f : 0f;
            SetAmount(target);
        }

        public void Close()
        {
            target = 1f;
            if (signal) { signal.color = lockedColor; signal.intensity = signalIntensity; signal.enabled = true; }
        }

        public void Open()
        {
            target = 0f;
            glowTimer = openGlowTime;
            if (signal) { signal.color = openColor; signal.intensity = signalIntensity; signal.enabled = true; }
        }

        /// <summary>Mo ngay, khong hieu ung (dung khi hoi sinh).</summary>
        /// <summary>Ve trang thai ban dau ngay, khong hieu ung (cua dong san thi dong lai, con lai mo).</summary>
        public void ResetInstant()
        {
            if (!startClosed) { OpenInstant(); return; }
            target = 1f;
            glowTimer = 0f;
            SetAmount(1f);
            if (signal) signal.enabled = false;
        }

        public void OpenInstant()
        {
            target = 0f;
            glowTimer = 0f;
            SetAmount(0f);
            if (signal) signal.enabled = false;
        }

        void Update()
        {
            if (!Mathf.Approximately(amount, target))
            {
                SetAmount(Mathf.MoveTowards(amount, target, Time.deltaTime / Mathf.Max(0.01f, moveTime)));
                if (target > 0.5f && amount >= 1f) Slam();
            }

            if (glowTimer > 0f && signal)
            {
                glowTimer -= Time.deltaTime;
                signal.intensity = signalIntensity * Mathf.Clamp01(glowTimer / openGlowTime);
                if (glowTimer <= 0f) signal.enabled = false;
            }
        }

        void Slam()
        {
            Sfx.Play(SfxId.GateSlam);
            if (slamDust)
            {
                slamDust.transform.position = transform.position + Vector3.down * height;
                slamDust.Emit(slamDustCount);
            }
            if (slamShake > 0f) CameraShake.Kick(slamShake, slamShakeTime);
        }

        void SetAmount(float k)
        {
            amount = k;
            float h = height * k;
            if (bars)
            {
                bars.enabled = h > 0.01f;
                bars.size = new Vector2(width, Mathf.Max(0.01f, h));
                bars.transform.localPosition = new Vector3(0f, -h * 0.5f, 0f);
            }
            if (blocker)
            {
                blocker.enabled = h > 0.01f;
                blocker.size = new Vector2(width, Mathf.Max(0.01f, h));
                blocker.offset = new Vector2(0f, -h * 0.5f);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.92f, 0.4f, 0.28f, 0.6f);
            Gizmos.DrawWireCube(transform.position + Vector3.down * height * 0.5f, new Vector3(width, height, 0f));
        }
    }
}
