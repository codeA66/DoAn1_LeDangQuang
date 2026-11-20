using LuHanh.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace LuHanh.EditorTools
{
    /// <summary>
    /// Bam Play khi dang mo mot scene phong the gioi (Assets/Scenes/World/...): Editor mo Core truoc
    /// (playModeStartScene), ghi ten phong vao SessionState de WorldLoader tai dung phong do va dat nguoi choi
    /// o diem xuat hien mac dinh. Scene khac (Level1-3, Test...) Play nhu cu.
    /// Phong duoc tai tu file tren dia, nen co hoi luu scene truoc khi Play.
    /// </summary>
    [InitializeOnLoad]
    static class WorldEditorBoot
    {
        public const string CorePath = WorldLoader.WorldFolder + "Core.unity";

        static WorldEditorBoot()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.activeSceneChangedInEditMode += (_, _) => Refresh();
            EditorSceneManager.sceneOpened += (_, _) => Refresh();
            EditorApplication.delayCall += Refresh;
        }

        public static bool IsRoomScene(Scene scene) =>
            scene.IsValid() && scene.path.StartsWith(WorldLoader.WorldFolder) && scene.path != CorePath;

        static void Refresh()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var active = SceneManager.GetActiveScene();
            var core = IsRoomScene(active) ? AssetDatabase.LoadAssetAtPath<SceneAsset>(CorePath) : null;
            EditorSceneManager.playModeStartScene = core;
            if (core) SessionState.SetString(WorldLoader.StartRoomKey, active.name);
            else SessionState.EraseString(WorldLoader.StartRoomKey);
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                var active = SceneManager.GetActiveScene();
                if (!IsRoomScene(active)) { EditorSceneManager.playModeStartScene = null; return; }
                // Phong tai tu dia: thay doi chua luu se khong co trong Play -> hoi luu (nguoi dung chon).
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorApplication.isPlaying = false;
                    return;
                }
                var core = AssetDatabase.LoadAssetAtPath<SceneAsset>(CorePath);
                EditorSceneManager.playModeStartScene = core;
                SessionState.SetString(WorldLoader.StartRoomKey, active.name);
            }
            else if (change == PlayModeStateChange.EnteredEditMode)
            {
                Refresh();
            }
        }
    }
}
