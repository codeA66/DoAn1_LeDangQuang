using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LuHanh.Core
{
    /// <summary>
    /// Trinh tu hoi sinh: man hinh toi dan -> dua nguoi choi ve checkpoint -> moi
    /// <see cref="IResettable"/> (quai...) tro ve ban dau -> sang dan.
    ///
    /// Danh sach IResettable la static va do tung object tu dang ky, nen khong phai
    /// tim bang FindObjectsOfType (cham) va quai dang tat van nam trong danh sach.
    /// </summary>
    public class RespawnManager : MonoBehaviour
    {
        static readonly List<IResettable> Resettables = new();

        public static RespawnManager Instance { get; private set; }

        public static void Register(IResettable item)
        {
            if (item != null && !Resettables.Contains(item)) Resettables.Add(item);
        }

        public static void Unregister(IResettable item) => Resettables.Remove(item);

        [Tooltip("Lop phu den toan man hinh (alpha 0 luc binh thuong).")]
        [SerializeField] CanvasGroup fade;
        [SerializeField] float fadeOutTime = 0.6f;
        [Tooltip("Giu man hinh den bao lau sau khi da dat lai moi thu.")]
        [SerializeField] float holdTime = 0.3f;
        [SerializeField] float fadeInTime = 0.5f;

        public bool IsRespawning { get; private set; }

        void Awake()
        {
            Instance = this;
            if (fade)
            {
                fade.alpha = 0f;
                fade.blocksRaycasts = false;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Bat dau hoi sinh. <paramref name="placePlayer"/> duoc goi luc man hinh den hoan toan.
        /// Dung thoi gian khong phu thuoc timeScale de van chay khi game dang khung hinh.
        /// </summary>
        public void Respawn(Action placePlayer)
        {
            if (IsRespawning) return;
            StartCoroutine(RespawnRoutine(placePlayer));
        }

        IEnumerator RespawnRoutine(Action placePlayer)
        {
            IsRespawning = true;

            yield return Fade(0f, 1f, fadeOutTime);

            placePlayer?.Invoke();
            ResetAll();

            yield return new WaitForSecondsRealtime(holdTime);
            yield return Fade(1f, 0f, fadeInTime);

            IsRespawning = false;
        }

        /// <summary>Dua moi IResettable ve ban dau. Goi duoc rieng, vd khi ngoi ghe.</summary>
        public static void ResetAll()
        {
            // Duyet nguoc phong khi ResetState lam thay doi danh sach.
            for (int i = Resettables.Count - 1; i >= 0; i--)
                Resettables[i].ResetState();
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            if (!fade) yield break;

            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                fade.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            fade.alpha = to;
        }
    }
}
