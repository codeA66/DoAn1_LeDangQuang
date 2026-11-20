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

        void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;
        void OnEnable() => all.Add(this);
        void OnDisable() => all.Remove(this);

        /// <summary>Nguoi choi vua den qua cua nay: bo qua cho toi khi ra khoi trigger.</summary>
        public void Disarm() => armed = false;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!armed || other.isTrigger || !other.CompareTag("Player")) return;
            if (string.IsNullOrEmpty(targetScene) || !WorldLoader.Instance) return;
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
