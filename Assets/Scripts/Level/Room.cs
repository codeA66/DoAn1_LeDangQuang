using System.Collections.Generic;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Mot phong kieu Celeste: vung chu nhat camera bi gioi han ben trong. Tam phong la vi tri object,
    /// kich thuoc la <see cref="size"/> (keo canh trong Scene view khi chon object).
    /// Phong tu dang ky vao danh sach chung khi bat, RoomManager khong phai tim.
    /// Phong nen phu ca ho ben duoi de nguoi choi roi xuong van tinh la trong phong.
    /// </summary>
    public class Room : MonoBehaviour
    {
        static readonly List<Room> all = new();

        /// <summary>Moi phong dang bat trong scene.</summary>
        public static IReadOnlyList<Room> All => all;

        [Tooltip("Kich thuoc phong (unit). Nen >= khung hinh camera (khoang 28.5 x 16 voi camera size 8, 16:9).")]
        [SerializeField] Vector2 size = new Vector2(30f, 17f);

        public Vector2 Size
        {
            get => size;
            set => size = new Vector2(Mathf.Max(1f, value.x), Mathf.Max(1f, value.y));
        }

        public Rect Bounds => new Rect((Vector2)transform.position - size * 0.5f, size);

        public bool Contains(Vector2 point) => Bounds.Contains(point);

        /// <summary>Phong dang bat chua <paramref name="point"/>, khong co thi null.</summary>
        public static Room At(Vector2 point)
        {
            for (int i = 0; i < all.Count; i++)
                if (all[i].Contains(point)) return all[i];
            return null;
        }

        /// <summary>Hai phong cham canh nhau (cho phep ho <paramref name="gap"/> unit).</summary>
        public bool Touches(Room other, float gap = 0.5f)
        {
            var a = Bounds;
            var b = other.Bounds;
            return a.xMin <= b.xMax + gap && b.xMin <= a.xMax + gap
                && a.yMin <= b.yMax + gap && b.yMin <= a.yMax + gap;
        }

        void OnEnable() => all.Add(this);
        void OnDisable() => all.Remove(this);

        void OnDrawGizmos() => DrawBounds(new Color(0.4f, 0.95f, 1f, 0.5f));

        void OnDrawGizmosSelected()
        {
            DrawBounds(new Color(0.4f, 0.95f, 1f, 1f));
            // Bao do neu phong nho hon khung hinh: camera se dung yen o giua phong theo truc do.
            var cam = Camera.main;
            if (!cam || !cam.orthographic) return;
            float viewH = cam.orthographicSize * 2f;
            float viewW = viewH * cam.aspect;
            if (size.x >= viewW && size.y >= viewH) return;
            Gizmos.color = new Color(1f, 0.35f, 0.3f, 0.9f);
            Gizmos.DrawWireCube(transform.position, new Vector3(viewW, viewH, 0f));
        }

        void DrawBounds(Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));
        }
    }
}
