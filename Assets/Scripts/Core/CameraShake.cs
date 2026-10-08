using Unity.Cinemachine;
using UnityEngine;

namespace LuHanh.Core
{
    /// <summary>
    /// Rung camera, gan len CinemachineCamera (trong CameraRig) sau RoomCameraClamp.
    /// Hai kieu: <see cref="Hold"/> rung lien tuc theo cuong do (vd dang tu luc) va <see cref="Kick"/>
    /// rung manh mot lan roi tat dan (vd bien hinh, no). Lay cuong do lon nhat cua hai kieu.
    /// Chay bang thoi gian thuc nen van rung trong luc HitStop.
    /// </summary>
    public class CameraShake : CinemachineExtension
    {
        static CameraShake instance;

        [Tooltip("Toc do dao dong (Perlin).")]
        [SerializeField] float frequency = 28f;
        [Tooltip("Gioi han do lech toi da (unit).")]
        [SerializeField] float maxAmplitude = 0.6f;

        float hold;
        float kick, kickTime, kickDuration = 1f;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (instance == this) instance = null;
        }

        /// <summary>Rung lien tuc voi bien do <paramref name="amplitude"/> (unit). 0 = tat.</summary>
        public static void Hold(float amplitude)
        {
            if (instance) instance.hold = Mathf.Max(0f, amplitude);
        }

        /// <summary>Rung manh <paramref name="amplitude"/> roi tat dan trong <paramref name="duration"/> giay.</summary>
        public static void Kick(float amplitude, float duration)
        {
            if (!instance) return;
            instance.kick = amplitude;
            instance.kickDuration = Mathf.Max(0.01f, duration);
            instance.kickTime = Time.unscaledTime;
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Noise) return;
            float k = kick > 0f ? Mathf.Clamp01((Time.unscaledTime - kickTime) / kickDuration) : 1f;
            float amp = Mathf.Min(maxAmplitude, Mathf.Max(hold, kick * (1f - k) * (1f - k)));
            // Chan NaN: mot gia tri hong se lam vo vi tri camera (loi "Invalid AABB").
            if (!(amp > 0.0001f)) return;
            float t = Time.unscaledTime * frequency;
            var offset = new Vector3(Mathf.PerlinNoise(t, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, t) - 0.5f, 0f) * 2f * amp;
            state.PositionCorrection += offset;
        }
    }
}
