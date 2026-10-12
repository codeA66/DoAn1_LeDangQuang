using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LuHanh.EditorTools
{
    /// <summary>
    /// To mau tung o Tilemap_Ground theo do sau kieu Hollow Knight: mep tren giap khong khi sang ro
    /// (anh sang tu tren roi xuong), mep ben vua, cang vao sau khoi dat cang toi toi gan den.
    /// Mau luu trong Tilemap (SetColor, bo TileFlags.LockColor). Sua dia hinh xong thi bam lai menu.
    /// </summary>
    public static class TilemapDepthShadeTool
    {
        const string GroundName = "Tilemap_Ground";
        const int MaxDepth = 4;
        // Do sang theo khoang cach toi khong khi: 0 = mep (tren / ben), 1, 2, 3+.
        const float TopEdge = 0.9f;
        const float SideEdge = 0.5f;
        static readonly float[] Inner = { 0f, 0.3f, 0.15f, 0.07f };
        static readonly Color Tint = new Color(0.86f, 0.84f, 1f);

        [MenuItem("LuHanh/Level/To mau do sau tilemap")]
        static void Shade()
        {
            Tilemap ground = null;
            foreach (var tm in Object.FindObjectsByType<Tilemap>())
                if (tm.name == GroundName && tm.gameObject.activeInHierarchy) ground = tm;
            if (!ground)
            {
                EditorUtility.DisplayDialog("To mau do sau", "Khong tim thay " + GroundName + " dang bat trong scene.", "OK");
                return;
            }

            Undo.RegisterCompleteObjectUndo(ground, "To mau do sau tilemap");
            ground.CompressBounds();
            ground.color = Color.white;

            // BFS tu cac o co dat giap khong khi (4 huong) vao trong.
            var depth = new Dictionary<Vector3Int, int>();
            var queue = new Queue<Vector3Int>();
            var dirs = new[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
            foreach (var c in ground.cellBounds.allPositionsWithin)
            {
                if (!ground.HasTile(c)) continue;
                foreach (var d in dirs)
                    if (!ground.HasTile(c + d)) { depth[c] = 0; queue.Enqueue(c); break; }
            }
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                int next = depth[c] + 1;
                if (next >= MaxDepth) continue;
                foreach (var d in dirs)
                {
                    var n = c + d;
                    if (!ground.HasTile(n) || depth.ContainsKey(n)) continue;
                    depth[n] = next;
                    queue.Enqueue(n);
                }
            }

            int count = 0;
            foreach (var c in ground.cellBounds.allPositionsWithin)
            {
                if (!ground.HasTile(c)) continue;
                int d = depth.TryGetValue(c, out var v) ? v : MaxDepth;
                float k = d == 0 ? (!ground.HasTile(c + Vector3Int.up) ? TopEdge : SideEdge) : Inner[Mathf.Min(d, Inner.Length - 1)];
                ground.SetTileFlags(c, TileFlags.None);
                ground.SetColor(c, new Color(Tint.r * k, Tint.g * k, Tint.b * k, 1f));
                count++;
            }
            EditorUtility.SetDirty(ground);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ground.gameObject.scene);
            Debug.Log($"To mau do sau: {count} o.");
        }
    }
}
