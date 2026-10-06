using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Giu hat (World space) luon nam trong hop quanh emitter: hat troi ra khoi mot mep thi dua sang
    /// mep doi dien. Dung cho bui lo lung bam theo camera - di bao xa mat do van deu, hat van troi
    /// nguoc lai khi camera chay (co chieu sau), khong can sinh them hat.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleWrap : MonoBehaviour
    {
        [Tooltip("Kich thuoc hop (x, y). De 0 thi lay theo Shape > Scale cua Particle System.")]
        [SerializeField] Vector2 size;

        ParticleSystem ps;
        ParticleSystem.Particle[] buffer;

        void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            buffer = new ParticleSystem.Particle[ps.main.maxParticles];
            if (size == Vector2.zero) size = ps.shape.scale;
        }

        void LateUpdate()
        {
            int count = ps.GetParticles(buffer);
            if (count == 0) return;

            Vector2 centre = transform.TransformPoint(ps.shape.position);
            Vector2 half = size * 0.5f;
            for (int i = 0; i < count; i++)
            {
                var p = buffer[i].position;
                p.x = centre.x + Wrap(p.x - centre.x, half.x);
                p.y = centre.y + Wrap(p.y - centre.y, half.y);
                buffer[i].position = p;
            }
            ps.SetParticles(buffer, count);
        }

        // Dua d ve khoang [-half, half).
        static float Wrap(float d, float half)
        {
            float span = half * 2f;
            return d - Mathf.Floor((d + half) / span) * span;
        }
    }
}
