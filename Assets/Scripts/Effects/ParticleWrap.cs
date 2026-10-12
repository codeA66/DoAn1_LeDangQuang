using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Giu hat (World space) luon nam trong hop quanh emitter: hat troi ra khoi mot mep thi dua sang
    /// mep doi dien. Dung cho bui lo lung bam theo camera - di bao xa mat do van deu, hat van troi
    /// nguoc lai khi camera chay (co chieu sau), khong can sinh them hat.
    /// Tuy chon parallax cho hat (kieu Hollow Knight co hat xa / hat gan man hinh):
    /// emitter bam theo camera, va moi hat duoc day them mot phan quang duong camera vua di.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleWrap : MonoBehaviour
    {
        [Tooltip("Kich thuoc hop (x, y). De 0 thi lay theo Shape > Scale cua Particle System.")]
        [SerializeField] Vector2 size;
        [Tooltip("Emitter (va hop) luon dat o vi tri camera.")]
        [SerializeField] bool followCamera;
        [Tooltip("Hat di theo camera bao nhieu phan: 0 = dung yen trong the gioi (cung do sau voi nguoi choi), " +
                 "duong = troi cham hon the gioi (o xa), am = troi nhanh hon (sat man hinh).")]
        [Range(-1f, 1f)] [SerializeField] float cameraFollow;

        ParticleSystem ps;
        ParticleSystem.Particle[] buffer;
        Transform cam;
        Vector3 lastCam;

        void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            buffer = new ParticleSystem.Particle[ps.main.maxParticles];
            if (size == Vector2.zero) size = ps.shape.scale;
        }

        void Start()
        {
            if (Camera.main) cam = Camera.main.transform;
            if (cam) lastCam = cam.position;
        }

        void LateUpdate()
        {
            Vector3 delta = Vector3.zero;
            if (cam)
            {
                delta = cam.position - lastCam;
                lastCam = cam.position;
                if (followCamera) transform.position = new Vector3(cam.position.x, cam.position.y, transform.position.z);
            }

            int count = ps.GetParticles(buffer);
            if (count == 0) return;

            Vector2 centre = transform.TransformPoint(ps.shape.position);
            Vector2 half = size * 0.5f;
            Vector2 shift = (Vector2)delta * cameraFollow;
            for (int i = 0; i < count; i++)
            {
                var p = buffer[i].position;
                p.x = centre.x + Wrap(p.x + shift.x - centre.x, half.x);
                p.y = centre.y + Wrap(p.y + shift.y - centre.y, half.y);
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
