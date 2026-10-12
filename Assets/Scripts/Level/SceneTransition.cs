using System.Collections;
using LuHanh.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LuHanh.Level
{
    /// <summary>
    /// Chuyen man kieu Soul Knight:
    /// chop sang -> man hinh dong lai thanh vong tron vao tam cong -> tai scene -> the ten man kieu
    /// Hollow Knight (hoa van tren/duoi toa ra, ten man chu hoa lon, ma man nho) -> man hinh mo ra tu cho nhan vat
    /// -> cong den mo ra va nhan vat bat ra.
    /// Tu tao lan dau duoc goi va song xuyen cac scene; chay bang thoi gian thuc.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        const float FlashTime = 0.25f;
        const float IrisCloseTime = 0.55f;
        const float CardInTime = 0.7f;
        const float TitleHoldTime = 1.6f;
        const float CardFadeTime = 0.6f;
        const int TitleSize = 92;
        const int TitleCapSize = 118;
        const float OrnamentY = 118f;
        const float IrisOpenTime = 0.7f;
        const float OpenRadius = 2.2f;

        static readonly int CenterId = Shader.PropertyToID("_Center");
        static readonly int RadiusId = Shader.PropertyToID("_Radius");
        static readonly Color CodeColor = new Color(0.72f, 1f, 0.94f, 1f);      // #B8FFF0
        static readonly Color TitleColor = new Color(0.93f, 0.91f, 0.97f, 1f);
        static readonly Color GlowColor = new Color(0.62f, 0.55f, 0.85f, 0.35f);
        static readonly Color OrnamentColor = new Color(0.93f, 0.91f, 0.97f, 0.95f);

        static SceneTransition instance;

        Image iris;
        Material irisMat;
        Image flash;
        CanvasGroup card;
        Text codeText;
        Text titleText;
        Image ornTop, ornBottom;
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
            UIBuild.SetupOverlay(gameObject, 1000);

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
            // Kieu the ten vung cua Hollow Knight: hoa van tren/duoi, ten man chu hoa lon o giua,
            // ma man nho phia tren.
            var ornament = style ? style.titleOrnament : null;
            ornTop = MakeOrnament("OrnamentTop", ornament, OrnamentY, false);
            ornBottom = MakeOrnament("OrnamentBottom", ornament, -OrnamentY - 20f, true);

            codeText = UIBuild.MakeText("Code", card.transform, style && style.levelCodeFont ? style.levelCodeFont : fallback, 34, CodeColor);
            ((RectTransform)codeText.transform).anchoredPosition = new Vector2(0f, 62f);

            titleText = UIBuild.MakeText("Title", card.transform, style && style.nameFont ? style.nameFont : fallback, TitleSize, TitleColor);
            titleText.supportRichText = true;
            ((RectTransform)titleText.transform).anchoredPosition = new Vector2(0f, -10f);
            var shadow = titleText.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.05f, 0.03f, 0.1f, 0.85f);
            shadow.effectDistance = new Vector2(3f, -4f);
            var glow = titleText.gameObject.AddComponent<Outline>();
            glow.effectColor = GlowColor;
            glow.effectDistance = new Vector2(1.5f, -1.5f);

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

        Image MakeOrnament(string name, Sprite sprite, float y, bool flip)
        {
            var img = MakeImage(name, card.transform, OrnamentColor);
            img.sprite = sprite;
            img.enabled = sprite;
            var rt = img.rectTransform;
            rt.sizeDelta = new Vector2(720f, 120f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.localScale = new Vector3(1f, flip ? -1f : 1f, 1f);
            return img;
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
            codeText.text = "LEVEL " + code;
            titleText.text = UIBuild.SmallCaps(title, TitleCapSize);
            var titleRt = (RectTransform)titleText.transform;
            foreach (var m in motes)
                m.anchoredPosition = new Vector2(Random.Range(-700f, 700f), Random.Range(-380f, 380f));
            card.alpha = 0f;

            // Hien mo dan: hoa van toa tu giua ra, ten man nhich len, ma man hien sau mot chut.
            for (float t = 0f; t < CardInTime; t += Time.unscaledDeltaTime)
            {
                float k = t / CardInTime;
                float e = 1f - (1f - k) * (1f - k);
                card.alpha = e;
                float sx = Mathf.Lerp(0.55f, 1f, e);
                ornTop.rectTransform.localScale = new Vector3(sx, 1f, 1f);
                ornBottom.rectTransform.localScale = new Vector3(sx, -1f, 1f);
                titleRt.anchoredPosition = new Vector2(0f, Mathf.Lerp(-28f, -10f, e));
                SetAlpha(codeText, Mathf.Clamp01(k * 1.6f - 0.6f));
                DriftMotes();
                yield return null;
            }
            card.alpha = 1f;
            SetAlpha(codeText, 1f);
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
