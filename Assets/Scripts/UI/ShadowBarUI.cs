using LuHanh.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LuHanh.UI
{
    /// <summary>
    /// Binh nang luong Shadow kieu Soul Vessel cua Hollow Knight, dat ben trai hang mask mau.
    /// Chat long (Image Filled Vertical) dang len theo nang luong, mat nuoc gon song o dinh.
    /// Day binh thi vien sang nhap nhay moi giu phim; dang giu phim thi vong sang chay quanh binh
    /// (Filled Radial); dang Shadow thi chat long sang hon va can dan.
    /// An khi chua mo khoa. Nghe su kien cua <see cref="PlayerShadow"/>, chi lam hieu ung moi frame.
    /// </summary>
    public class ShadowBarUI : MonoBehaviour
    {
        [SerializeField] CanvasGroup group;
        [Tooltip("Chat long: Image Type = Filled, Vertical, Origin Bottom.")]
        [SerializeField] Image fill;
        [Tooltip("Mat nuoc: dai sang nam o muc chat long, gon song nhe. De trong thi bo qua.")]
        [SerializeField] RectTransform surface;
        [Tooltip("Vong sang chay quanh binh theo tien do giu phim (Filled, Radial 360).")]
        [SerializeField] Image holdFill;
        [Tooltip("Vien sang nhap nhay khi day thanh / dang Shadow.")]
        [SerializeField] Image glow;
        [SerializeField] Color fillColor = new Color(0.27f, 0.86f, 0.77f, 1f);     // #45DCC4
        [SerializeField] Color activeColor = new Color(0.72f, 1f, 0.94f, 1f);     // #B8FFF0
        [SerializeField] float pulseSpeed = 5f;
        [Tooltip("Toc do thanh chay toi gia tri moi (cang lon cang nhanh).")]
        [SerializeField] float fillSmooth = 12f;

        PlayerShadow shadow;
        float target;
        float hold;

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p) shadow = p.GetComponent<PlayerShadow>();
            if (!shadow) { gameObject.SetActive(false); return; }

            shadow.EnergyChanged += OnEnergy;
            shadow.HoldChanged += OnHold;
            shadow.Unlocked += Show;
            target = shadow.Energy01;
            if (fill) fill.fillAmount = target;
            OnHold(0f);
            if (group) group.alpha = shadow.IsUnlocked ? 1f : 0f;
        }

        void OnDestroy()
        {
            if (!shadow) return;
            shadow.EnergyChanged -= OnEnergy;
            shadow.HoldChanged -= OnHold;
            shadow.Unlocked -= Show;
        }

        void Show() { if (group) group.alpha = 1f; }
        void OnEnergy(float value) => target = value;
        void OnHold(float value) => hold = value;

        void Update()
        {
            if (!shadow || !fill) return;
            // Tu luc: chat long bi rut can dan vao nguoi choi theo tien do giu phim (tha ra thi dang lai).
            // Dang Shadow: binh da can, vong sang dem nguoc thoi gian con lai (Energy01 dang giam dan).
            float shown = shadow.IsActive ? 0f : target * (1f - hold);
            fill.fillAmount = Mathf.Lerp(fill.fillAmount, shown, 1f - Mathf.Exp(-fillSmooth * Time.unscaledDeltaTime));
            if (holdFill) holdFill.fillAmount = shadow.IsActive ? shadow.Energy01 : hold;
            fill.color = shadow.IsActive ? activeColor : fillColor;
            UpdateSurface();

            if (!glow) return;
            bool lit = shadow.IsActive || shadow.IsFull;
            var c = glow.color;
            c.a = lit ? 0.35f + 0.35f * Mathf.Sin(Time.unscaledTime * pulseSpeed) : 0f;
            glow.color = c;
        }

        /// <summary>Dat mat nuoc o muc chat long; hep lai gan day/mieng binh vi binh tron.</summary>
        void UpdateSurface()
        {
            if (!surface) return;
            float k = fill.fillAmount;
            bool visible = k > 0.02f && k < 0.98f;
            surface.gameObject.SetActive(visible);
            if (!visible) return;
            var area = fill.rectTransform.rect;
            float y = area.yMin + area.height * k;
            // Do rong day cung cua hinh tron tai do cao nay.
            float r = area.height * 0.5f, dy = y - area.center.y;
            float width = 2f * Mathf.Sqrt(Mathf.Max(0f, r * r - dy * dy)) * 0.92f;
            surface.anchoredPosition = new Vector2(0f, y + Mathf.Sin(Time.unscaledTime * 3f) * 1.5f);
            surface.sizeDelta = new Vector2(width, surface.sizeDelta.y);
            surface.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.unscaledTime * 1.7f) * 3f);
        }
    }
}
