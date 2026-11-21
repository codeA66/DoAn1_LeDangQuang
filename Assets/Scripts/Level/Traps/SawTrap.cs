using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Cua chay qua lai tren ray, tu A toi B (toa do cuc bo). Luoi cua la con <see cref="blade"/>
    /// co collider trigger + HazardZone, nen chem xuong (Pogo) nay duoc tren luoi cua.
    /// </summary>
    public class SawTrap : MonoBehaviour
    {
        [SerializeField] Transform blade;
        [SerializeField] Vector2 pointA = new(-2f, 0f);
        [SerializeField] Vector2 pointB = new(2f, 0f);
        [Tooltip("Unit/giay.")]
        [SerializeField] float speed = 3.5f;
        [Tooltip("Do/giay, am = quay theo chieu kim dong ho.")]
        [SerializeField] float spinSpeed = -540f;
        [Tooltip("Lech pha 0..1 de nhieu cua khong chay giong nhau.")]
        [SerializeField, Range(0f, 1f)] float phase;

        void Update()
        {
            if (!blade) return;
            float len = Vector2.Distance(pointA, pointB);
            float t = len > 0.01f ? Mathf.PingPong(Time.time * speed / len + phase * 2f, 1f) : 0f;
            blade.localPosition = Vector2.Lerp(pointA, pointB, Mathf.SmoothStep(0f, 1f, t));
            blade.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.92f, 0.4f, 0.28f, 0.9f);
            Gizmos.DrawLine(transform.TransformPoint(pointA), transform.TransformPoint(pointB));
        }
    }
}
