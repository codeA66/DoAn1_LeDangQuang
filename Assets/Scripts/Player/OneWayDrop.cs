using System.Collections;
using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Dang dung tren platform 1 chieu (PlatformEffector2D, One Way) ma nhan xuong (S) thi roi xuyen qua:
    /// tam bo va cham giua nhan vat va platform do trong <see cref="dropTime"/> giay.
    /// Luot / roi tu tren xuong van dap len binh thuong vi chi nhan xuong moi bo va cham.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class OneWayDrop : MonoBehaviour
    {
        [Tooltip("Bo va cham voi platform bao lau (giay) - du de than nhan vat roi het qua.")]
        [SerializeField] float dropTime = 0.3f;
        [Tooltip("Day xuong mot chut de bat dau roi ngay, khong cho trong luc.")]
        [SerializeField] float dropSpeed = 3f;

        PlayerController controller;
        Collider2D selfCollider;
        Rigidbody2D body;
        int lastMoveY;
        Collider2D dropping;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            selfCollider = GetComponent<Collider2D>();
            body = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            int moveY = controller.MoveY;
            // Chi tinh luc vua nhan xuong, giu S khong lam roi lien tuc qua nhieu tang.
            bool pressedDown = moveY == -1 && lastMoveY != -1;
            lastMoveY = moveY;
            if (!pressedDown || !controller.enabled || !controller.IsGrounded) return;

            var platform = controller.GroundCollider;
            if (!platform || !platform.usedByEffector) return;
            if (!platform.TryGetComponent<PlatformEffector2D>(out var effector) || !effector.useOneWay) return;
            StartCoroutine(Drop(platform));
        }

        IEnumerator Drop(Collider2D platform)
        {
            Restore();
            dropping = platform;
            Physics2D.IgnoreCollision(selfCollider, platform, true);
            var v = body.linearVelocity;
            v.y = -dropSpeed;
            body.linearVelocity = v;
            yield return new WaitForSeconds(dropTime);
            Restore();
        }

        void Restore()
        {
            if (dropping) Physics2D.IgnoreCollision(selfCollider, dropping, false);
            dropping = null;
        }

        // Bi tat giua chung (vd bi hut vao cong) cung phai tra lai va cham.
        void OnDisable()
        {
            StopAllCoroutines();
            Restore();
        }
    }
}
