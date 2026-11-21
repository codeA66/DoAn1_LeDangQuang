using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>Dong ho chung cho bay theo nhip: giay da troi trong chu ky (co lech pha) va chon khung hinh.</summary>
    public static class TrapClock
    {
        public static float Phase(float period, float offset)
        {
            if (period <= 0f) return 0f;
            return Mathf.Repeat(Time.time + offset, period);
        }

        public static Sprite Frame(Sprite[] frames, float t, float fps, bool loop = true)
        {
            if (frames == null || frames.Length == 0) return null;
            int i = Mathf.FloorToInt(Mathf.Max(0f, t) * fps);
            i = loop ? i % frames.Length : Mathf.Min(i, frames.Length - 1);
            return frames[i];
        }
    }
}
