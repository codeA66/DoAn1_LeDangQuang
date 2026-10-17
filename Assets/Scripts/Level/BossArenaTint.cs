using LuHanh.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Level
{
    /// <summary>
    /// Đổi màu nền / sương / đèn chung khi trận boss bắt đầu (gọi <see cref="FadeIn"/> từ RoomEncounter.onStarted)
    /// và trả lại màu cũ khi xong trận (<see cref="FadeOut"/> từ onCleared). Người chơi chết: trả màu cũ ngay.
    /// Màu gốc đọc lúc Awake nên không phải ghi lại màu của màn.
    /// </summary>
    public class BossArenaTint : MonoBehaviour, IResettable
    {
        [System.Serializable]
        public class SpriteTarget
        {
            public SpriteRenderer renderer;
            public Color fightColor = new(0.165f, 0.055f, 0.078f); // #2A0E14
            [HideInInspector] public Color original;
        }

        [System.Serializable]
        public class LightTarget
        {
            public Light2D light;
            public Color fightColor = new(0.78f, 0.56f, 0.6f);
            [Min(0f)] public float fightIntensity = 0.55f;
            [HideInInspector] public Color original;
            [HideInInspector] public float originalIntensity;
        }

        [SerializeField] SpriteTarget[] sprites;
        [SerializeField] LightTarget[] lights;
        [SerializeField] float fadeInTime = 1f;
        [SerializeField] float fadeOutTime = 2f;

        float k, from, to, timer, duration;

        void Awake()
        {
            foreach (var s in sprites) if (s.renderer) s.original = s.renderer.color;
            foreach (var l in lights)
                if (l.light) { l.original = l.light.color; l.originalIntensity = l.light.intensity; }
            RespawnManager.Register(this);
        }

        void OnDestroy() => RespawnManager.Unregister(this);

        public void FadeIn() => Fade(1f, fadeInTime);
        public void FadeOut() => Fade(0f, fadeOutTime);

        public void ResetState()
        {
            timer = duration = 0f;
            Apply(0f);
        }

        void Fade(float target, float time)
        {
            from = k; to = target; timer = 0f; duration = Mathf.Max(0.01f, time);
            enabled = true;
        }

        void Update()
        {
            if (duration <= 0f) return;
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            Apply(Mathf.Lerp(from, to, t * t * (3f - 2f * t)));
            if (t >= 1f) duration = 0f;
        }

        void Apply(float value)
        {
            k = value;
            foreach (var s in sprites)
                if (s.renderer) s.renderer.color = Color.Lerp(s.original, s.fightColor, k);
            foreach (var l in lights)
            {
                if (!l.light) continue;
                l.light.color = Color.Lerp(l.original, l.fightColor, k);
                l.light.intensity = Mathf.Lerp(l.originalIntensity, l.fightIntensity, k);
            }
        }
    }
}
