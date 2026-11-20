using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LuHanh.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LuHanh.EditorTools
{
    /// <summary>
    /// Cong cu the gioi kieu Hollow Knight (moi phong 1 scene):
    /// - Tao phong moi: chep scene mau cua vung (Assets/Scenes/World/_RoomTemplate_[Vung].unity) sang
    ///   Assets/Scenes/World/[Vung]/[Ma].unity, them vao Build Profile, mo ra.
    /// - Kiem tra cua: mo tung phong, bao cua tro toi scene / cua khong co, hoac cua ben kia khong tro nguoc lai.
    /// </summary>
    public class WorldRoomTools : EditorWindow
    {
        const string TemplatePrefix = "_RoomTemplate_";

        string roomId = "A02";
        int zoneIndex;

        [MenuItem("LuHanh/World/Tao phong moi")]
        static void Open() => GetWindow<WorldRoomTools>(true, "Tao phong moi", true).minSize = new Vector2(320f, 120f);

        static string[] Zones() => Directory.GetFiles(WorldLoader.WorldFolder, TemplatePrefix + "*.unity")
            .Select(p => Path.GetFileNameWithoutExtension(p).Substring(TemplatePrefix.Length)).ToArray();

        void OnGUI()
        {
            var zones = Zones();
            if (zones.Length == 0)
            {
                EditorGUILayout.HelpBox($"Chua co scene mau {WorldLoader.WorldFolder}{TemplatePrefix}[Vung].unity", MessageType.Warning);
                return;
            }
            zoneIndex = EditorGUILayout.Popup("Vung", Mathf.Clamp(zoneIndex, 0, zones.Length - 1), zones);
            roomId = EditorGUILayout.TextField("Ma phong", roomId).Trim();
            if (GUILayout.Button("Tao")) Create(zones[zoneIndex], roomId);
        }

        static void Create(string zone, string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            string folder = WorldLoader.WorldFolder + zone;
            string path = $"{folder}/{id}.unity";
            if (File.Exists(path)) { EditorUtility.DisplayDialog("Tao phong", $"Da co {path}", "OK"); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(WorldLoader.WorldFolder.TrimEnd('/'), zone);
            AssetDatabase.CopyAsset($"{WorldLoader.WorldFolder}{TemplatePrefix}{zone}.unity", path);
            AddToBuild(path);
            EditorSceneManager.OpenScene(path);
            Debug.Log($"Da tao phong {path} va them vao Build Profile.");
        }

        /// <summary>Them scene vao Build Profile (neu chua co). Core dung truoc cac phong.</summary>
        public static void AddToBuild(string path)
        {
            var list = EditorBuildSettings.scenes.ToList();
            if (list.Any(s => s.path == path)) return;
            list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        struct DoorInfo
        {
            public string scene, id, target, targetDoor;
        }

        [MenuItem("LuHanh/World/Kiem tra cua")]
        static void Validate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var rooms = EditorBuildSettings.scenes
                .Where(s => s.path.StartsWith(WorldLoader.WorldFolder) && s.path != WorldEditorBoot.CorePath)
                .Select(s => s.path).ToList();
            var doors = new List<DoorInfo>();
            var noRoom = new List<string>();
            try
            {
                foreach (var path in rooms)
                {
                    var scene = SceneManager.GetSceneByPath(path);
                    bool opened = !scene.isLoaded;
                    if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    var roots = scene.GetRootGameObjects();
                    if (!roots.Any(r => r.GetComponentInChildren<WorldRoom>(true))) noRoom.Add(scene.name);
                    foreach (var d in roots.SelectMany(r => r.GetComponentsInChildren<RoomDoor>(true)))
                        doors.Add(new DoorInfo { scene = scene.name, id = d.DoorId, target = d.TargetScene, targetDoor = d.TargetDoor });
                    if (opened) EditorSceneManager.CloseScene(scene, true);
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }

            var names = new HashSet<string>(rooms.Select(Path.GetFileNameWithoutExtension));
            var report = new StringBuilder();
            foreach (var n in noRoom) report.AppendLine($"Phong {n}: thieu WorldRoom.");
            foreach (var d in doors)
            {
                string at = $"{d.scene}/{d.id}";
                if (string.IsNullOrEmpty(d.target)) { report.AppendLine($"{at}: chua noi (targetScene trong)."); continue; }
                if (!names.Contains(d.target)) { report.AppendLine($"{at}: scene '{d.target}' khong co trong Build Profile."); continue; }
                var back = doors.Where(o => o.scene == d.target && o.id == d.targetDoor).ToList();
                if (back.Count == 0) report.AppendLine($"{at}: phong {d.target} khong co cua '{d.targetDoor}'.");
                else if (back[0].target != d.scene || back[0].targetDoor != d.id)
                    report.AppendLine($"{at}: cua {d.target}/{d.targetDoor} khong tro nguoc lai (tro toi {back[0].target}/{back[0].targetDoor}).");
            }
            if (report.Length == 0) Debug.Log($"Kiem tra cua: {rooms.Count} phong, {doors.Count} cua, khong co loi.");
            else Debug.LogWarning($"Kiem tra cua: {rooms.Count} phong, {doors.Count} cua.\n{report}");
        }
    }
}
