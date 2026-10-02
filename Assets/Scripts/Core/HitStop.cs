using UnityEngine;

namespace LuHanh.Core
{
    /// <summary>
    /// Khung hinh dung lai mot thoang (hitstop) - dung chung cho moi nguon (chem trung, bi danh...).
    /// Chi MOT noi dieu khien Time.timeScale nen cac yeu cau chong nhau khong gianh nhau:
    /// yeu cau moi chi keo dai thoi diem ket thuc neu dai hon. Dem bang thoi gian thuc
    /// (unscaled) vi luc timeScale = 0 thi thoi gian thuong dung yen.
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        static HitStop instance;

        float endRealtime;
        bool active;

        /// <summary>Dung hinh <paramref name="seconds"/> giay thuc. Tu tao object dieu khien lan dau.</summary>
        public static void Request(float seconds)
        {
            if (seconds <= 0f) return;
            if (!instance) instance = new GameObject("HitStop").AddComponent<HitStop>();
            instance.Begin(seconds);
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
            if (active && Time.unscaledTime >= endRealtime) Release();
        }

        void Release()
        {
            Time.timeScale = 1f;
            active = false;
        }

        // Bi huy giua luc dung hinh (doi scene, thoat Play) thi khong de game dung mai.
        void OnDisable()
        {
            if (active) Release();
        }
    }
}
