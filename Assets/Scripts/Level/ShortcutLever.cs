using LuHanh.Combat;
using LuHanh.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Level
{
    /// <summary>
    /// Can gat duong tat kieu Hollow Knight: chem (J) vao thi bat sang, mo cac Gate duoc gan.
    /// Dat o phia ben kia duong tat nen chi mo duoc tu mot phia.
    /// Neu gan <see cref="requiredEncounter"/> thi phai don xong phong do moi chem duoc (can tat den, chem khong an).
    /// Da mo thi giu mo khi nguoi choi chet (phan thuong); tai lai man thi ve dong (khong luu).
    /// Collider trigger o layer bi chem (Enemy) de PlayerAttack tim thay qua IDamageable.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShortcutLever : MonoBehaviour, IDamageable
    {
        [SerializeField] Gate[] gates;
        [Tooltip("Phong khoa phai don xong moi chem duoc can. De trong = luc nao cung chem duoc.")]
        [SerializeField] RoomEncounter requiredEncounter;
        [Tooltip("Tay gat (con), xoay tu goc nay sang goc kia khi bat.")]
        [SerializeField] Transform handle;
        [SerializeField] float offAngle = 35f;
        [SerializeField] float onAngle = -35f;
        [SerializeField] float flipTime = 0.12f;
        [Tooltip("Den bao: xanh ngoc khi chem duoc, tat khi chua duoc / da dung.")]
        [SerializeField] Light2D readyLight;
        [SerializeField] float hitStop = 0.08f;
        [SerializeField] float shake = 0.25f;

        bool used;
        float flipTimer = -1f;

        public bool IsUsed => used;
        bool Ready => !used && (!requiredEncounter || requiredEncounter.IsCleared);

        void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (handle) handle.localRotation = Quaternion.Euler(0f, 0f, offAngle);
        }

        public void TakeHit(HitInfo hit)
        {
            if (!Ready) return;
            used = true;
            flipTimer = 0f;
            if (hitStop > 0f) HitStop.Request(hitStop);
            CameraShake.Kick(shake, 0.3f);
            foreach (var g in gates) if (g) g.Open();
        }

        void Update()
        {
            if (readyLight) readyLight.enabled = Ready;
            if (flipTimer < 0f || !handle) return;
            flipTimer += Time.deltaTime;
            float k = Mathf.Clamp01(flipTimer / flipTime);
            handle.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(offAngle, onAngle, 1f - (1f - k) * (1f - k)));
            if (k >= 1f) flipTimer = -1f;
        }
    }
}
