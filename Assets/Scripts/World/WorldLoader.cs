using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuHanh.Core;
using LuHanh.Level;
using LuHanh.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LuHanh.World
{
    /// <summary>
    /// The gioi kieu Hollow Knight: scene Core (nguoi choi, HUD, camera, GameSystems) song suot, moi phong la
    /// mot scene tai them (Additive). Qua cua (<see cref="RoomDoor"/>): tu di tiep + toi nhanh -> go phong cu,
    /// tai phong moi -> dat nguoi choi o cua ben kia -> sang lai, tu di them mot nhip roi tra dieu khien.
    /// Chet: tai phong co ghe da ngoi cuoi (SaveData.checkpointScene) va dat len ghe (RespawnManager cho xong).
    /// Phong dang tai la Active Scene, nen vat sinh ra khi choi (dan, hieu ung) bi go cung phong.
    /// Dat trong Core (CoreEssentials/GameSystems). Level1-3 cu khong co WorldLoader nen chay nhu truoc.
    /// </summary>
    public class WorldLoader : MonoBehaviour
    {
        /// <summary>Khoa SessionState: Editor ghi ten phong dang mo khi bam Play (WorldEditorBoot).</summary>
        public const string StartRoomKey = "LuHanh.World.StartRoom";
        /// <summary>Thu muc chua scene the gioi (Core + cac phong).</summary>
        public const string WorldFolder = "Assets/Scenes/World/";

        public static WorldLoader Instance { get; private set; }
        public static bool Active => Instance;
        /// <summary>Dang doi phong / dang tai phong hoi sinh.</summary>
        public static bool IsBusy => Instance && Instance.busy;

        /// <summary>Vua vao mot phong (da dat nguoi choi, camera da vao phong, man hinh con toi).</summary>
        public static event Action<WorldRoom> RoomEntered;

        [Tooltip("Phong bat dau game moi (chua ngoi ghe nao).")]
        [SerializeField] string firstRoom = "A01";
        [Tooltip("Kieu chu cua the ten vung (dung lai LevelList cua the ten man).")]
        [SerializeField] LevelList titleStyle;
        [Tooltip("Dong nho phia tren ten vung. De trong = chi hien ten vung.")]
        [SerializeField] string areaHeader = "";

        [Header("Chuyen phong")]
        [SerializeField, Min(0f)] float fadeOutTime = 0.2f;
        [SerializeField, Min(0f)] float fadeInTime = 0.25f;
        [Tooltip("Di len qua cua tren: van toc doc toi thieu o phong moi de khong roi lai cua cu.")]
        [SerializeField] float upwardBoost = 12f;

        Transform player;
        Rigidbody2D body;
        PlayerController controller;
        PlayerHealth health;
        RoomTransitionFx fx;
        RoomManager rooms;
        Scene current;
        bool busy;

        /// <summary>Ten scene phong dang tai (null khi chua tai).</summary>
        public string CurrentRoom => current.IsValid() ? current.name : null;

        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        IEnumerator Start()
        {
            busy = true;
            var p = GameObject.FindWithTag("Player");
            if (p)
            {
                player = p.transform;
                body = p.GetComponent<Rigidbody2D>();
                controller = p.GetComponent<PlayerController>();
                health = p.GetComponent<PlayerHealth>();
            }
            fx = FindAnyObjectByType<RoomTransitionFx>();
            rooms = FindAnyObjectByType<RoomManager>();
            if (!player) { Debug.LogError("WorldLoader: Core khong co Player.", this); yield break; }

            // Chua co phong: giu nguoi choi dung yen, man hinh den.
            body.simulated = false;
            if (fx && fx.HasFade) yield return fx.FadeOut(0f);

            string room = PickStartRoom(out bool atBench);
            yield return SwapTo(room);
            var worldRoom = WorldRoom.Current;
            if (!worldRoom)
            {
                Debug.LogError($"WorldLoader: scene '{room}' khong co WorldRoom.", this);
                yield break;
            }
            Vector2 pos = atBench ? SaveGame.Data.checkpointPosition : worldRoom.DefaultSpawn;
            controller.Respawn(pos);
            if (health) { health.SetCheckpoint(pos); health.ResetSafePosition(pos); }
            body.simulated = true;
            EnterRoom(worldRoom);

            yield return null;
            yield return null;
            if (fx && fx.HasFade) yield return fx.FadeIn(fadeInTime);
            busy = false;
        }

        /// <summary>Phong bat dau: phong dang mo trong Editor -> ghe da ngoi cuoi -> phong dau game.</summary>
        string PickStartRoom(out bool atBench)
        {
            atBench = false;
#if UNITY_EDITOR
            string editorRoom = UnityEditor.SessionState.GetString(StartRoomKey, "");
            UnityEditor.SessionState.EraseString(StartRoomKey);
            if (IsWorldRoom(editorRoom)) return editorRoom;
#endif
            string bench = SaveGame.Data.checkpointScene;
            if (IsWorldRoom(bench)) { atBench = true; return bench; }
            return firstRoom;
        }

        // ---- Qua cua ----

        public void GoThrough(RoomDoor door)
        {
            if (busy || !door || !player) return;
            if (!IsWorldRoom(door.TargetScene))
            {
                Debug.LogWarning($"Cua {door.gameObject.scene.name}/{door.DoorId}: scene '{door.TargetScene}' khong co trong Build Profile (thu muc World).", door);
                return;
            }
            StartCoroutine(Travel(door.TargetScene, door.TargetDoor, door.DoorSide));
        }

        IEnumerator Travel(string sceneName, string doorId, RoomDoor.Side exitSide)
        {
            busy = true;
            bool horizontal = exitSide == RoomDoor.Side.Left || exitSide == RoomDoor.Side.Right;
            try
            {
                if (fx && fx.HasFade) yield return fx.FadeOut(fadeOutTime);

                Vector2 velocity = body.linearVelocity;
                body.simulated = false;
                yield return SwapTo(sceneName);

                var room = WorldRoom.Current;
                var door = room ? room.FindDoor(doorId) : null;
                if (!door) Debug.LogWarning($"WorldLoader: phong '{sceneName}' khong co cua '{doorId}', dat o diem xuat hien mac dinh.");
                Vector2 pos = door ? door.SpawnPoint : room ? room.DefaultSpawn : (Vector2)player.position;
                if (door) door.Disarm();

                body.position = pos;
                player.position = pos;
                body.simulated = true;
                if (horizontal) body.linearVelocity = new Vector2(velocity.x, 0f);
                else if (exitSide == RoomDoor.Side.Top) body.linearVelocity = new Vector2(velocity.x, Mathf.Max(velocity.y, upwardBoost));
                else body.linearVelocity = velocity;
                if (health) health.ResetSafePosition(pos);
                if (room) EnterRoom(room);

                // Cho Cinemachine dat camera vao phong moi khi man hinh con den.
                yield return null;
                yield return null;
                if (fx && fx.HasFade) yield return fx.FadeIn(fadeInTime);
            }
            finally
            {
                if (controller) controller.ReleaseForcedMove();
                if (body) body.simulated = true;
                busy = false;
            }
        }

        // ---- Tram di chuyen nhanh ----

        /// <summary>
        /// Di chuyen nhanh toi tram cua phong <paramref name="sceneName"/>: toi man hinh, tai phong, dat nguoi choi o
        /// <see cref="FastTravelStation"/> cua phong do, sang lai. Goi tu bang chon tram (game dang dung thi van chay).
        /// </summary>
        public void TravelToStation(string sceneName)
        {
            if (busy || !player || !IsWorldRoom(sceneName)) return;
            StartCoroutine(StationTravel(sceneName));
        }

        IEnumerator StationTravel(string sceneName)
        {
            busy = true;
            try
            {
                if (fx && fx.HasFade) yield return fx.FadeOut(fadeOutTime * 2f);
                body.simulated = false;
                yield return SwapTo(sceneName);
                var room = WorldRoom.Current;
                var station = FindAnyObjectByType<FastTravelStation>();
                Vector2 pos = station && station.gameObject.scene == current ? station.ArrivalPoint
                    : room ? room.DefaultSpawn : (Vector2)player.position;
                controller.Respawn(pos);
                if (health) health.ResetSafePosition(pos);
                body.simulated = true;
                if (room) EnterRoom(room);
                yield return null;
                yield return null;
                if (fx && fx.HasFade) yield return fx.FadeIn(fadeInTime * 2f);
            }
            finally
            {
                if (body) body.simulated = true;
                busy = false;
            }
        }

        // ---- Hoi sinh ----

        /// <summary>
        /// Chet: tai phong co ghe da ngoi cuoi va dat nguoi choi len ghe (chua ngoi ghe nao thi ve phong dau game).
        /// Goi luc man hinh da den (RespawnManager); RespawnManager doi <see cref="IsBusy"/> roi moi sang lai.
        /// Khong bat lai vat ly: PlayerHealth tu bat khi hoi sinh xong.
        /// </summary>
        public void RespawnAtBench()
        {
            busy = true;
            StartCoroutine(RespawnRoutine());
        }

        IEnumerator RespawnRoutine()
        {
            try
            {
                var d = SaveGame.Data;
                bool bench = IsWorldRoom(d.checkpointScene);
                body.simulated = false;
                // Kieu Hollow Knight: luon tai lai phong (ke ca cung phong) de quai ve ban dau.
                yield return SwapTo(bench ? d.checkpointScene : firstRoom);
                var room = WorldRoom.Current;
                Vector2 pos = bench ? d.checkpointPosition : room ? room.DefaultSpawn : (Vector2)player.position;
                controller.Respawn(pos);
                if (health) { health.SetCheckpoint(pos); health.ResetSafePosition(pos); }
                if (room) EnterRoom(room);
            }
            finally
            {
                busy = false;
            }
        }

        // ---- Tai / go scene ----

        IEnumerator SwapTo(string sceneName)
        {
            if (current.IsValid() && current.isLoaded)
            {
                var unload = SceneManager.UnloadSceneAsync(current);
                while (unload != null && !unload.isDone) yield return null;
            }
            current = default;

            var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (load == null)
            {
                Debug.LogError($"WorldLoader: khong tai duoc scene '{sceneName}' (chua co trong Build Profile?).", this);
                yield break;
            }
            while (!load.isDone) yield return null;
            current = SceneManager.GetSceneByName(sceneName);
            if (current.IsValid()) SceneManager.SetActiveScene(current);
        }

        void EnterRoom(WorldRoom room)
        {
            // Moi phong deu dat tam o goc toa do: phong cu dang go co the van con trong Room.All,
            // nen chon thang Room cua scene vua vao.
            if (rooms) rooms.Refresh(room.gameObject.scene);
            RebaseParallax(room);
            WorldState.Set(WorldMapData.VisitedFlag + room.RoomId);
            // Vao vung moi lan dau: hien ten vung (luu trong co the gioi).
            string zoneKey = "Zone/" + room.ZoneId;
            if (!WorldState.Has(zoneKey))
            {
                WorldState.Set(zoneKey);
                if (!string.IsNullOrEmpty(room.ZoneTitle)) SceneTransition.ShowAreaTitle(areaHeader, room.ZoneTitle, titleStyle);
            }
            RoomEntered?.Invoke(room);
        }

        /// <summary>Moc parallax cua cac tang background trong phong = camera o tam phong (khong phai cho camera luc tai).</summary>
        void RebaseParallax(WorldRoom room)
        {
            var cam = Camera.main;
            if (!cam) return;
            Vector2 centre = rooms && rooms.Current ? rooms.Current.Bounds.center : (Vector2)room.transform.position;
            var reference = new Vector3(centre.x, centre.y, cam.transform.position.z);
            foreach (var root in room.gameObject.scene.GetRootGameObjects())
                foreach (var layer in root.GetComponentsInChildren<LuHanh.Environment.ParallaxEffect>(true))
                    layer.Rebase(reference);
        }

        // ---- Danh sach phong ----

        static Dictionary<string, string> worldScenes;

        /// <summary>Scene <paramref name="sceneName"/> la phong the gioi trong Build Profile (thu muc World, khong phai Core).</summary>
        public static bool IsWorldRoom(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            if (worldScenes == null)
            {
                worldScenes = new Dictionary<string, string>();
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string path = SceneUtility.GetScenePathByBuildIndex(i);
                    string name = Path.GetFileNameWithoutExtension(path);
                    if (path.StartsWith(WorldFolder) && name != "Core") worldScenes[name] = path;
                }
            }
            return worldScenes.ContainsKey(sceneName);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            worldScenes = null;
            RoomEntered = null;
        }
    }
}
