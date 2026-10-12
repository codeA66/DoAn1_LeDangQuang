using System.Collections;
using LuHanh.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LuHanh.UI
{
    /// <summary>
    /// Ten boss hien len khi vao tran (kieu Hollow Knight): dong phu nho phia tren, ten chu hoa lon,
    /// chu cai dau moi tu to hon; mo dan, giu, roi tat dan. Goi <see cref="Show"/> (vd tu RoomEncounter.onStarted).
    /// Canvas rieng dung mot lan trong Awake; nguoi choi chet thi tat ngay.
    /// </summary>
    public class BossTitleCard : MonoBehaviour, IResettable
    {
        [Header("Noi dung")]
        [SerializeField] string bossName = "Foundry Warden";
        [Tooltip("Dong nho phia tren ten. De trong = khong hien.")]
        [SerializeField] string subtitle = "";
        [Tooltip("Font Beholden trong Assets/Art/Fonts.")]
        [SerializeField] Font font;

        [Header("Bo cuc (khung tham chieu 1920x1080)")]
        [Tooltip("Vi tri tam ten tren man hinh, 0..1 (0,0 = goc duoi trai).")]
        [SerializeField] Vector2 screenAnchor = new Vector2(0.5f, 0.24f);
        [SerializeField] int nameSize = 96;
        [Tooltip("Co chu cai dau moi tu.")]
        [SerializeField] int nameCapSize = 124;
        [SerializeField] int subtitleSize = 34;
        [SerializeField] Color nameColor = new Color(0.93f, 0.91f, 0.97f, 1f);
        [SerializeField] Color subtitleColor = new Color(0.72f, 0.68f, 0.82f, 1f);

        [Header("Thoi gian (giay thuc)")]
        [SerializeField] float fadeIn = 0.6f;
        [SerializeField] float hold = 1.8f;
        [SerializeField] float fadeOut = 0.8f;

        CanvasGroup group;
        Text nameText;
        RectTransform nameRect;

        /// <summary>Tong thoi gian the hien tren man hinh.</summary>
        public float Duration => fadeIn + hold + fadeOut;

        void Awake()
        {
            RespawnManager.Register(this);
            Build();
        }

        void OnDestroy() => RespawnManager.Unregister(this);

        public void Show()
        {
            StopAllCoroutines();
            StartCoroutine(Run());
        }

        public void ResetState()
        {
            StopAllCoroutines();
            if (group) group.alpha = 0f;
        }

        IEnumerator Run()
        {
            for (float t = 0f; t < fadeIn; t += Time.unscaledDeltaTime)
            {
                float e = 1f - (1f - t / fadeIn) * (1f - t / fadeIn);
                group.alpha = e;
                nameRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(-18f, 0f, e));
                yield return null;
            }
            group.alpha = 1f;
            nameRect.anchoredPosition = Vector2.zero;
            for (float t = 0f; t < hold; t += Time.unscaledDeltaTime) yield return null;
            for (float t = 0f; t < fadeOut; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1f - t / fadeOut;
                yield return null;
            }
            group.alpha = 0f;
        }

        void Build()
        {
            var canvasGo = new GameObject("BossTitle", typeof(CanvasGroup));
            canvasGo.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(canvasGo, 60);
            group = canvasGo.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            var anchorGo = new GameObject("Anchor", typeof(RectTransform));
            anchorGo.transform.SetParent(canvasGo.transform, false);
            var anchor = (RectTransform)anchorGo.transform;
            anchor.anchorMin = anchor.anchorMax = screenAnchor;
            anchor.sizeDelta = Vector2.zero;

            nameText = UIBuild.MakeText("Name", anchor, font, nameSize, nameColor);
            nameText.supportRichText = true;
            nameText.text = UIBuild.SmallCaps(bossName, nameCapSize);
            nameRect = (RectTransform)nameText.transform;
            var shadow = nameText.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.05f, 0.03f, 0.1f, 0.85f);
            shadow.effectDistance = new Vector2(3f, -4f);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var sub = UIBuild.MakeText("Subtitle", anchor, font, subtitleSize, subtitleColor);
                sub.text = subtitle;
                ((RectTransform)sub.transform).anchoredPosition = new Vector2(0f, nameCapSize * 0.75f);
            }
        }
    }
}
