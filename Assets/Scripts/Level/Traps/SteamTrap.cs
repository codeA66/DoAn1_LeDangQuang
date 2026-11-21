using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Ong hoi nong tren san: nghi -> khe do + khoi nho bao truoc -> phut hoi (gay sat thuong).
    /// Goc object = mat san giua mieng ong.
    /// </summary>
    public class SteamTrap : MonoBehaviour
    {
        [SerializeField] SpriteRenderer vent;
        [SerializeField] Sprite[] ventFrames;       // 0 nghi, 1 bao truoc
        [SerializeField] SpriteRenderer puff;
        [SerializeField] Sprite[] puffFrames;
        [SerializeField] SpriteRenderer jet;
        [SerializeField] Sprite[] jetStartFrames;
        [SerializeField] Sprite[] jetLoopFrames;
        [SerializeField] Collider2D hit;
        [SerializeField] float offTime = 1.6f;
        [SerializeField] float warnTime = 0.6f;
        [SerializeField] float onTime = 1.4f;
        [SerializeField] float offset;
        [SerializeField] float fps = 12f;

        void Update()
        {
            float t = TrapClock.Phase(offTime + warnTime + onTime, offset);
            bool warn = t >= offTime && t < offTime + warnTime;
            bool on = t >= offTime + warnTime;
            if (vent && ventFrames != null && ventFrames.Length > 1) vent.sprite = ventFrames[warn || on ? 1 : 0];
            if (puff)
            {
                puff.enabled = warn;
                if (warn) puff.sprite = TrapClock.Frame(puffFrames, t - offTime, fps);
            }
            if (jet)
            {
                jet.enabled = on;
                if (on)
                {
                    float jt = t - offTime - warnTime;
                    int startCount = jetStartFrames != null ? jetStartFrames.Length : 0;
                    jet.sprite = jt * fps < startCount
                        ? TrapClock.Frame(jetStartFrames, jt, fps, false)
                        : TrapClock.Frame(jetLoopFrames, jt - startCount / fps, fps);
                }
            }
            if (hit && hit.enabled != on) hit.enabled = on;
        }
    }
}
