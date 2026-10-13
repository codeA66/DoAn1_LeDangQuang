using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Hieu ung luc ban cau, ve tay theo frame (CastBurst + CastWave). Cac phan la FrameAnimation con,
    /// chia 2 nhom: thuong va Shadow (Shadow co khoi tim den). Lat theo huong ban, phat xong tu huy.
    /// </summary>
    public class CastFx : MonoBehaviour
    {
        [SerializeField] FrameAnimation[] normalParts;
        [SerializeField] FrameAnimation[] shadowParts;

        /// <summary>Phat theo huong ngang <paramref name="dirX"/> (-1 / 1).</summary>
        public void Play(float dirX, bool shadowForm)
        {
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * Mathf.Sign(dirX);
            transform.localScale = s;

            float longest = 0f;
            foreach (var part in shadowForm ? shadowParts : normalParts)
            {
                if (!part) continue;
                part.Play();
                longest = Mathf.Max(longest, part.Duration);
            }
            Destroy(gameObject, longest + 0.05f);
        }
    }
}
