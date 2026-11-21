using System.Collections.Generic;
using UnityEngine;

namespace LuHanh.World
{
    /// <summary>
    /// Cua o mep phong: nguoi choi cham vao thi <see cref="WorldLoader"/> chuyen sang <see cref="targetScene"/>
    /// va dat nguoi choi o cua <see cref="targetDoor"/> ben do.
    /// Goc object dat o chan lo cua (giua mep duoi voi cua trai/phai, giua lo voi cua tren/duoi);
    /// BoxCollider2D trigger phu lo cua. Nguoi choi xuat hien cach cua <see cref="spawnDistance"/> vao trong phong.
    /// Vua den qua cua nay thi cua tam khoa cho toi khi nguoi choi ra khoi trigger (khong bi day nguoc lai).
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomDoor : MonoBehaviour
    {
        public enum Side { Left, Right, Top, Bottom }

        [Tooltip("Ma cua trong phong nay, vd L, R, T, B, L2...")]
        [SerializeField] string doorId = "R";
        [Tooltip("Cua nam o mep nao cua phong.")]
        [SerializeField] Side side = Side.Right;
        [Tooltip("Scene phong ben kia (vd A02). De trong = cua chua noi (khong lam gi).")]
        [SerializeField] string targetScene;
        [Tooltip("Ma cua ben phong kia, noi nguoi choi xuat hien.")]
        [SerializeField] string targetDoor = "L";
        [Tooltip("Xuat hien cach cua bao nhieu unit vao trong phong.")]
        [SerializeField] float spawnDistance = 1.5f;

        static readonly List<RoomDoor> all = new();
        public static IReadOnlyList<RoomDoor> All => all;

        bool armed = true;

        public string DoorId => doorId;
        public Side DoorSide => side;
        public string TargetScene => targetScene;
        public string TargetDoor => targetDoor;

        /// <summary>Huong tu cua vao trong phong.</summary>
        public Vector2 Inward => side switch
        {
            Side.Left => Vector2.right,
            Side.Right => Vector2.left,
            Side.Top => Vector2.down,
            _ => Vector2.up
        };

        /// <summary>Cho dat chan nguoi choi khi vao phong qua cua nay.</summary>
        public Vector2 SpawnPoint => (Vector2)transform.position + Inward * spawnDistance;

        BoxCollider2D box;
        static readonly Collider2D[] overlap = new Collider2D[8];

        void Awake()
        {
            box = GetComponent<BoxCollider2D>();
            box.isTrigger = true;
        }

        // Dang khoa ma nguoi choi khong con trong lo cua (vd xuat hien ngoai trigger) thi mo lai:
        // OnTriggerExit2D khong bao gio den neu nguoi choi chua tung o trong trigger.
        void FixedUpdate()
        {
            if (armed) return;
            var b = box.bounds;
            int n = Physics2D.OverlapBox(b.center, b.size, 0f, ContactFilter2D.noFilter, overlap);
            for (int i = 0; i < n; i++)
                if (overlap[i] && !overlap[i].isTrigger && overlap[i].CompareTag("Player")) return;
            armed = true;
        }
        void OnEnable() => all.Add(this);
        void OnDisable() => all.Remove(this);

        /// <summary>Nguoi choi vua den qua cua nay: bo qua cho toi khi khong con cham trigger.</summary>
        public void Disarm() => armed = false;

        void OnTriggerEnter2D(Collider2D other) => TryEnter(other);

        // Cham cua dung luc WorldLoader con ban (vua toi noi, dang sang lai) thi GoThrough bo qua:
        // thu lai moi buoc vat ly khi nguoi choi con trong lo cua, khong de nguoi choi di xuyen ra ngoai phong.
        void OnTriggerStay2D(Collider2D other) => TryEnter(other);

        void TryEnter(Collider2D other)
        {
            if (!armed || other.isTrigger || !other.CompareTag("Player")) return;
            if (string.IsNullOrEmpty(targetScene) || !WorldLoader.Instance || WorldLoader.IsBusy) return;
            WorldLoader.Instance.GoThrough(this);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.isTrigger && other.CompareTag("Player")) armed = true;
        }

        void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            Gizmos.color = string.IsNullOrEmpty(targetScene) ? new Color(1f, 0.4f, 0.3f, 0.8f) : new Color(0.27f, 0.86f, 0.77f, 0.8f);
            if (box) Gizmos.DrawWireCube(transform.TransformPoint(box.offset), box.size);
            Gizmos.DrawLine(transform.position, SpawnPoint);
            Gizmos.DrawWireSphere(SpawnPoint, 0.2f);
        }
    }
}
