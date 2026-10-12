using LuHanh.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Level
{
    /// <summary>
    /// Anh sang phong boss kieu Hollow Knight: cot sang tu tran roi xuong san dau.
    /// Truoc tran mo (idleLevel), cua sap thi bung sang (<see cref="Brighten"/>), boss chet thi diu xuong (<see cref="Dim"/>).
    /// Nhan tung muc voi cuong do / do trong goc cua den va cot sang. Nguoi choi chet: ve muc truoc tran.
    /// </summary>
    public class BossArenaLights : MonoBehaviour, IResettable
    {
        [SerializeField] Light2D[] lights;
        [Tooltip("Sprite cot sang (moon_beam), doi do trong theo muc.")]
        [SerializeField] SpriteRenderer[] beams;
        [Range(0f, 1f)] [SerializeField] float idleLevel = 0.3f;
        [Range(0f, 1f)] [SerializeField] float fightLevel = 1f;
        [Range(0f, 1f)] [SerializeField] float clearedLevel = 0.6f;
        [Tooltip("Giay de bung sang khi cua sap.")]
        [SerializeField] float brightenTime = 0.4f;
        [Tooltip("Giay de diu xuong khi boss chet.")]
        [SerializeField] float dimTime = 1.5f;

        float[] baseIntensity;
        float[] baseAlpha;
        float level, target, speed;

        void Awake()
        {
            RespawnManager.Register(this);
            baseIntensity = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++) if (lights[i]) baseIntensity[i] = lights[i].intensity;
            baseAlpha = new float[beams.Length];
            for (int i = 0; i < beams.Length; i++) if (beams[i]) baseAlpha[i] = beams[i].color.a;
            SetLevel(idleLevel);
            target = idleLevel;
        }

        void OnDestroy() => RespawnManager.Unregister(this);

        public void Brighten() => GoTo(fightLevel, brightenTime);
        public void Dim() => GoTo(clearedLevel, dimTime);

        public void ResetState()
        {
            target = idleLevel;
            SetLevel(idleLevel);
        }

        void GoTo(float value, float time)
        {
            target = value;
            speed = Mathf.Abs(value - level) / Mathf.Max(0.01f, time);
        }

        void Update()
        {
            if (Mathf.Approximately(level, target)) return;
            SetLevel(Mathf.MoveTowards(level, target, speed * Time.deltaTime));
        }

        void SetLevel(float k)
        {
            level = k;
            for (int i = 0; i < lights.Length; i++)
                if (lights[i]) lights[i].intensity = baseIntensity[i] * k;
            for (int i = 0; i < beams.Length; i++)
            {
                if (!beams[i]) continue;
                var c = beams[i].color;
                c.a = baseAlpha[i] * k;
                beams[i].color = c;
            }
        }
    }
}
