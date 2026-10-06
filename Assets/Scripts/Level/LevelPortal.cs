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
        [Header("Luc bi hut")]
        [Tooltip("Camera zoom vao con bao nhieu phan kich thuoc cu.")]
        [SerializeField] float suckZoom = 0.7f;
        [Tooltip("Cong phinh to bao nhieu khi dang hut.")]
        [SerializeField] float suckSwell = 1.3f;
        [Tooltip("So hat song xung kich luc nhan vat bien mat.")]
        [SerializeField] int shockwaveCount = 60;

        [Header("Cong den (o man moi)")]
        [SerializeField] float arrivalOpenTime = 0.3f;
        [Tooltip("Nhan vat bat ra tu cong trong bao lau.")]
        [SerializeField] float arrivalPopTime = 0.35f;
        [SerializeField] float arrivalCloseTime = 0.3f;

        float glowIntensity;
        bool isOpen;
        bool entering;
        bool isArrival;
        GameObject arrivingPlayer;
        Vector3 arrivingScale;

        public bool IsOpen => isOpen;

        void Awake()
        {
            trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
            if (glow) glowIntensity = glow.intensity;
        }

        void Start()
        {
            if (isArrival) return;
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

            var cam = FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
            float startSize = cam ? cam.Lens.OrthographicSize : 0f;
            if (swirl)
            {
                var em = swirl.emission;
                em.rateOverTimeMultiplier *= 2f;
            }

            for (float time = 0f; time < suckTime; time += Time.deltaTime)
            {
                float k = time / suckTime;
                float ease = k * k;
                t.position = Vector3.Lerp(startPos, target, ease);
                t.localScale = Vector3.Scale(startScale, Vector3.one * (1f - ease));
                t.rotation = Quaternion.Euler(0f, 0f, ease * suckSpins * 360f);
                if (visual) visual.localScale = Vector3.one * Mathf.Lerp(1f, suckSwell, k);
                if (cam)
                {
                    var lens = cam.Lens;
                    lens.OrthographicSize = Mathf.Lerp(startSize, startSize * suckZoom, 1f - (1f - k) * (1f - k));
                    cam.Lens = lens;
                }
                yield return null;
            }
            t.localScale = Vector3.zero;
            if (burst) burst.Emit(shockwaveCount);

            GoToNextLevel();
        }

        // ---- Cong den: ban sao cua cong vua di qua, mo ra o diem xuat phat cua man moi ----

        /// <summary>Tao ban sao lam cong den, song qua luc doi scene, tat cho toi khi SceneTransition goi.</summary>
        LevelPortal CreateArrivalCopy()
        {
            var copy = Instantiate(this);
            copy.isArrival = true;
            copy.gameObject.SetActive(false);
            DontDestroyOnLoad(copy.gameObject);
            return copy;
        }

        /// <summary>Dung cong nay lam cong den (goi ngay sau Instantiate, truoc Start).</summary>
        public void MarkAsArrival() => isArrival = true;

        /// <summary>Goi ngay sau khi man moi tai xong: an nhan vat, dat cong vao cho nhan vat (chua hien).</summary>
        public void BeginArrival(GameObject player)
        {
            arrivingPlayer = player;
            arrivingScale = player.transform.localScale;
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            gameObject.SetActive(true);
            transform.position = player.transform.position + Vector3.up * 0.9f;
            SetHidden();
            LockPlayer(player);
            player.transform.localScale = Vector3.zero;
        }

        /// <summary>Goi khi man hinh bat dau mo ra: cong mo, nhan vat bat ra, cong dong roi tu huy.</summary>
        public void PlayArrival()
        {
            if (arrivingPlayer) StartCoroutine(ArrivalRoutine());
        }

        IEnumerator ArrivalRoutine()
        {
            if (spin) spin.Play();
            if (swirl) swirl.Play();
            for (float t = 0f; t < arrivalOpenTime; t += Time.deltaTime)
            {
                float k = t / arrivalOpenTime;
                if (visual) visual.localScale = Vector3.one * k;
                if (glow) glow.intensity = glowIntensity * k;
                yield return null;
            }

            if (burst) burst.Emit(burstCount);
            var pt = arrivingPlayer.transform;
            pt.rotation = Quaternion.identity;
            for (float t = 0f; t < arrivalPopTime; t += Time.deltaTime)
            {
                float k = t / arrivalPopTime;
                // Bat vuot 1 mot chut roi ve 1.
                float s = k < 0.7f ? Mathf.Lerp(0f, 1.15f, k / 0.7f) : Mathf.Lerp(1.15f, 1f, (k - 0.7f) / 0.3f);
                pt.localScale = arrivingScale * s;
                yield return null;
            }
            pt.localScale = arrivingScale;
            UnlockPlayer(arrivingPlayer);

            if (swirl) swirl.Stop();
            for (float t = 0f; t < arrivalCloseTime; t += Time.deltaTime)
            {
                float k = 1f - t / arrivalCloseTime;
                if (visual) visual.localScale = Vector3.one * k;
                if (glow) glow.intensity = glowIntensity * k;
                yield return null;
            }
            Destroy(gameObject, 1f);
            if (visual) visual.localScale = Vector3.zero;
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

        static void UnlockPlayer(GameObject player)
        {
            if (player.TryGetComponent<Rigidbody2D>(out var body)) body.simulated = true;
            if (player.TryGetComponent<LuHanh.Player.PlayerController>(out var controller)) controller.enabled = true;
            if (player.TryGetComponent<LuHanh.Player.PlayerAttack>(out var attack)) attack.enabled = true;
            if (player.TryGetComponent<LuHanh.Player.PlayerMovementFx>(out var fx)) fx.enabled = true;
            if (player.TryGetComponent<UnityEngine.InputSystem.PlayerInput>(out var input)) input.enabled = true;
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
            if (next != null)
                SceneTransition.Load(next.sceneName, next.code, next.title, levels, transform.position, CreateArrivalCopy());
            else
                SceneTransition.Load(levels.completeScene, null, null, levels, transform.position);
        }
    }
}
