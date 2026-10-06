using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LuHanh.Level
{
    /// <summary>
    /// Chuyen man kieu Soul Knight: toi dan -> tai scene -> hien the ten man (ma + ten) -> sang dan.
    /// Tu tao lan dau duoc goi va song xuyen cac scene (DontDestroyOnLoad); khong can dat vao scene.
    /// Chay bang thoi gian thuc nen khong bi anh huong boi hitstop.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        const float FadeOutTime = 0.45f;
        const float TitleHoldTime = 1.3f;
        const float FadeInTime = 0.6f;

        static SceneTransition instance;

        CanvasGroup group;
        Text codeText;
        Text titleText;
        bool busy;

        public static bool IsBusy => instance && instance.busy;

        /// <summary>Chuyen sang <paramref name="sceneName"/>; co <paramref name="code"/> thi hien the ten man.</summary>
        public static void Load(string sceneName, string code = null, string title = null)
        {
            if (!instance) Create();
            if (instance.busy) return;
            instance.StartCoroutine(instance.Run(sceneName, code, title));
        }

        static void Create()
        {
            var go = new GameObject("SceneTransition");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SceneTransition>();
            instance.Build();
        }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            var bg = new GameObject("Fade", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(transform, false);
            Stretch((RectTransform)bg.transform);
            bg.GetComponent<Image>().color = new Color(0.02f, 0.01f, 0.05f, 1f);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            codeText = MakeText("Code", font, 96, new Vector2(0f, 40f), new Color(0.75f, 0.98f, 1f));
            titleText = MakeText("Title", font, 44, new Vector2(0f, -50f), new Color(0.85f, 0.82f, 0.95f));
        }

        Text MakeText(string name, Font font, int size, Vector2 pos, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1600f, 140f);
            rt.anchoredPosition = pos;
            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            return t;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        IEnumerator Run(string sceneName, string code, string title)
        {
            busy = true;
            group.blocksRaycasts = true;
            SetTitle(null, null);

            yield return Fade(0f, 1f, FadeOutTime);

            Time.timeScale = 1f;
            var op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone) yield return null;

            if (!string.IsNullOrEmpty(code))
            {
                SetTitle(code, title);
                yield return new WaitForSecondsRealtime(TitleHoldTime);
            }

            yield return Fade(1f, 0f, FadeInTime);
            group.blocksRaycasts = false;
            busy = false;
        }

        void SetTitle(string code, string title)
        {
            codeText.text = code ?? "";
            titleText.text = title ?? "";
        }

        IEnumerator Fade(float from, float to, float time)
        {
            for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / time);
                yield return null;
            }
            group.alpha = to;
        }
    }
}
