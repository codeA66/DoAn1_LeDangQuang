using LuHanh.Core;
using LuHanh.Player;
using LuHanh.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LuHanh.Level
{
    /// <summary>
    /// Ghe checkpoint kieu Hollow Knight: dung gan bam E de ngoi -> hoi day mau, dat diem hoi sinh, luu game,
    /// quai da chet o cac phong khac hoi sinh (quai trong phong dang ngoi giu nguyen, nhu Hollow Knight). Bam trai/phai/nhay de dung day. Tat game mo lai thi hien o ghe da ngoi cuoi.
    /// Hinh ghe tu lat frame (BenchIdle khi chua ngoi, BenchActive luc vua ngoi, BenchLit khi da kich hoat).
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Hinh")]
        [SerializeField] SpriteRenderer bench;
        [SerializeField] Sprite[] idleFrames;
        [SerializeField] Sprite[] activeFrames;
        [SerializeField] Sprite[] litFrames;
        [SerializeField] float fps = 10f;
        [Tooltip("Den tinh the xanh ngoc tren cot ghe.")]
        [SerializeField] Light2D glow;
        [SerializeField] float idleIntensity = 0.35f;
        [SerializeField] float litIntensity = 1.1f;

        [Header("Hieu ung")]
        [Tooltip("Hat sang bay len tu tinh the (lap). It khi chua ngoi, nhieu khi da kich hoat.")]
        [SerializeField] ParticleSystem motes;
        [SerializeField] float motesIdleRate = 1f;
        [SerializeField] float motesLitRate = 7f;
        [Tooltip("Vong hat bung ra luc vua ngoi.")]
        [SerializeField] ParticleSystem burst;
        [SerializeField] int burstCount = 40;
        [Tooltip("Vong sang ve tay (BenchActivate) lan ra tu tinh the luc vua ngoi.")]
        [SerializeField] LuHanh.Effects.FrameAnimation activateFx;
        [Tooltip("Luc vua ngoi: den tinh the loe rong ra roi thu ve.")]
        [SerializeField] float pulseRadius = 9f;
        [SerializeField] float pulseIntensity = 2.6f;
        [SerializeField] float pulseTime = 0.7f;
        [SerializeField] Color flashColor = new Color(0.72f, 1f, 0.94f, 0.28f);
        [SerializeField] float flashTime = 0.5f;
        [Tooltip("Hoi tung mask mau mot, cach nhau bao lau.")]
        [SerializeField] float healInterval = 0.22f;
        [SerializeField] float savedTextTime = 2.2f;

        [Header("Ngoi")]
        [Tooltip("Cho nguoi choi ngoi, tinh tu goc ghe (chan ghe).")]
        [SerializeField] Vector2 seatOffset = Vector2.zero;
        [Tooltip("Khoang cach ngang toi ghe de bam E duoc.")]
        [SerializeField] float useRange = 1.6f;
        [SerializeField] string interactAction = "Interact";
        [SerializeField] string jumpAction = "Jump";
        [SerializeField] Font font;

        static bool restoredThisLoad;

        PlayerController controller;
        PlayerHealth health;
        PlayerActionAnimation actionAnimation;
        MonoBehaviour[] blockWhileSitting;
        InputAction interact, jump;
        Sprite[] frames;
        CanvasGroup prompt;
        CanvasGroup savedText;
        Image flash;
        float baseRadius;
        float frameTimer, standTimer = -1f;
        int frame;
        bool lit, sitting;

        Vector2 Seat => (Vector2)transform.position + seatOffset;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => restoredThisLoad = false;

        void Awake() => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
        static void OnSceneLoaded(Scene s, LoadSceneMode m) => restoredThisLoad = false;

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p)
            {
                controller = p.GetComponent<PlayerController>();
                health = p.GetComponent<PlayerHealth>();
                actionAnimation = p.GetComponent<PlayerActionAnimation>();
                blockWhileSitting = new MonoBehaviour[]
                {
                    controller, p.GetComponent<PlayerAttack>(), p.GetComponent<PlayerOrbShot>(), p.GetComponent<PlayerShadow>()
                };
                var input = p.GetComponent<PlayerInput>();
                if (input)
                {
                    interact = input.actions.FindAction(interactAction);
                    jump = input.actions.FindAction(jumpAction);
                }
            }
            BuildPrompt();
            if (glow) baseRadius = glow.pointLightOuterRadius;
            SetMotes(motesIdleRate);
            SetFrames(idleFrames);
            if (glow) glow.intensity = idleIntensity;

            // Mo game / vao man: dua nguoi choi ve ghe da ngoi cuoi trong scene nay.
            if (!restoredThisLoad && controller && SaveGame.HasCheckpointIn(gameObject.scene.name) &&
                Vector2.Distance(SaveGame.Data.checkpointPosition, Seat) < 0.5f)
            {
                restoredThisLoad = true;
                controller.Respawn(Seat);
                if (health) health.SetCheckpoint(Seat);
                Light();
            }
        }

        void Update()
        {
            Animate();
            if (!controller) return;

            if (sitting)
            {
                if (standTimer >= 0f)
                {
                    standTimer -= Time.deltaTime;
                    if (standTimer < 0f) FinishStand();
                }
                else if (Mathf.Abs(controller.RawMove.x) > 0.5f || (jump != null && jump.WasPressedThisFrame()))
                {
                    standTimer = actionAnimation ? actionAnimation.PlayStandUp() : 0f;
                }
                return;
            }

            bool near = controller.enabled && controller.IsGrounded &&
                        Mathf.Abs(controller.transform.position.x - Seat.x) <= useRange &&
                        Mathf.Abs(controller.transform.position.y - Seat.y) <= 1f;
            if (prompt) prompt.alpha = Mathf.MoveTowards(prompt.alpha, near ? 1f : 0f, Time.deltaTime * 6f);
            if (near && interact != null && interact.WasPressedThisFrame()) Sit();
        }

        void Sit()
        {
            sitting = true;
            if (prompt) prompt.alpha = 0f;
            foreach (var b in blockWhileSitting) if (b) b.enabled = false;
            var body = controller.GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.zero;
            body.position = Seat;
            controller.transform.position = Seat;
            controller.Face(1f);
            if (actionAnimation) actionAnimation.PlaySit();

            if (health)
            {
                StartCoroutine(HealStepByStep());
                health.SetCheckpoint(Seat);
            }
            SaveGame.Data.checkpointScene = gameObject.scene.name;
            SaveGame.Data.checkpointPosition = Seat;
            SaveGame.Save();
            StartCoroutine(SitFx());
            RespawnManager.RespawnDefeatedEnemies(Room.At(Seat));

            frame = 0;
            SetFrames(activeFrames);
            lit = true;
        }

        void FinishStand()
        {
            sitting = false;
            standTimer = -1f;
            foreach (var b in blockWhileSitting) if (b) b.enabled = true;
            if (actionAnimation) actionAnimation.Cancel();
        }

        void Light()
        {
            lit = true;
            SetFrames(litFrames);
            if (glow) glow.intensity = litIntensity;
            SetMotes(motesLitRate);
        }

        void SetFrames(Sprite[] f)
        {
            frames = f;
            frame = 0;
            frameTimer = 0f;
            if (bench && frames != null && frames.Length > 0) bench.sprite = frames[0];
        }

        void Animate()
        {
            if (!bench || frames == null || frames.Length == 0) return;
            frameTimer += Time.deltaTime;
            if (frameTimer < 1f / fps) return;
            frameTimer -= 1f / fps;
            frame++;
            // BenchActive chay mot luot roi sang BenchLit lap.
            if (frames == activeFrames && frame >= frames.Length) { Light(); return; }
            bench.sprite = frames[frame % frames.Length];
            if (glow && frames == activeFrames)
                glow.intensity = Mathf.Lerp(idleIntensity, litIntensity, (float)frame / frames.Length);
        }

        void SetMotes(float rate)
        {
            if (!motes) return;
            var e = motes.emission;
            e.rateOverTime = rate;
            if (!motes.isPlaying) motes.Play();
        }

        /// <summary>Hoi tung mask (moi mask nay len trong HUD) thay vi day mot luc.</summary>
        System.Collections.IEnumerator HealStepByStep()
        {
            var h = health.Health;
            while (sitting && h.Current < h.Max)
            {
                yield return new WaitForSeconds(healInterval);
                h.Heal(1);
                Sfx.Play(SfxId.Heal, 0.6f);
            }
        }

        /// <summary>Luc vua ngoi: hat bung ra, den loe rong roi thu ve, chop sang man hinh, chu "Game Saved".</summary>
        System.Collections.IEnumerator SitFx()
        {
            Sfx.Play(SfxId.Bench);
            if (burst) burst.Emit(burstCount);
            if (activateFx) activateFx.Play();
            if (flash) flash.color = flashColor;
            float t = 0f;
            while (t < Mathf.Max(pulseTime, flashTime))
            {
                t += Time.deltaTime;
                if (glow)
                {
                    float k = 1f - Mathf.Clamp01(t / pulseTime);
                    glow.pointLightOuterRadius = Mathf.Lerp(baseRadius, pulseRadius, k * k);
                    glow.intensity = Mathf.Max(glow.intensity, Mathf.Lerp(litIntensity, pulseIntensity, k * k));
                }
                if (flash)
                {
                    var c = flashColor;
                    c.a *= 1f - Mathf.Clamp01(t / flashTime);
                    flash.color = c;
                }
                yield return null;
            }
            if (glow) glow.pointLightOuterRadius = baseRadius;

            // Chu "Game Saved" hien mo roi tat.
            if (!savedText) yield break;
            for (t = 0f; t < savedTextTime; t += Time.deltaTime)
            {
                savedText.alpha = Mathf.Clamp01(Mathf.Min(t, savedTextTime - t) * 3f);
                yield return null;
            }
            savedText.alpha = 0f;
        }

        void BuildPrompt()
        {
            var go = new GameObject("CheckpointPrompt", typeof(CanvasGroup));
            go.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(go, 40);
            prompt = go.GetComponent<CanvasGroup>();
            prompt.alpha = 0f;
            prompt.blocksRaycasts = false;
            var t = UIBuild.MakeText("Text", go.transform, font, 34, new Color(0.72f, 1f, 0.94f));
            t.text = "E  -  Rest";
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 140f);
            t.gameObject.AddComponent<Outline>().effectColor = new Color(0.05f, 0.04f, 0.08f, 0.9f);

            // Lop chop sang toan man hinh luc vua ngoi (canvas rieng: khong an theo alpha cua loi nhac).
            var fxGo = new GameObject("CheckpointFx");
            fxGo.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(fxGo, 41);
            var flashGo = new GameObject("Flash", typeof(RectTransform), typeof(Image));
            flashGo.transform.SetParent(fxGo.transform, false);
            var frt = (RectTransform)flashGo.transform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one; frt.offsetMin = frt.offsetMax = Vector2.zero;
            flash = flashGo.GetComponent<Image>();
            flash.raycastTarget = false;
            flash.color = Color.clear;

            // Chu "Game Saved" o goc duoi ben phai.
            var savedGo = new GameObject("Saved", typeof(RectTransform), typeof(CanvasGroup));
            savedGo.transform.SetParent(fxGo.transform, false);
            savedText = savedGo.GetComponent<CanvasGroup>();
            savedText.alpha = 0f;
            var st = UIBuild.MakeText("Text", savedGo.transform, font, 30, new Color(0.72f, 1f, 0.94f));
            st.text = "Game Saved";
            st.alignment = TextAnchor.MiddleRight;
            var srt = st.rectTransform;
            srt.anchorMin = srt.anchorMax = srt.pivot = new Vector2(1f, 0f);
            srt.sizeDelta = new Vector2(500f, 60f);
            srt.anchoredPosition = new Vector2(-60f, 50f);
            st.gameObject.AddComponent<Outline>().effectColor = new Color(0.05f, 0.04f, 0.08f, 0.9f);
        }
    }
}
