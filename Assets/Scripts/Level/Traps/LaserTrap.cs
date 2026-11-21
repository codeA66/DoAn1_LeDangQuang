using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Laser theo nhip: tat -> tia mo bao truoc -> ban. Chi gay sat thuong luc ban (collider HazardZone bat).
    /// Goc object = mieng den; tia chieu theo truc x cuc bo (xoay object de chieu doc).
    /// </summary>
    public class LaserTrap : MonoBehaviour
    {
        [SerializeField] SpriteRenderer emitter;
        [SerializeField] Sprite[] emitterFrames;     // 0 tat, 1 sang
        [SerializeField] SpriteRenderer beam;
        [SerializeField] Sprite[] beamFrames;
        [SerializeField] SpriteRenderer warning;
        [SerializeField] Collider2D hit;
        [SerializeField] float offTime = 1.4f;
        [SerializeField] float warnTime = 0.6f;
        [SerializeField] float onTime = 1.2f;
        [SerializeField] float offset;
        [SerializeField] float fps = 14f;

        void Update()
        {
            float t = TrapClock.Phase(offTime + warnTime + onTime, offset);
            bool warn = t >= offTime && t < offTime + warnTime;
            bool on = t >= offTime + warnTime;
            if (warning) warning.enabled = warn;
            if (beam)
            {
                beam.enabled = on;
                if (on) beam.sprite = TrapClock.Frame(beamFrames, t, fps);
            }
            if (hit && hit.enabled != on) hit.enabled = on;
            if (emitter && emitterFrames != null && emitterFrames.Length > 1) emitter.sprite = emitterFrames[warn || on ? 1 : 0];
        }
    }
}
