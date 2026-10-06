using System.Collections;
using LuHanh.Effects;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace LuHanh.Level
{
    /// <summary>
    /// Cong dich chuyen cuoi man kieu Soul Knight. Man thuong: mo san. Man boss: an, boss chet thi goi
    /// <see cref="Open"/>. Nguoi choi cham vao: khoa dieu khien, bi hut vao tam cong roi chuyen sang man ke tiep
    /// theo <see cref="LevelList"/> (het danh sach thi sang scene hoan thanh).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelPortal : MonoBehaviour
    {
        [SerializeField] LevelList levels;
        [Tooltip("Bat: cong mo san tu dau man. Tat: an cho toi khi goi Open() (vd boss chet).")]
        [SerializeField] bool openOnStart = true;

        [Header("Thanh phan")]
        [Tooltip("Object hinh cong (phong to tu 0 khi mo).")]
        [SerializeField] Transform visual;
        [SerializeField] FrameAnimation spin;
        [SerializeField] Light2D glow;
        [Tooltip("Hat nang luong xoay hut vao tam, chay lien tuc khi cong mo.")]
        [SerializeField] ParticleSystem swirl;
        [Tooltip("Hat bung ra luc cong mo va luc nguoi choi buoc vao.")]
        [SerializeField] ParticleSystem burst;
        [SerializeField] int burstCount = 30;

        [Header("Thoi gian")]
        [SerializeField] float openTime = 0.6f;
        [Tooltip("Nguoi choi bi hut vao tam cong trong bao lau.")]
        [SerializeField] float suckTime = 0.6f;
        [Tooltip("Nguoi choi xoay bao nhieu vong khi bi hut.")]
        [SerializeField] float suckSpins = 1.5f;

        Collider2D trigger;
        float glowIntensity;
        bool isOpen;
        bool entering;

        public bool IsOpen => isOpen;

        void Awake()
        {
            trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
            if (glow) glowIntensity = glow.intensity;
        }

        void Start()
        {
            if (openOnStart) SetOpenInstant();
            else SetHidden();
        }

        void SetHidden()
        {
            isOpen = false;
            trigger.enabled = false;
            if (visual) visual.localScale = Vector3.zero;
            if (glow) glow.intensity = 0f;
            if (swirl) swirl.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (spin) spin.Stop();
        }

        void SetOpenInstant()
        {
            isOpen = true;
            trigger.enabled = true;
            if (visual) visual.localScale = Vector3.one;
            if (glow) glow.intensity = glowIntensity;
            if (swirl) swirl.Play();
            if (spin) spin.Play();
        }

        /// <summary>Mo cong co hieu ung (goi khi boss chet, hoac khi xong thu thach).</summary>
        public void Open()
        {
            if (isOpen) return;
            StartCoroutine(OpenRoutine());
        }

        IEnumerator OpenRoutine()
        {
            isOpen = true;
            if (spin) spin.Play();
            if (burst) burst.Emit(burstCount);
            if (swirl) swirl.Play();
            for (float t = 0f; t < openTime; t += Time.deltaTime)
            {
                float k = t / openTime;
                // Phong to vuot qua 1 mot chut roi bat ve - cam giac cong "bung" ra.
                float s = k < 0.7f ? Mathf.Lerp(0f, 1.15f, k / 0.7f) : Mathf.Lerp(1.15f, 1f, (k - 0.7f) / 0.3f);
                if (visual) visual.localScale = Vector3.one * s;
                if (glow) glow.intensity = glowIntensity * k;
                yield return null;
            }
            if (visual) visual.localScale = Vector3.one;
            if (glow) glow.intensity = glowIntensity;
            trigger.enabled = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!isOpen || entering || !other.CompareTag("Player")) return;
            var player = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
            StartCoroutine(EnterRoutine(player));
        }

        IEnumerator EnterRoutine(GameObject player)
        {
            entering = true;
            LockPlayer(player);
            if (burst) burst.Emit(burstCount);

            var t = player.transform;
            Vector3 startPos = t.position;
            Vector3 startScale = t.localScale;
            // Tam cong tinh tu chan nhan vat: ha xuong nua chieu cao de nhan vat chui vao giua vong xoay.
            Vector3 target = transform.position + Vector3.down * 0.8f;
            for (float time = 0f; time < suckTime; time += Time.deltaTime)
            {
                float k = time / suckTime;
                float ease = k * k;
                t.position = Vector3.Lerp(startPos, target, ease);
                t.localScale = Vector3.Scale(startScale, Vector3.one * (1f - ease));
                t.rotation = Quaternion.Euler(0f, 0f, ease * suckSpins * 360f);
                yield return null;
            }
            t.localScale = Vector3.zero;

            GoToNextLevel();
        }

        static void LockPlayer(GameObject player)
        {
            // Chi tat phan dieu khien; den, quang sang van giu de nhan vat sang khi bi hut vao.
            // PlayerMovementFx cung tat vi no tu keo scale ve 1, tranh voi hieu ung thu nho.
            if (player.TryGetComponent<LuHanh.Player.PlayerController>(out var controller)) controller.enabled = false;
            if (player.TryGetComponent<LuHanh.Player.PlayerAttack>(out var attack)) attack.enabled = false;
            if (player.TryGetComponent<LuHanh.Player.PlayerMovementFx>(out var fx)) fx.enabled = false;
            if (player.TryGetComponent<UnityEngine.InputSystem.PlayerInput>(out var input)) input.enabled = false;
            if (player.TryGetComponent<Rigidbody2D>(out var body))
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }
        }

        void GoToNextLevel()
        {
            if (!levels)
            {
                Debug.LogWarning("LevelPortal chua gan LevelList.", this);
                return;
            }
            int index = levels.IndexOf(SceneManager.GetActiveScene().name);
            LevelProgress.Unlock(index + 1);
            var next = levels.Get(index + 1);
            if (next != null) SceneTransition.Load(next.sceneName, next.code, next.title);
            else SceneTransition.Load(levels.completeScene);
        }
    }
}
