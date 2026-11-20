using System.Collections.Generic;
using UnityEngine;

namespace LuHanh.World
{
    /// <summary>
    /// Danh dau mot scene la mot phong cua the gioi kieu Hollow Knight (moi phong 1 scene, tai them vao Core).
    /// Ma phong = ten scene (A01, B12...). Moi scene phong co dung mot WorldRoom.
    /// Bam Play ngay trong scene phong thi Editor tu mo Core truoc (WorldEditorBoot) roi tai phong nay,
    /// nguoi choi xuat hien o <see cref="defaultSpawn"/>.
    /// </summary>
    public class WorldRoom : MonoBehaviour
    {
        [Tooltip("Ma vung (Foundry, Mossy). Doi vung lan dau thi hien the ten vung.")]
        [SerializeField] string zoneId = "Foundry";
        [Tooltip("Ten vung hien tren man hinh (tieng Anh).")]
        [SerializeField] string zoneTitle = "Forgotten Foundry";
        [Tooltip("Cho xuat hien khi vao phong khong qua cua (bam Play trong scene, phong dau game).")]
        [SerializeField] Transform defaultSpawn;

        static readonly List<WorldRoom> loaded = new();

        /// <summary>Phong dang tai (chi co mot luc choi binh thuong).</summary>
        public static WorldRoom Current => loaded.Count > 0 ? loaded[loaded.Count - 1] : null;

        public string RoomId => gameObject.scene.name;
        public string ZoneId => zoneId;
        public string ZoneTitle => zoneTitle;
        public Vector2 DefaultSpawn => defaultSpawn ? (Vector2)defaultSpawn.position : (Vector2)transform.position;

        void OnEnable() => loaded.Add(this);
        void OnDisable() => loaded.Remove(this);

        /// <summary>Cua co ma <paramref name="doorId"/> trong phong nay, khong co thi null.</summary>
        public RoomDoor FindDoor(string doorId)
        {
            foreach (var d in RoomDoor.All)
                if (d.gameObject.scene == gameObject.scene && d.DoorId == doorId) return d;
            return null;
        }

        void OnDrawGizmos()
        {
            if (!defaultSpawn) return;
            Gizmos.color = new Color(0.27f, 0.86f, 0.77f, 0.9f);
            Gizmos.DrawWireSphere(defaultSpawn.position + Vector3.up * 0.9f, 0.5f);
        }
    }
}
