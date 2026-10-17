using UnityEngine;

namespace LuHanh.Core
{
    /// <summary>
    /// Noi DUY NHAT dieu khien Time.timeScale, de cac nguon dung hinh khong gianh nhau:
    /// - Hitstop (chem trung, bi danh...): dung hinh mot khoang thoi gian; yeu cau moi chi keo dai neu dai hon.
    /// - Giu dung (vd camera dang truot sang phong moi): dung cho toi khi nha ra, dem so lan giu.
    /// Game chi chay lai khi het hitstop VA khong con ai giu. Dem bang thoi gian thuc (unscaled)
    /// vi luc timeScale = 0 thi thoi gian thuong dung yen.
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        static HitStop instance;

        float endRealtime;
        bool active;
        int holds;
        float slowEndRealtime, slowScale = 1f;

        /// <summary>
        /// Tua cham: sau khi het dung hinh, game chay o toc do <paramref name="scale"/> trong <paramref name="seconds"/> giay thuc
        /// (vd nguoi choi trung don kieu Hollow Knight). Dung hinh / giu dung van uu tien hon.
        /// </summary>
        public static void SlowMotion(float scale, float seconds)
        {
            if (seconds <= 0f) return;
            var hs = Ensure();
            hs.slowScale = Mathf.Clamp(scale, 0.05f, 1f);
            hs.slowEndRealtime = Mathf.Max(hs.slowEndRealtime, Time.unscaledTime + seconds);
            hs.TryResume();
        }

        /// <summary>Dung hinh <paramref name="seconds"/> giay thuc. Tu tao object dieu khien lan dau.</summary>
        public static void Request(float seconds)
        {
            if (seconds <= 0f) return;
            Ensure().Begin(seconds);
        }

        /// <summary>Dung game cho toi khi goi <see cref="Unhold"/> (moi lan Hold phai co mot lan Unhold).</summary>
        public static void Hold()
        {
            var hs = Ensure();
            hs.holds++;
            Time.timeScale = 0f;
        }

        public static void Unhold()
        {
            if (!instance || instance.holds == 0) return;
            instance.holds--;
            instance.TryResume();
        }

        static HitStop Ensure()
        {
            if (!instance) instance = new GameObject("HitStop").AddComponent<HitStop>();
            return instance;
        }

        void Begin(float seconds)
        {
            endRealtime = Mathf.Max(endRealtime, Time.unscaledTime + seconds);
            Time.timeScale = 0f;
            active = true;
        }

        // Update van chay khi timeScale = 0 (chi deltaTime = 0).
        void Update()
        {
            if (active && Time.unscaledTime >= endRealtime)
            {
                active = false;
                TryResume();
            }
            else if (!active && holds == 0 && slowScale < 1f) TryResume(); // het tua cham thi tra ve 1
        }

        void TryResume()
        {
            if (active || holds > 0) return;
            bool slow = Time.unscaledTime < slowEndRealtime;
            if (!slow) slowScale = 1f;
            Time.timeScale = slow ? slowScale : 1f;
        }

        // Bi huy giua luc dung hinh (doi scene, thoat Play) thi khong de game dung mai.
        void OnDisable()
        {
            active = false;
            holds = 0;
            slowScale = 1f;
            slowEndRealtime = 0f;
            Time.timeScale = 1f;
        }
    }
}
