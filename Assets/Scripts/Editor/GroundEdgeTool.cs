using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LuHanh.EditorTools
{
    /// <summary>
    /// Tu dat vien mep kieu Hollow Knight: moi o Tilemap_Ground ma phia tren trong se duoc
    /// phu mot vien sang (go thep + reu tim) tren Tilemap_GroundEdge. Bien the chon theo vi tri
    /// nen chay lai nhieu lan van ra cung ket qua. Sua dia hinh xong thi bam lai menu.
    /// </summary>
    public static class GroundEdgeTool
    {
        const string TileFolder = "Assets/Art/Tiles/Edge";
        const string GroundName = "Tilemap_Ground";
        const string EdgeName = "Tilemap_GroundEdge";

        [MenuItem("LuHanh/Level/Tao vien mep dat")]
        static void Build()
        {
            var ground = FindTilemap(GroundName);
            if (!ground)
            {
                EditorUtility.DisplayDialog("Vien mep dat", "Khong tim thay " + GroundName + " dang bat trong scene.", "OK");
                return;
            }

            var variants = LoadVariants();
            if (variants.Length == 0)
            {
                EditorUtility.DisplayDialog("Vien mep dat", "Khong co tile nao trong " + TileFolder + ".", "OK");
                return;
            }

            var edge = FindOrCreateEdge(ground);
            Undo.RegisterCompleteObjectUndo(edge, "Tao vien mep dat");
            edge.ClearAllTiles();

            ground.CompressBounds();
            var bounds = ground.cellBounds;
            int count = 0;
            foreach (var cell in bounds.allPositionsWithin)
            {
                if (!ground.HasTile(cell) || ground.HasTile(cell + Vector3Int.up)) continue;
                int hash = (cell.x * 73856093) ^ (cell.y * 19349663);
                edge.SetTile(cell, variants[Mathf.Abs(hash) % variants.Length]);
                count++;
            }
            EditorUtility.SetDirty(edge);
            Debug.Log($"[GroundEdgeTool] Da dat {count} o vien mep.", edge);
        }

        static Tilemap FindTilemap(string name)
        {
            foreach (var tm in Object.FindObjectsByType<Tilemap>())
                if (tm.name == name && tm.gameObject.activeInHierarchy) return tm;
            return null;
        }

        static TileBase[] LoadVariants()
        {
            var guids = AssetDatabase.FindAssets("t:TileBase", new[] { TileFolder });
            var tiles = new TileBase[guids.Length];
            for (int i = 0; i < guids.Length; i++)
                tiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>(AssetDatabase.GUIDToAssetPath(guids[i]));
            return tiles;
        }

        static Tilemap FindOrCreateEdge(Tilemap ground)
        {
            var existing = ground.transform.parent ? ground.transform.parent.Find(EdgeName) : null;
            if (existing) return existing.GetComponent<Tilemap>();

            var go = new GameObject(EdgeName, typeof(Tilemap), typeof(TilemapRenderer));
            Undo.RegisterCreatedObjectUndo(go, "Tao vien mep dat");
            go.transform.SetParent(ground.transform.parent, false);
            var renderer = go.GetComponent<TilemapRenderer>();
            // Ngay tren dat (-5), duoi platform 1 chieu (-4).
            renderer.sortingLayerID = ground.GetComponent<TilemapRenderer>().sortingLayerID;
            renderer.sortingOrder = ground.GetComponent<TilemapRenderer>().sortingOrder + 1;
            return go.GetComponent<Tilemap>();
        }
    }
}
