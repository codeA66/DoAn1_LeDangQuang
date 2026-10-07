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

        /// <summary>Frame nay vua tiep dat, frame truoc thi chua.</summary>
        public bool JustLanded { get; private set; }

        /// <summary>Con trong cua so coyote - van cho phep nhay.</summary>
        public bool CanCoyoteJump => Time.time - lastGroundedTime <= config.coyoteTime;

        public Vector2 ProbeCentre => (Vector2)origin.position + config.groundProbeOffset;

        /// <summary>Goi mot lan moi buoc vat ly, truoc khi cac trang thai chay.</summary>
        public void FixedTick()
        {
            bool wasGrounded = IsGrounded;

            int count = Physics2D.OverlapCircle(ProbeCentre, config.groundProbeRadius, filter, hits);
            GroundCollider = null;
            for (int i = 0; i < count; i++)
            {
                if (!IsStandable(hits[i])) continue;
                GroundCollider = hits[i];
                break;
            }
            IsGrounded = GroundCollider != null;

            JustLanded = IsGrounded && !wasGrounded;
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
            if (!c.usedByEffector || !c.TryGetComponent<PlatformEffector2D>(out var effector) || !effector.useOneWay) return true;
            if (body && body.linearVelocity.y > 0.01f) return false;
            return ProbeCentre.y >= c.bounds.max.y - 0.1f;
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
