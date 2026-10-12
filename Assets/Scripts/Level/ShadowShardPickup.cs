using LuHanh.Core;
using LuHanh.Player;
using LuHanh.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LuHanh.Level
{
    /// <summary>
    /// Vat pham huong dan Shadow (Level2): cham vao thi mo khoa kha nang, nap day thanh de thu ngay
    /// va (neu bat showScreenPrompt) hien loi huong dan giu phim; bien hinh lan dau thi loi huong dan tat.
    /// Choi lai (da mo khoa tu truoc) hoac nguoi choi chet thi vat pham hien lai de nhat lai va xem lai huong dan.
    /// Loi huong dan tu dung Canvas rieng mot lan trong Awake, khong tao gi them khi choi.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShadowShardPickup : MonoBehaviour, IResettable
    {
        [Tooltip("Hinh vat pham (con), nhap nho len xuong.")]
        [SerializeField] Transform visual;
        [SerializeField] float bobHeight = 0.2f;
        [SerializeField] float bobSpeed = 2.2f;

        [Header("Loi huong dan")]
        [Tooltip("Hien chu huong dan giua man hinh khi nhat. Mac dinh tat: huong dan dung bien chu dat trong man (kieu Level1).")]
        [SerializeField] bool showScreenPrompt;
        [SerializeField] Font font;
        [SerializeField] string title = "SHADOW FORM";
        [SerializeField, TextArea] string hint = "Hold  [ Y ]  when the bar is full";
        [SerializeField, TextArea] string detail = "Defeat enemies to collect soul energy.\nShadow form: faster, longer reach, strikes seek enemies.";
        [SerializeField] float fadeTime = 0.4f;

        Vector3 visualBase;
        CanvasGroup prompt;
        PlayerShadow shadow;
        float fadeTarget;
        bool taken;

        void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            RespawnManager.Register(this);
            if (visual) visualBase = visual.localPosition;
            if (showScreenPrompt) BuildPrompt();
        }

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p) shadow = p.GetComponent<PlayerShadow>();
            // Luon hien du da mo khoa tu truoc: day la cho huong dan cua man, choi lai van can thay.
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (taken || other.isTrigger || !other.CompareTag("Player") || !shadow) return;
            taken = true;
            shadow.Unlock();
            shadow.FillEnergy();
            shadow.Started += HidePrompt;
            SetShown(false);
            fadeTarget = 1f;
        }

        /// <summary>Nguoi choi chet: tinh the hien lai de nhat lai (nap day + xem lai huong dan).</summary>
        public void ResetState()
        {
            taken = false;
            SetShown(true);
            fadeTarget = 0f;
            if (prompt) prompt.alpha = 0f;
            if (shadow) shadow.Started -= HidePrompt;
        }

        void SetShown(bool shown)
        {
            if (visual) visual.gameObject.SetActive(shown);
            foreach (var l in GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>(true))
                l.enabled = shown;
        }

        void HidePrompt()
        {
            fadeTarget = 0f;
            if (shadow) shadow.Started -= HidePrompt;
        }

        void OnDestroy()
        {
            RespawnManager.Unregister(this);
            if (shadow) shadow.Started -= HidePrompt;
        }

        void Update()
        {
            if (visual && !taken)
                visual.localPosition = visualBase + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            if (prompt && !Mathf.Approximately(prompt.alpha, fadeTarget))
                prompt.alpha = Mathf.MoveTowards(prompt.alpha, fadeTarget, Time.unscaledDeltaTime / fadeTime);
        }

        void BuildPrompt()
        {
            var canvasGo = new GameObject("ShadowTutorial", typeof(CanvasGroup));
            canvasGo.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(canvasGo, 50);
            prompt = canvasGo.GetComponent<CanvasGroup>();
            prompt.alpha = 0f;
            prompt.blocksRaycasts = false;

            MakeLine(canvasGo.transform, title, 54, new Color(0.72f, 1f, 0.94f), -150f);
            MakeLine(canvasGo.transform, hint, 40, new Color(0.9f, 0.88f, 0.97f), -215f);
            MakeLine(canvasGo.transform, detail, 28, new Color(0.7f, 0.66f, 0.82f), -285f);
        }

        /// <summary>Mot dong chu neo mep tren man hinh, co vien toi.</summary>
        void MakeLine(Transform parent, string value, int size, Color color, float y)
        {
            var t = UIBuild.MakeText("Text", parent, font, size, color);
            t.text = value;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            t.gameObject.AddComponent<Outline>().effectColor = new Color(0.05f, 0.04f, 0.08f, 0.9f);
        }
    }
}
