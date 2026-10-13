using LuHanh.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LuHanh.UI
{
    /// <summary>
    /// Hang nac nang luong ban cau (duoi hang mau). Tu tao cac nac (Image) mot lan luc Start theo
    /// <see cref="PlayerEnergy.Max"/>; nac day sang xanh ngoc, nac trong toi. Du nang luong ban thi
    /// cac nac day nhap nhay nhe. An khi chua mo ky nang ban.
    /// </summary>
    public class EnergyBarUI : MonoBehaviour
    {
        [SerializeField] Vector2 pipSize = new Vector2(22f, 22f);
        [SerializeField] float spacing = 8f;
        [SerializeField] Color fullColor = new Color(0.27f, 0.86f, 0.77f, 1f);    // #45DCC4
        [SerializeField] Color readyColor = new Color(0.72f, 1f, 0.94f, 1f);      // #B8FFF0
        [SerializeField] Color emptyColor = new Color(0.16f, 0.15f, 0.2f, 0.85f); // #292734
        [SerializeField] float pulseSpeed = 5f;

        PlayerEnergy energy;
        Image[] pips;
        CanvasGroup group;

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p) energy = p.GetComponent<PlayerEnergy>();
            if (!energy) { gameObject.SetActive(false); return; }

            group = GetComponent<CanvasGroup>();
            if (!group) group = gameObject.AddComponent<CanvasGroup>();
            pips = new Image[energy.Max];
            for (int i = 0; i < pips.Length; i++)
            {
                var go = new GameObject("Pip" + i, typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(transform, false);
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0.5f);
                rt.sizeDelta = pipSize;
                rt.anchoredPosition = new Vector2(i * (pipSize.x + spacing), 0f);
                rt.localRotation = Quaternion.Euler(0f, 0f, 45f);   // nac hinh thoi
                pips[i] = go.GetComponent<Image>();
                pips[i].raycastTarget = false;
            }
            PlayerSkills.Unlocked += OnSkill;
            group.alpha = PlayerSkills.Has(Skill.OrbShot) ? 1f : 0f;
        }

        void OnDestroy() => PlayerSkills.Unlocked -= OnSkill;

        void OnSkill(Skill s) { if (s == Skill.OrbShot && group) group.alpha = 1f; }

        void Update()
        {
            if (!energy || pips == null) return;
            bool ready = energy.CanShoot;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
            for (int i = 0; i < pips.Length; i++)
            {
                bool full = i < energy.Current;
                pips[i].color = !full ? emptyColor : ready ? Color.Lerp(fullColor, readyColor, pulse) : fullColor;
            }
        }
    }
}
