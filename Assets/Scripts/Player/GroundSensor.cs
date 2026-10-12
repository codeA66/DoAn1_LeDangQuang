using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Do xem nhan vat co dang cham dat khong, va giu luon coyote time.
    ///
    /// Coyote time nam o day chu khong o trang thai nhay: no la mot tinh chat cua
    /// "vua roi con o tren dat", nen thuoc ve cam bien.
    /// </summary>
    public class GroundSensor
    {
        // Theo SE220 (Dead Cells): mat dat phai huong len du doc, va khong tinh luc dang bay len.
        const float MinGroundNormalY = 0.7f;
        const float MaxGroundedRiseSpeed = 0.01f;

        readonly Transform origin;
        readonly PlayerMovementConfig config;
        readonly Collider2D selfCollider;
        readonly Rigidbody2D body;
        readonly Collider2D[] hits = new Collider2D[4];
        readonly ContactFilter2D filter;

        float lastGroundedTime = float.NegativeInfinity;

        public GroundSensor(Transform origin, PlayerMovementConfig config)
        {
            this.origin = origin;
            this.config = config;
            selfCollider = origin.GetComponent<Collider2D>();
            body = origin.GetComponent<Rigidbody2D>();

            filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = config.groundLayers,
                useTriggers = false
            };
        }

        /// <summary>Dang thuc su cham dat ngay frame nay.</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>Con trong cua so coyote - van cho phep nhay.</summary>
        public bool CanCoyoteJump => Time.time - lastGroundedTime <= config.coyoteTime;

        public Vector2 ProbeCentre => (Vector2)origin.position + config.groundProbeOffset;

        /// <summary>Goi mot lan moi buoc vat ly, truoc khi cac trang thai chay.</summary>
        public void FixedTick()
        {
            int count = Physics2D.OverlapCircle(ProbeCentre, config.groundProbeRadius, filter, hits);
            GroundCollider = null;
            for (int i = 0; i < count; i++)
            {
                if (!IsStandable(hits[i])) continue;
                GroundCollider = hits[i];
                break;
            }
            IsGrounded = GroundCollider != null;
            if (IsGrounded) lastGroundedTime = Time.time;
        }

        /// <summary>Collider dang dung len (null neu khong cham dat).</summary>
        public Collider2D GroundCollider { get; private set; }

        /// <summary>
        /// Platform 1 chieu chi tinh la dat khi dang dung tren mat no: khong tinh luc nhay xuyen len
        /// (dang bay len hoac chan con duoi mat platform) va luc dang roi qua (va cham dang bi bo qua).
        /// </summary>
        bool IsStandable(Collider2D c)
        {
            if (selfCollider && Physics2D.GetIgnoreCollision(selfCollider, c)) return false;
            // Dang bay len thi khong tinh la cham dat (ca dat thuong lan platform 1 chieu).
            if (body && body.linearVelocity.y > MaxGroundedRiseSpeed) return false;
            if (!FacesUp(c)) return false;
            if (!c.usedByEffector || !c.TryGetComponent<PlatformEffector2D>(out var effector) || !effector.useOneWay) return true;
            return ProbeCentre.y >= c.bounds.max.y - 0.1f;
        }

        /// <summary>
        /// Duoi vong do phai co mat huong len (khong phai chi cham mat tuong / canh tile khi ca goc).
        /// Ban 3 tia xuong (giua, hai mep vong do): dung sat tuong thi tia van trung san, dung mep vuc van tinh.
        /// Khong dung ClosestPoint: Tilemap gop thanh 1 CompositeCollider, sat tuong thi diem gan nhat la mat tuong.
        /// </summary>
        bool FacesUp(Collider2D c)
        {
            Vector2 centre = ProbeCentre;
            float r = config.groundProbeRadius;
            for (int i = -1; i <= 1; i++)
            {
                var from = centre + new Vector2(i * r * 0.9f, r);
                var hit = Physics2D.Raycast(from, Vector2.down, r * 2f + 0.05f, config.groundLayers);
                if (hit.collider == c && (hit.distance <= 0f || hit.normal.y > MinGroundNormalY)) return true;
            }
            return false;
        }

        /// <summary>Huy cua so coyote. Goi ngay sau khi nhay de mot cu bam khong bat ra hai lan.</summary>
        public void ConsumeCoyote() => lastGroundedTime = float.NegativeInfinity;

        public void DrawGizmos()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(ProbeCentre, config.groundProbeRadius);
        }
    }
}
