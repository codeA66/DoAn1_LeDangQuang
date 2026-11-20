using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LuHanh.UI
{
    /// <summary>
    /// Khung thoai co lua chon (vd "Want to upgrade?" Yes / No), dung bang code. Dung chung cho moi NPC:
    /// goi <see cref="Show"/>, nhan chi so lua chon qua callback. Chu hien dan tung ky tu; trai/phai doi lua chon,
    /// xac nhan bang E (Interact), Space (Jump), J (Attack) hoac Enter (UI/Submit). Dem bang thoi gian thuc nen chay ca khi game dang dung.
    /// </summary>
    public class ChoiceDialog : MonoBehaviour
    {
        static ChoiceDialog instance;

        [SerializeField] float charsPerSecond = 45f;
        [SerializeField] Color panelColor = new(0.047f, 0.039f, 0.078f, 0.9f);   // #0C0A14
        [SerializeField] Color borderColor = new(0.95f, 0.96f, 0.97f, 0.85f);     // #F2F4F8
        [SerializeField] Color nameColor = new(0.75f, 0.85f, 0.95f);              // #BFD8F2
        [SerializeField] Color textColor = new(0.95f, 0.96f, 0.97f);
        [SerializeField] Color selectedColor = new(0.72f, 1f, 0.94f);             // #B8FFF0
        [SerializeField] Color idleColor = new(0.55f, 0.55f, 0.62f);

        CanvasGroup group;
        Text nameText, bodyText;
        Text[] optionTexts;
        string fullText;
        string[] options;
        float shownChars;
        int selected;
        Action<int> onChosen;
        InputAction move;
        InputAction[] confirms = new InputAction[0];
        bool waitRelease, open;
        float lastMoveSign;

        public static bool IsOpen => instance && instance.open;

        /// <summary>
        /// Hien khung thoai. <paramref name="input"/>: PlayerInput cua nguoi choi (doc action Move va cac phim xac nhan).
        /// Lua chon mac dinh la phan tu dau.
        /// </summary>
        public static void Show(PlayerInput input, Font font, string speaker, string text, string[] choices, Action<int> chosen)
        {
            if (!instance)
            {
                var go = new GameObject("ChoiceDialog");
                instance = go.AddComponent<ChoiceDialog>();
                instance.Build(font);
            }
            instance.Open(input, speaker, text, choices, chosen);
        }

        void OnDestroy() { if (instance == this) instance = null; }

        void Open(PlayerInput input, string speaker, string text, string[] choices, Action<int> chosen)
        {
            move = input ? input.actions.FindAction("Move") : null;
            confirms = input ? new[]
            {
                input.actions.FindAction("Interact"), input.actions.FindAction("Jump"),
                input.actions.FindAction("Attack"), input.actions.FindAction("UI/Submit")
            } : new InputAction[0];
            // Enter thuoc map UI: map Player dang bat khong tu bat action nay.
            foreach (var a in confirms) if (a != null && !a.enabled) a.Enable();
            nameText.text = speaker;
            fullText = text;
            options = choices;
            onChosen = chosen;
            shownChars = 0f;
            selected = 0;
            waitRelease = true;   // phim E vua bam de mo khung thoai: doi nha ra moi tinh xac nhan
            lastMoveSign = 0f;
            for (int i = 0; i < optionTexts.Length; i++) optionTexts[i].gameObject.SetActive(i < choices.Length);
            open = true;
            group.alpha = 1f;
            Refresh();
        }

        void Close(int choice)
        {
            open = false;
            group.alpha = 0f;
            var cb = onChosen;
            onChosen = null;
            cb?.Invoke(choice);
        }

        void Update()
        {
            if (!open) return;
            bool pressed = false;
            foreach (var a in confirms) if (a != null && a.IsPressed()) pressed = true;
            if (waitRelease) { if (!pressed) waitRelease = false; else return; }

            if (shownChars < fullText.Length)
            {
                shownChars += Time.unscaledDeltaTime * charsPerSecond;
                // Bam xac nhan khi chu chua hien het: hien het ngay.
                if (WasConfirmPressed()) shownChars = fullText.Length;
                Refresh();
                return;
            }

            float x = move != null ? move.ReadValue<Vector2>().x : 0f;
            float sign = Mathf.Abs(x) > 0.5f ? Mathf.Sign(x) : 0f;
            if (sign != 0f && sign != lastMoveSign)
            {
                selected = (selected + (int)sign + options.Length) % options.Length;
                Refresh();
            }
            lastMoveSign = sign;

            if (WasConfirmPressed()) Close(selected);
        }

        bool WasConfirmPressed()
        {
            foreach (var a in confirms) if (a != null && a.WasPressedThisFrame()) return true;
            return false;
        }

        void Refresh()
        {
            bodyText.text = fullText.Substring(0, Mathf.Min(fullText.Length, Mathf.FloorToInt(shownChars)));
            bool ready = shownChars >= fullText.Length;
            for (int i = 0; i < options.Length && i < optionTexts.Length; i++)
            {
                bool sel = i == selected;
                optionTexts[i].text = (sel ? "> " : "   ") + options[i];
                optionTexts[i].color = ready ? (sel ? selectedColor : idleColor) : new Color(idleColor.r, idleColor.g, idleColor.b, 0.3f);
            }
        }

        void Build(Font font)
        {
            UIBuild.SetupOverlay(gameObject, 60);
            group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            // Khung: vien sang + nen toi, dat giua phia duoi man hinh.
            var border = MakeImage("Border", transform, borderColor, new Vector2(1100f, 300f));
            border.rectTransform.anchorMin = border.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            border.rectTransform.anchoredPosition = new Vector2(0f, 220f);
            var panel = MakeImage("Panel", border.transform, panelColor, new Vector2(1092f, 292f));

            nameText = UIBuild.MakeText("Name", panel.transform, font, 34, nameColor);
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.rectTransform.sizeDelta = new Vector2(1000f, 50f);
            nameText.rectTransform.anchoredPosition = new Vector2(0f, 105f);

            bodyText = UIBuild.MakeText("Body", panel.transform, font, 40, textColor);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.rectTransform.sizeDelta = new Vector2(1000f, 110f);
            bodyText.rectTransform.anchoredPosition = new Vector2(0f, 15f);

            optionTexts = new Text[4];
            for (int i = 0; i < optionTexts.Length; i++)
            {
                var t = UIBuild.MakeText("Option" + i, panel.transform, font, 38, idleColor);
                t.alignment = TextAnchor.MiddleLeft;
                t.rectTransform.sizeDelta = new Vector2(240f, 50f);
                t.rectTransform.anchoredPosition = new Vector2(-380f + i * 260f, -100f);
                optionTexts[i] = t;
            }
        }

        static Image MakeImage(string name, Transform parent, Color color, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            img.rectTransform.sizeDelta = size;
            return img;
        }
    }
}
