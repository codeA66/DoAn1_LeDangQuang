using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LuHanh.Level
{
    /// <summary>
    /// Chuyen man kieu Soul Knight:
    /// chop sang -> man hinh dong lai thanh vong tron vao tam cong -> tai scene -> the ten man
    /// (duong ke toa ra, ma man truot len, ten man hien tung chu) -> man hinh mo ra tu cho nhan vat
    /// -> cong den mo ra va nhan vat bat ra.
    /// Tu tao lan dau duoc goi va song xuyen cac scene; chay bang thoi gian thuc.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        const float FlashTime = 0.25f;
        const float IrisCloseTime = 0.55f;
        const float LineTime = 0.35f;
        const float CodeTime = 0.35f;
        const float CharTime = 0.045f;
        const float TitleHoldTime = 0.9f;
        const float CardFadeTime = 0.3f;
        const float IrisOpenTime = 0.7f;
        const float OpenRadius = 2.2f;

        static readonly int CenterId = Shader.PropertyToID("_Center");
        static readonly int RadiusId = Shader.PropertyToID("_Radius");
        static readonly Color CodeColor = new Color(0.75f, 0.98f, 1f);
        static readonly Color GlowColor = new Color(0.2f, 0.75f, 1f, 0.6f);

        static SceneTransition instance;

        Image iris;
        Material irisMat;
        Image flash;
        CanvasGroup card;
        Text codeText;
        Text titleText;
        RectTransform lineLeft, lineRight;
        RectTransform[] motes;
        bool busy;

        public static bool IsBusy => instance && instance.busy;

        /// <summary>
        /// Chuyen sang <paramref name="sceneName"/>. <paramref name="style"/> cap font + material vong tron.
        /// <paramref name="irisWorld"/>: vong tron dong lai vao diem nay (null = giua man hinh).
        /// <paramref name="arrival"/>: cong den (ban sao cua cong vua di qua) de nhan vat bat ra o man moi.
        /// </summary>
        public static void Load(string sceneName, string code, string title, LevelList style,
            Vector3? irisWorld = null, LevelPortal arrival = null)
        {
            if (!instance) Create(style);
            if (instance.busy) return;
            instance.StartCoroutine(instance.Run(sceneName, code, title, irisWorld, arrival));
        }

        /// <summary>
        /// Mo dau man khi vao thang (khong qua cong): man hinh toi -> the ten man -> mo vong tron tu cho
        /// nhan vat, cong den mo ra va nhan vat bat ra. Bo qua neu dang chuyen man (da co hieu ung roi).
        /// </summary>
        public static void PlayIntro(string code, string title, LevelList style, LevelPortal arrival)
        {
            if (!instance) Create(style);
            if (instance.busy) return;
            instance.StartCoroutine(instance.Intro(code, title, arrival));
        }

        static void Create(LevelList style)
        {
            var go = new GameObject("SceneTransition");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SceneTransition>();
            instance.Build(style);
        }

        void Build(LevelList style)
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            iris = MakeImage("Iris", transform, new Color(0.02f, 0.01f, 0.05f, 1f));
            Stretch(iris.rectTransform);
            if (style && style.irisMaterial)
            {
                irisMat = new Material(style.irisMaterial);
                iris.material = irisMat;
            }
            SetIris(new Vector2(0.5f, 0.5f), OpenRadius);
            iris.enabled = false;

            var cardGo = new GameObject("Card", typeof(RectTransform), typeof(CanvasGroup));
            cardGo.transform.SetParent(transform, false);
            Stretch((RectTransform)cardGo.transform);
            card = cardGo.GetComponent<CanvasGroup>();
            card.alpha = 0f;

            var fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            codeText = MakeText("Code", card.transform, style && style.codeFont ? style.codeFont : fallback, 120, CodeColor);
            var outline = codeText.gameObject.AddComponent<Outline>();
            outline.effectColor = GlowColor;
            outline.effectDistance = new Vector2(3f, -3f);
            var shadow = codeText.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.1f, 0.5f, 1f, 0.35f);
            shadow.effectDistance = new Vector2(0f, -8f);
            titleText = MakeText("Title", card.transform, style && style.titleFont ? style.titleFont : fallback, 52, new Color(0.88f, 0.85f, 0.96f));
            ((RectTransform)titleText.transform).anchoredPosition = new Vector2(0f, -70f);

            lineLeft = MakeLine("LineLeft", new Vector2(1f, 0.5f));
            lineRight = MakeLine("LineRight", new Vector2(0f, 0.5f));

            motes = new RectTransform[14];
            for (int i = 0; i < motes.Length; i++)
            {
                var m = MakeImage("Mote", card.transform, new Color(0.6f, 0.95f, 1f, Random.Range(0.25f, 0.7f)));
                float s = Random.Range(3f, 7f);
                m.rectTransform.sizeDelta = new Vector2(s, s);
                motes[i] = m.rectTransform;
            }

            flash = MakeImage("Flash", transform, new Color(0.85f, 1f, 1f, 0f));
            Stretch(flash.rectTransform);
            flash.enabled = false;
        }

        static Image MakeImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static Text MakeText(string name, Transform parent, Font font, int size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1600f, 180f);
            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.color = color;
            t.raycastTarget = false;
            return t;
        }

        RectTransform MakeLine(string name, Vector2 pivot)
        {
            var img = MakeImage(name, card.transform, new Color(0.6f, 0.95f, 1f, 0.8f));
            var rt = img.rectTransform;
            rt.pivot = pivot;
            rt.anchoredPosition = new Vector2(pivot.x > 0.5f ? -110f : 110f, 10f);
            rt.sizeDelta = new Vector2(0f, 2f);
            return rt;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        void SetIris(Vector2 viewport, float radius)
        {
            if (!irisMat)
            {
                // Khong co shader vong tron: dung lop toi phu deu theo do mo.
                var c = iris.color;
                c.a = Mathf.Clamp01(1f - radius / OpenRadius);
                iris.color = c;
                return;
            }
            irisMat.SetVector(CenterId, viewport);
            irisMat.SetFloat(RadiusId, radius);
        }

        static Vector2 ToViewport(Vector3? world)
        {
            var cam = Camera.main;
            if (!world.HasValue || !cam) return new Vector2(0.5f, 0.5f);
            return cam.WorldToViewportPoint(world.Value);
        }

        IEnumerator Run(string sceneName, string code, string title, Vector3? irisWorld, LevelPortal arrival)
        {
            busy = true;
            iris.enabled = true;
            iris.raycastTarget = true;

            // 1) Chop sang + dong vong tron vao tam cong.
            Vector2 centre = ToViewport(irisWorld);
            flash.enabled = true;
            for (float t = 0f; t < IrisCloseTime; t += Time.unscaledDeltaTime)
            {
                float k = t / IrisCloseTime;
                flash.color = new Color(0.85f, 1f, 1f, Mathf.Clamp01(1f - t / FlashTime) * 0.85f);
                SetIris(centre, Mathf.Lerp(OpenRadius, 0f, 1f - (1f - k) * (1f - k)));
                yield return null;
            }
            flash.enabled = false;
            SetIris(centre, 0f);

            // 2) Tai scene.
            Time.timeScale = 1f;
            var op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone) yield return null;
            yield return null;   // cho camera dat vao cho nhan vat

            yield return Arrive(code, title, arrival);
        }

        IEnumerator Intro(string code, string title, LevelPortal arrival)
        {
            busy = true;
            iris.enabled = true;
            iris.raycastTarget = true;
            SetIris(new Vector2(0.5f, 0.5f), 0f);
            yield return null;   // cho camera dat vao cho nhan vat
            yield return Arrive(code, title, arrival);
        }

        /// <summary>Man hinh dang dong kin: an nhan vat, hien the ten man, mo vong tron va cho nhan vat bat ra.</summary>
        IEnumerator Arrive(string code, string title, LevelPortal arrival)
        {
            var player = GameObject.FindWithTag("Player");
            if (arrival && player) arrival.BeginArrival(player);

            if (!string.IsNullOrEmpty(code)) yield return ShowCard(code, title ?? "");

            Vector2 open = player ? ToViewport(player.transform.position + Vector3.up * 0.9f) : new Vector2(0.5f, 0.5f);
            if (arrival && player) arrival.PlayArrival();
            for (float t = 0f; t < IrisOpenTime; t += Time.unscaledDeltaTime)
            {
                float k = t / IrisOpenTime;
                SetIris(open, Mathf.Lerp(0f, OpenRadius, k * k));
                yield return null;
            }
            SetIris(open, OpenRadius);
            iris.enabled = false;
            iris.raycastTarget = false;
            busy = false;
        }

        IEnumerator ShowCard(string code, string title)
        {
            codeText.text = code;
            titleText.text = "";
            var codeRt = (RectTransform)codeText.transform;
            foreach (var m in motes)
                m.anchoredPosition = new Vector2(Random.Range(-700f, 700f), Random.Range(-380f, 380f));
            card.alpha = 1f;
            lineLeft.sizeDelta = lineRight.sizeDelta = new Vector2(0f, 2f);

            // Duong ke toa ra + ma man truot len.
            float total = Mathf.Max(LineTime, CodeTime);
            for (float t = 0f; t < total; t += Time.unscaledDeltaTime)
            {
                float kl = Mathf.Clamp01(t / LineTime);
                float w = Mathf.Lerp(0f, 520f, 1f - (1f - kl) * (1f - kl));
                lineLeft.sizeDelta = lineRight.sizeDelta = new Vector2(w, 2f);
                float kc = Mathf.Clamp01(t / CodeTime);
                codeRt.anchoredPosition = new Vector2(0f, Mathf.Lerp(-30f, 40f, 1f - (1f - kc) * (1f - kc)));
                SetAlpha(codeText, kc);
                DriftMotes();
                yield return null;
            }

            // Ten man hien tung chu.
            for (int i = 1; i <= title.Length; i++)
            {
                titleText.text = title.Substring(0, i);
                for (float t = 0f; t < CharTime; t += Time.unscaledDeltaTime) { DriftMotes(); yield return null; }
            }
            for (float t = 0f; t < TitleHoldTime; t += Time.unscaledDeltaTime) { DriftMotes(); yield return null; }

            for (float t = 0f; t < CardFadeTime; t += Time.unscaledDeltaTime)
            {
                card.alpha = 1f - t / CardFadeTime;
                DriftMotes();
                yield return null;
            }
            card.alpha = 0f;
        }

        void DriftMotes()
        {
            float dy = 25f * Time.unscaledDeltaTime;
            foreach (var m in motes) m.anchoredPosition += new Vector2(0f, dy);
        }

        static void SetAlpha(Graphic g, float a)
        {
            var c = g.color;
            c.a = a;
            g.color = c;
        }
    }
}
