using System.Collections.Generic;
using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Do toi chung cua man do quai gay ra (Lampsnuffer hut sang). Moi nguon dang ky mot muc toi (0..1);
    /// <see cref="TargetFactor"/> = 1 - tong, khong thap hon <see cref="MinFactor"/>.
    /// <see cref="DamageLightDip"/> nhan he so nay vao den Global va bat vang sang quanh nguoi choi.
    /// </summary>
    public static class AmbientDarkness
    {
        /// <summary>Toi nhat con lai bao nhieu phan anh sang chung.</summary>
        public const float MinFactor = 0.3f;

        static readonly Dictionary<Object, float> sources = new Dictionary<Object, float>();

        public static float TargetFactor { get; private set; } = 1f;

        public static void Set(Object source, float amount)
        {
            if (amount <= 0f) sources.Remove(source);
            else sources[source] = amount;
            Recalculate();
        }

        public static void Remove(Object source)
        {
            if (sources.Remove(source)) Recalculate();
        }

        static void Recalculate()
        {
            float sum = 0f;
            foreach (var kv in sources) sum += kv.Value;
            TargetFactor = Mathf.Max(MinFactor, 1f - sum);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            sources.Clear();
            TargetFactor = 1f;
        }
    }
}
