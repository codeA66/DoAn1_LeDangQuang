using UnityEngine;
using UnityEngine.UI;

namespace LuHanh.UI
{
    /// <summary>Tien ich dung UI bang code (the ten man, ten boss, loi huong dan): canvas phu man hinh, chu, chu hoa kieu Hollow Knight.</summary>
    public static class UIBuild
    {
        /// <summary>Gan Canvas Screen Space Overlay + CanvasScaler 1920x1080 len <paramref name="go"/>.</summary>
        public static Canvas SetupOverlay(GameObject go, int sortingOrder)
        {
            var canvas = go.GetComponent<Canvas>();
            if (!canvas) canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            if (!scaler) scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            return canvas;
        }

        /// <summary>Chu can giua, tran ra ngoai khung (font co ascent lon se bi cat neu Truncate).</summary>
        public static Text MakeText(string name, Transform parent, Font font, int size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(1600f, 180f);
            var t = go.GetComponent<Text>();
            t.font = font ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>Viet hoa, chu cai dau moi tu co <paramref name="capSize"/> (kieu "FOG CANYON" cua Hollow Knight). Can supportRichText.</summary>
        public static string SmallCaps(string title, int capSize)
        {
            var sb = new System.Text.StringBuilder();
            bool wordStart = true;
            foreach (char ch in title.ToUpperInvariant())
            {
                if (char.IsWhiteSpace(ch)) { sb.Append(ch); wordStart = true; continue; }
                if (wordStart) sb.Append("<size=").Append(capSize).Append('>').Append(ch).Append("</size>");
                else sb.Append(ch);
                wordStart = false;
            }
            return sb.ToString();
        }
    }
}
