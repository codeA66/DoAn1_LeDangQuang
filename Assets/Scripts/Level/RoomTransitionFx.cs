using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Level
{
    /// <summary>
    /// Hieu ung luc camera truot sang phong moi: Volume rieng (meo ong kinh lech ve huong di, tach mau,
    /// vien toi) manh dan roi tan theo duong chuong, cong vet toc do mo luot nguoc huong di.
    /// Chay bang thoi gian thuc vi game dang dung trong luc chuyen phong.
    /// Dat duoi Main Camera (vet toc do di theo man hinh).
    /// </summary>
    public class RoomTransitionFx : MonoBehaviour
    {
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
            if (volume)
            {
                volume.weight = 0f;
                if (volume.profile) volume.profile.TryGet(out lens);
            }
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
            if (volume) volume.weight = 0f;
        }
    }
}
