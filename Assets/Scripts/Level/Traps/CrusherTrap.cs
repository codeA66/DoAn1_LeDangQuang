using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Bua dap: dau bua treo duoi goc object (tran). Cho -> den bao + rung -> dap xuong <see cref="travel"/> unit
    /// -> giu -> keo len cham. Collider HazardZone o mat duoi dau bua chi bat luc dang dap / nam duoi.
    /// Truc piston (SpriteRenderer Tiled, pivot giua) keo dai tu goc toi dau bua.
    /// </summary>
    public class CrusherTrap : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] SpriteRenderer headRenderer;
        [SerializeField] Sprite[] headFrames;       // 0 thuong, 1 den bao
        [SerializeField] SpriteRenderer shaft;
        [SerializeField] Collider2D hit;
        [SerializeField] float travel = 6f;
        [SerializeField] float waitTop = 1.3f;
        [SerializeField] float warnTime = 0.5f;
        [SerializeField] float slamSpeed = 26f;
        [SerializeField] float holdBottom = 0.35f;
        [SerializeField] float riseSpeed = 4f;
        [SerializeField] float offset;
        [SerializeField] float shakeAmount = 0.06f;
        [SerializeField] float impactShake = 0.12f;

        float topY;
        bool wasDown;

        void Awake() => topY = head ? head.localPosition.y : 0f;

        void Update()
        {
            if (!head) return;
            float slam = travel / Mathf.Max(0.1f, slamSpeed), rise = travel / Mathf.Max(0.1f, riseSpeed);
            float t = TrapClock.Phase(waitTop + warnTime + slam + holdBottom + rise, offset);
            float y;
            bool warn = false, deadly = false;
            if (t < waitTop) y = 0f;
            else if (t < waitTop + warnTime) { y = 0f; warn = true; }
            else if (t < waitTop + warnTime + slam) { y = (t - waitTop - warnTime) / slam * travel; deadly = true; }
            else if (t < waitTop + warnTime + slam + holdBottom) { y = travel; deadly = true; }
            else y = travel * (1f - (t - waitTop - warnTime - slam - holdBottom) / rise);

            float jitter = warn ? Random.Range(-shakeAmount, shakeAmount) : 0f;
            head.localPosition = new Vector3(jitter, topY - y, 0f);
            if (headRenderer && headFrames != null && headFrames.Length > 1) headRenderer.sprite = headFrames[warn || deadly ? 1 : 0];
            if (hit && hit.enabled != deadly) hit.enabled = deadly;
            if (shaft)
            {
                float len = Mathf.Max(0.01f, -(topY - y));
                shaft.size = new Vector2(shaft.size.x, len);
                shaft.transform.localPosition = new Vector3(0f, -len * 0.5f, 0f);
            }

            bool down = y >= travel - 0.001f;
            if (down && !wasDown && impactShake > 0f && headRenderer && headRenderer.isVisible) CameraShake.Kick(impactShake, 0.15f);
            wasDown = down;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.92f, 0.4f, 0.28f, 0.9f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * travel);
        }
    }
}
