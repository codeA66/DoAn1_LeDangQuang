using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Level
{
    /// <summary>
    /// Lop phu toi/sang tao san cho chuyen phong, chay bang thoi gian thuc.
    /// Giu hieu ung truot cu de prefab chua gan fade van hoat dong trong luc cho duyet preview.
    /// </summary>
    public class RoomTransitionFx : MonoBehaviour
    {
        [Header("Chuyen phong toi / sang")]
        [Tooltip("Lop phu den tao san trong CameraRig. De trong de giu kieu truot cu cho prefab chua chuyen doi.")]
        [SerializeField] CanvasGroup fade;
        [Tooltip("Thoi gian toi dan truoc khi dat camera vao phong moi (giay thuc).")]
        [SerializeField, Min(0f)] float fadeOutTime = 0.15f;
        [Tooltip("Thoi gian sang lai sau khi camera da on dinh (giay thuc).")]
        [SerializeField, Min(0f)] float fadeInTime = 0.2f;

        public bool HasFade => isActiveAndEnabled && fade && fade.gameObject.activeInHierarchy;

        [Tooltip("Volume toan cuc rieng (priority cao hon Volume chinh), weight 0 luc binh thuong.")]
        [SerializeField] Volume volume;
        [Tooltip("Tam meo ong kinh lech ve huong di bao nhieu (0..0.5 man hinh).")]
        [SerializeField] float distortionShift = 0.18f;
        [Tooltip("Do manh cao nhat (0..1) cua Volume o giua luc truot.")]
        [Range(0f, 1f)] [SerializeField] float peakWeight = 1f;

        [Header("Vet toc do")]
        [Tooltip("Particle System con (Local space, Use Unscaled Time, Stretched Billboard).")]
        [SerializeField] ParticleSystem streaks;
        [SerializeField] int streakCount = 18;
        [SerializeField] float streakSpeed = 45f;

        LensDistortion lens;
        Coroutine running;

        void Awake()
        {
            ResetFade();
            if (volume)
            {
                volume.weight = 0f;
                if (volume.profile) volume.profile.TryGet(out lens);
            }
        }

        /// <summary>Toi hoan toan truoc khi doi phong; khong phat meo hinh hay vet toc do.</summary>
        public IEnumerator FadeOut()
        {
            if (running != null) StopCoroutine(running);
            running = null;
            if (volume) volume.weight = 0f;
            if (streaks) streaks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            yield return FadeTo(1f, fadeOutTime);
        }

        public IEnumerator FadeIn() => FadeTo(0f, fadeInTime);

        IEnumerator FadeTo(float target, float duration)
        {
            if (!HasFade) yield break;
            float from = fade.alpha;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                if (!HasFade) yield break;
                float k = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                fade.alpha = Mathf.Lerp(from, target, k);
                yield return null;
            }
            if (fade) fade.alpha = target;
        }

        /// <summary>Don lop phu khi chuyen xong hoac bi huy giua chung.</summary>
        public void ResetFade()
        {
            if (!fade) return;
            fade.alpha = 0f;
            fade.blocksRaycasts = false;
            fade.interactable = false;
        }

        /// <summary>Phat hieu ung theo huong camera di (<paramref name="direction"/> don vi) trong <paramref name="duration"/> giay thuc.</summary>
        public void Play(Vector2 direction, float duration)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Run(direction, Mathf.Max(0.05f, duration)));
        }

        IEnumerator Run(Vector2 direction, float duration)
        {
            if (lens) lens.center.Override(new Vector2(0.5f, 0.5f) + direction * distortionShift);
            EmitStreaks(direction, duration);

            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                // Duong chuong: 0 -> 1 o giua -> 0, dao ham bang 0 o hai dau nen bat/tat em.
                float k = Mathf.Sin(Mathf.PI * (t / duration));
                if (volume) volume.weight = peakWeight * k * k;
                yield return null;
            }
            if (volume) volume.weight = 0f;
            running = null;
        }

        void EmitStreaks(Vector2 direction, float duration)
        {
            if (!streaks || streakCount <= 0) return;
            var main = streaks.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(duration * 0.5f, duration * 0.9f);
            // Camera di ve phia direction nen canh vat (va vet) troi nguoc lai.
            var vel = streaks.velocityOverLifetime;
            vel.x = new ParticleSystem.MinMaxCurve(-direction.x * streakSpeed);
            vel.y = new ParticleSystem.MinMaxCurve(-direction.y * streakSpeed);
            vel.z = new ParticleSystem.MinMaxCurve(0f);
            streaks.Emit(streakCount);
        }

        void OnDisable()
        {
            StopAllCoroutines();
            running = null;
            ResetFade();
            if (volume) volume.weight = 0f;
        }
    }
}
