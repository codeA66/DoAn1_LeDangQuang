using LuHanh.Combat;
using LuHanh.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Level
{
    /// <summary>
    /// Can dieu khien thang may: chem (hoac ban cau) vao thi tay gat lat va thang chay.
    /// Toggle = can tren be (doi chieu); CallTop / CallBottom = can goi dat o moi dau.
    /// Collider trigger o layer bi chem (Enemy) de PlayerAttack / OrbProjectile tim thay qua IDamageable.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ElevatorSwitch : MonoBehaviour, IDamageable
    {
        public enum Mode { Toggle, CallTop, CallBottom }

        [SerializeField] Elevator elevator;
        [SerializeField] Mode mode;
        [Tooltip("Tay gat (con), lat qua lai moi lan chem.")]
        [SerializeField] Transform handle;
        [SerializeField] float angle = 35f;
        [SerializeField] float flipTime = 0.12f;
        [Tooltip("Den bao: sang khi thang dang dung (gat duoc).")]
        [SerializeField] Light2D readyLight;
        [SerializeField] float hitStop = 0.05f;

        float flipTimer = -1f, fromAngle, toAngle;
        bool flipped;

        void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (handle) handle.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void TakeHit(HitInfo hit)
        {
            if (!elevator || elevator.IsMoving) return;
            if (hitStop > 0f) HitStop.Request(hitStop);
            flipped = !flipped;
            fromAngle = flipped ? angle : -angle;
            toAngle = -fromAngle;
            flipTimer = 0f;
            switch (mode)
            {
                case Mode.Toggle: elevator.Toggle(); break;
                case Mode.CallTop: elevator.CallTo(true); break;
                case Mode.CallBottom: elevator.CallTo(false); break;
            }
        }

        void Update()
        {
            if (readyLight) readyLight.enabled = elevator && !elevator.IsMoving;
            if (flipTimer < 0f || !handle) return;
            flipTimer += Time.deltaTime;
            float k = Mathf.Clamp01(flipTimer / flipTime);
            handle.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(fromAngle, toAngle, 1f - (1f - k) * (1f - k)));
            if (k >= 1f) flipTimer = -1f;
        }
    }
}
