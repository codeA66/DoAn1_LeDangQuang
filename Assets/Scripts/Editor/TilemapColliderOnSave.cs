using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace LuHanh.EditorTools
{
    /// <summary>
    /// Truoc khi luu scene: tao lai collider cua moi Tilemap co TilemapCollider2D + CompositeCollider2D.
    /// Unity chi cap nhat collider tilemap o vong update ke tiep cua editor; sua tile bang script
    /// (hoac luc Unity mat focus) roi luu ngay thi collider cu bi luu theo -> tuong tang hinh / roi xuyen dat.
    /// Menu "LuHanh/Level/Tao lai collider tilemap" de chay tay.
    /// </summary>
    [InitializeOnLoad]
    static class TilemapColliderOnSave
    {
        static TilemapColliderOnSave()
        {
            EditorSceneManager.sceneSaving += (scene, _) => Rebuild(scene);
        }

        [MenuItem("LuHanh/Level/Tao lai collider tilemap")]
        static void RebuildActive()
        {
            int n = Rebuild(SceneManager.GetActiveScene());
            Debug.Log($"Tao lai collider cho {n} tilemap.");
        }

        static int Rebuild(Scene scene)
        {
            if (EditorApplication.isPlaying || !scene.isLoaded) return 0;
            int count = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var tc in root.GetComponentsInChildren<TilemapCollider2D>(true))
                {
                    var composite = tc.GetComponent<CompositeCollider2D>();
                    if (!composite || !tc.enabled) continue;
                    tc.ProcessTilemapChanges();
                    composite.GenerateGeometry();
                    count++;
                }
            }
            return count;
        }
    }
}
