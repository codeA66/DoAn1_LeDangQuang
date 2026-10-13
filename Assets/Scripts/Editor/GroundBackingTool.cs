using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LuHanh.EditorTools
{
    /// <summary>
    /// Man dat (Level3): sau khi ve / xoa o tren Tilemap_Ground, bam menu de lam lai
    /// - Tilemap_Backing: nen toi dung ngay sau moi o dat (T_Backing_Top o hang tren cung, con lai T_Backing_Full);
    /// - Tilemap_DarkFill: phu den loi dat, chua 1 o vien quanh moi mat giap khong khi (kieu Hollow Knight).
    /// Collider tu tao lai khi luu scene (TilemapColliderOnSave).
    /// </summary>
    public static class GroundBackingTool
    {
        const string BackingFull = "Assets/Art/Tiles/BuriedDepths/T_Backing_Full.asset";
        const string BackingTop = "Assets/Art/Tiles/BuriedDepths/T_Backing_Top.asset";

        [MenuItem("LuHanh/Level/Lam lai nen + phu toi dat")]
        static void Build()
        {
            var ground = Find("Tilemap_Ground");
            var backing = Find("Tilemap_Backing");
            var dark = Find("Tilemap_DarkFill");
            if (!ground || !backing || !dark)
            {
                EditorUtility.DisplayDialog("Nen + phu toi", "Can du Tilemap_Ground, Tilemap_Backing, Tilemap_DarkFill trong scene (xem Level3).", "OK");
                return;
            }
            var full = AssetDatabase.LoadAssetAtPath<TileBase>(BackingFull);
            var top = AssetDatabase.LoadAssetAtPath<TileBase>(BackingTop);

            Undo.RegisterCompleteObjectUndo(new Object[] { backing, dark }, "Lam lai nen + phu toi dat");
            ground.CompressBounds();
            var bounds = ground.cellBounds;
            backing.ClearAllTiles();
            dark.ClearAllTiles();

            var backPos = new List<Vector3Int>();
            var backTiles = new List<TileBase>();
            var darkPos = new List<Vector3Int>();
            foreach (var p in bounds.allPositionsWithin)
            {
                if (!ground.HasTile(p)) continue;
                var up = p + Vector3Int.up;
                backPos.Add(p);
                backTiles.Add(bounds.Contains(up) && !ground.HasTile(up) ? top : full);
                if (!TouchesAir(ground, bounds, p)) darkPos.Add(p);
            }
            backing.SetTiles(backPos.ToArray(), backTiles.ToArray());
            var darkTiles = new TileBase[darkPos.Count];
            for (int i = 0; i < darkTiles.Length; i++) darkTiles[i] = full;
            dark.SetTiles(darkPos.ToArray(), darkTiles);

            EditorSceneManager.MarkSceneDirty(ground.gameObject.scene);
            Debug.Log($"Nen: {backPos.Count} o, phu toi: {darkPos.Count} o.");
        }

        // Co o trong (khong khi) trong 8 o xung quanh.
        static bool TouchesAir(Tilemap ground, BoundsInt bounds, Vector3Int p)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    var q = p + new Vector3Int(dx, dy, 0);
                    if (bounds.Contains(q) && !ground.HasTile(q)) return true;
                }
            return false;
        }

        static Tilemap Find(string name)
        {
            foreach (var tm in Object.FindObjectsByType<Tilemap>())
                if (tm.name == name && tm.gameObject.activeInHierarchy) return tm;
            return null;
        }
    }
}
