using System.Collections.Generic;
using LuHanh.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace LuHanh.UI
{
    /// <summary>
    /// Hang mask mau kieu Hollow Knight o goc man hinh. Tao du so mask mot lan luc dau,
    /// sau do chi doi sprite khi mau doi (nghe su kien, khong kiem tra moi frame).
    /// Mask vua mat thi nay to len roi thu ve - cam giac "vo".
    /// </summary>
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] Health health;
        [Tooltip("Image mau (tat san); duoc nhan ban thanh tung mask.")]
        [SerializeField] Image maskTemplate;
        [SerializeField] Sprite fullSprite;
        [SerializeField] Sprite emptySprite;
        [SerializeField] float popScale = 1.4f;
        [SerializeField] float popTime = 0.2f;

        readonly List<Image> masks = new();
        int shown = -1;
        int poppingIndex = -1;
        float popTimer;

        void Start()
        {
            if (!health || !maskTemplate) return;
            maskTemplate.gameObject.SetActive(false);
            Build(health.Max);
            Refresh(health.Current, health.Max);
            health.Changed += Refresh;
        }

        void OnDestroy()
        {
            if (health) health.Changed -= Refresh;
        }

        void Build(int count)
        {
            while (masks.Count < count)
            {
                var img = Instantiate(maskTemplate, maskTemplate.transform.parent);
                img.gameObject.SetActive(true);
                masks.Add(img);
            }
            for (int i = 0; i < masks.Count; i++) masks[i].gameObject.SetActive(i < count);
        }

        void Refresh(int current, int max)
        {
            Build(max);
            // Mat mau: mask cuoi cung vua mat se nay len.
            if (shown >= 0 && current < shown)
            {
                poppingIndex = current;
                popTimer = popTime;
            }
            shown = current;

            for (int i = 0; i < max; i++)
                masks[i].sprite = i < current ? fullSprite : emptySprite;
        }

        void Update()
        {
            if (poppingIndex < 0) return;

            // Dung thoi gian thuc: van chay luc game dang khung hinh.
            popTimer -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(popTimer / popTime);
            masks[poppingIndex].transform.localScale = Vector3.one * Mathf.Lerp(1f, popScale, k);
            if (popTimer <= 0f) poppingIndex = -1;
        }
    }
}
