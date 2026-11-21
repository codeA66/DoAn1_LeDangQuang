using System;
using System.Collections.Generic;
using System.IO;
using LuHanh.Level;
using LuHanh.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace LuHanh.EditorTools
{
    /// <summary>
    /// Dung phong the gioi tu file bo cuc JSON (Assets/Settings/WorldLayouts/[Vung]/[Ma].json) - chinh la so do
    /// da duyet: o (x, y) tinh tu goc tren-trai cua phong, 1 o = 1 unit, phong dat tam o goc toa do.
    /// Tao scene tu scene mau neu chua co; neu da co thi XOA SACH dat / cua / quai / do vat cu roi dung lai
    /// (khong dung toi Background, den vung, Room, WorldRoom). Luu scene, dong lai neu tu mo.
    /// Menu: LuHanh/World/Dung phong tu bo cuc (chon file .json trong Project).
    /// </summary>
    public static class WorldRoomBuilder
    {
        public const string LayoutFolder = "Assets/Settings/WorldLayouts/";
        const int Margin = 4;

        // ---------- Du lieu bo cuc ----------
        [Serializable] public class R4 { public int x, y, w, h; }
        [Serializable] public class Fake { public int x, y, w, h; public string mode = "Wall"; public string flag; }
        [Serializable] public class Trap { public int x, y, w, h; public string type; }
        [Serializable] public class Mark { public string k; public int x, y; public string flag; }
        [Serializable] public class Door { public string id, side, target, targetDoor; public int x, y, w, h; public float dist = 1.5f; }
        [Serializable] public class GateSpec { public int x, y, w; public string flag; }
        [Serializable]
        public class RoomSpec
        {
            public string id, zone = "Foundry";
            public int w, h, f;
            public R4[] air = new R4[0], sol = new R4[0], ow = new R4[0], pl = new R4[0], sp = new R4[0], spL = new R4[0], spD = new R4[0];
            public Fake[] fk = new Fake[0];
            public Trap[] tr = new Trap[0];
            public Mark[] mk = new Mark[0];
            public Door[] doors = new Door[0];
            public GateSpec[] gates = new GateSpec[0];
        }

        // ---------- Asset ----------
        const string GroundTile = "Assets/Art/Tiles/Level1/RT_Industrial.asset";
        const string FillTile = "Assets/Art/Tiles/Level1/T_L1_IndustrialFill.asset";
        const string TopTile = "Assets/Art/Tiles/Level1/T_L1_IndustrialTop_0.asset";
        const string EdgeFolder = "Assets/Art/Tiles/Edge";
        const string CrackSprite = "Assets/Art/Generated/Secret/SecretCrack.png";
        static readonly Dictionary<string, string> MarkPrefabs = new()
        {
            { "g", "Assets/Prefabs/Enemies/Man1/Ghoul.prefab" },
            { "b", "Assets/Prefabs/Enemies/Man1/Bat.prefab" },
            { "c", "Assets/Prefabs/Enemies/Man1/CinderHusk.prefab" },
            { "B", "Assets/Prefabs/Level/Checkpoint_Bench.prefab" },
            { "S", "Assets/Prefabs/World/FastTravelStation.prefab" },
            { "K", "Assets/Prefabs/Level/SkillPickup_Dash.prefab" },
            { "M", "Assets/Prefabs/Level/MapPiece_Foundry1.prefab" },
            { "W", "Assets/Prefabs/Level/SkillPickup_WallCling.prefab" },
            { "L", "Assets/Prefabs/Level/ShortcutLever.prefab" },
            { "X", "Assets/Prefabs/Level/BreakableDevice.prefab" },
            { "A", "Assets/Prefabs/NPC/OldAnvil.prefab" },
            { "O", "Assets/Prefabs/Level/SkillPickup_OrbShot.prefab" },
            { "N", "Assets/Prefabs/Level/MapPiece_Foundry3.prefab" },
            { "Z", "Assets/Prefabs/Level/CollapsingWall.prefab" },
        };
        // Diem danh dau de trong (cho vat pham lam sau): ten object.
        static readonly Dictionary<string, string> EmptySpots = new()
        {
            { "$", "SecretSpot (de trong)" },
            { "H", "MaskShardSpot (de trong)" },
            { "C", "ShardCacheSpot (de trong)" },
        };
        const string MapAsset = "Assets/Settings/WorldMap.asset";

        [MenuItem("LuHanh/World/Dung phong tu bo cuc")]
        static void BuildSelected()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!path.EndsWith(".json") || !path.StartsWith(LayoutFolder))
            {
                Debug.LogWarning($"Chon file bo cuc .json trong {LayoutFolder} truoc.");
                return;
            }
            Build(path);
        }

        /// <summary>Dung mot phong tu file bo cuc. Tra ve duong dan scene.</summary>
        public static string Build(string layoutPath)
        {
            var spec = JsonUtility.FromJson<RoomSpec>(File.ReadAllText(layoutPath));
            string scenePath = $"{WorldLoader.WorldFolder}{spec.zone}/{spec.id}.unity";
            if (!File.Exists(scenePath))
            {
                AssetDatabase.CopyAsset($"{WorldLoader.WorldFolder}_RoomTemplate_{spec.zone}.unity", scenePath);
                WorldRoomTools.AddToBuild(scenePath);
            }
            var scene = SceneManager.GetSceneByPath(scenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                BuildInto(scene, spec);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
            Debug.Log($"[WorldRoomBuilder] Da dung {scenePath} ({spec.w}x{spec.h}).");
            return scenePath;
        }

        static GameObject Root(Scene s, string name)
        {
            foreach (var r in s.GetRootGameObjects()) if (r.name == name) return r;
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, s);
            return go;
        }

        static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) Object.DestroyImmediate(t.GetChild(i).gameObject);
        }

        static void BuildInto(Scene s, RoomSpec r)
        {
            // Toa do: o (x, y) cua so do -> o tilemap (x - w/2, h/2 - 1 - y); y tren cua o = h/2 - y.
            int ox = -r.w / 2, oy = r.h / 2 - 1;
            Vector3Int Cell(int x, int y) => new(x + ox, oy - y, 0);
            float WX(float x) => x + ox;          // canh trai o x
            float WTop(float y) => oy + 1 - y;    // canh tren o y

            var grid = Root(s, "Grid");
            var ground = grid.transform.Find("Tilemap_Ground").GetComponent<Tilemap>();
            var edge = grid.transform.Find("Tilemap_GroundEdge").GetComponent<Tilemap>();
            ground.ClearAllTiles();
            edge.ClearAllTiles();
            for (int i = grid.transform.childCount - 1; i >= 0; i--)
            {
                var c = grid.transform.GetChild(i);
                if (c.GetComponent<SecretWall>()) Object.DestroyImmediate(c.gameObject);
            }

            // --- Luoi dac / rong (co le Margin o ngoai phong) ---
            int W = r.w + Margin * 2, H = r.h + Margin * 2;
            var solid = new bool[W, H];
            bool In(int x, int y) => x >= -Margin && y >= -Margin && x < r.w + Margin && y < r.h + Margin;
            void Set(int x, int y, bool v) { if (In(x, y)) solid[x + Margin, y + Margin] = v; }
            for (int y = -Margin; y < r.h + Margin; y++)
            for (int x = -Margin; x < r.w + Margin; x++)
                Set(x, y, y < 2 || x < 2 || x >= r.w - 2 || y >= r.f);
            void Fill(R4 a, bool v, bool extend)
            {
                int x0 = a.x, y0 = a.y, x1 = a.x + a.w, y1 = a.y + a.h;
                if (extend)
                {
                    if (x0 <= 0) x0 = -Margin;
                    if (y0 <= 0) y0 = -Margin;
                    if (x1 >= r.w) x1 = r.w + Margin;
                    if (y1 >= r.h) y1 = r.h + Margin;
                }
                for (int y = y0; y < y1; y++) for (int x = x0; x < x1; x++) Set(x, y, v);
            }
            foreach (var a in r.sol) Fill(a, true, false);
            foreach (var a in r.air) Fill(a, false, true);
            // Cua chua noi (phong ben kia chua dung): khong khoet ra ngoai mep phong.
            foreach (var d in r.doors)
                if (string.IsNullOrEmpty(d.target)) Fill(new R4 { x = d.x, y = d.y, w = d.w, h = d.h }, false, false);
            foreach (var d in r.doors)
                if (string.IsNullOrEmpty(d.target))
                {
                    // Bit lai phan le ngoai mep da bi khoet boi air (neu air cham mep).
                    for (int y = -Margin; y < r.h + Margin; y++)
                    for (int x = -Margin; x < r.w + Margin; x++)
                    {
                        bool outside = x < 0 || y < 0 || x >= r.w || y >= r.h;
                        bool inCol = x >= d.x && x < d.x + d.w, inRow = y >= d.y && y < d.y + d.h;
                        if (outside && (inCol || inRow) && Near(d, x, y)) Set(x, y, true);
                    }
                }
            foreach (var fk in r.fk) Fill(new R4 { x = fk.x, y = fk.y, w = fk.w, h = fk.h }, false, false);

            var groundTile = AssetDatabase.LoadAssetAtPath<TileBase>(GroundTile);
            for (int y = -Margin; y < r.h + Margin; y++)
            for (int x = -Margin; x < r.w + Margin; x++)
                if (solid[x + Margin, y + Margin]) ground.SetTile(Cell(x, y), groundTile);

            // --- Vien mep dat (cung logic GroundEdgeTool) ---
            var variants = new List<TileBase>();
            foreach (var g in AssetDatabase.FindAssets("t:TileBase", new[] { EdgeFolder }))
                variants.Add(AssetDatabase.LoadAssetAtPath<TileBase>(AssetDatabase.GUIDToAssetPath(g)));
            ground.CompressBounds();
            foreach (var cell in ground.cellBounds.allPositionsWithin)
            {
                if (!ground.HasTile(cell) || ground.HasTile(cell + Vector3Int.up) || variants.Count == 0) continue;
                int hash = (cell.x * 73856093) ^ (cell.y * 19349663);
                edge.SetTile(cell, variants[Mathf.Abs(hash) % variants.Count]);
            }
            EditorUtility.SetDirty(ground);
            EditorUtility.SetDirty(edge);

            // --- Room + WorldRoom ---
            var room = Root(s, "Room").GetComponent<Room>();
            room.transform.position = Vector3.zero;
            var so = new SerializedObject(room);
            so.FindProperty("size").vector2Value = new Vector2(r.w, r.h);
            so.ApplyModifiedPropertiesWithoutUndo();

            var wr = Root(s, "WorldRoom").GetComponent<WorldRoom>();
            var spawn = wr.transform.Find("Spawn");

            var doorsRoot = Root(s, "Doors").transform;
            var enemies = Root(s, "Enemies").transform;
            var props = Root(s, "Props").transform;
            Clear(doorsRoot); Clear(enemies); Clear(props);

            // --- Cua ---
            var doorPf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/World/RoomDoor.prefab");
            foreach (var d in r.doors)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(doorPf, doorsRoot);
                go.name = "Door_" + d.id;
                var box = go.GetComponent<BoxCollider2D>();
                bool vertical = d.side == "Top" || d.side == "Bottom";
                if (!vertical)
                {
                    go.transform.position = new Vector3(WX(d.x) + d.w * 0.5f, WTop(d.y + d.h), 0f);
                    box.size = new Vector2(1f, d.h);
                    box.offset = new Vector2(0f, d.h * 0.5f);
                }
                else
                {
                    go.transform.position = new Vector3(WX(d.x) + d.w * 0.5f, WTop(d.y) - d.h * 0.5f, 0f);
                    box.size = new Vector2(d.w, 1f);
                    box.offset = Vector2.zero;
                }
                var dso = new SerializedObject(go.GetComponent<RoomDoor>());
                dso.FindProperty("doorId").stringValue = d.id;
                dso.FindProperty("side").enumValueIndex = (int)Enum.Parse(typeof(RoomDoor.Side), d.side);
                dso.FindProperty("targetScene").stringValue = d.target ?? "";
                dso.FindProperty("targetDoor").stringValue = d.targetDoor ?? "";
                dso.FindProperty("spawnDistance").floatValue = d.dist;
                dso.ApplyModifiedPropertiesWithoutUndo();
            }

            // --- Platform mot chieu (mat tren = canh tren o) ---
            var owPf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Platform/OneWayPlatform.prefab");
            foreach (var a in r.ow)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(owPf, props);
                go.name = $"OneWay_{a.w}";
                var col = go.GetComponent<BoxCollider2D>();
                go.transform.position = new Vector3(WX(a.x) + a.w * 0.5f, WTop(a.y) - col.size.y * 0.5f, 0f);
                col.size = new Vector2(a.w, col.size.y);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.size = new Vector2(a.w, sr.size.y);
            }

            // --- Platform dac (prefab) ---
            foreach (var a in r.pl) PlaceSolidPlatforms(props, a, WX, WTop);

            // --- Gai ---
            var spikePf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Objects/SpikePit_JumpTutorial.prefab");
            foreach (var a in r.sp)
            {
                int n = Mathf.Max(1, Mathf.RoundToInt(a.w / 4f));
                float seg = a.w / (float)n;
                for (int i = 0; i < n; i++)
                {
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(spikePf, props);
                    go.name = "Spikes";
                    go.transform.position = new Vector3(WX(a.x) + seg * (i + 0.5f), WTop(a.y + a.h) + 0.375f, 0f);
                    go.transform.localScale = new Vector3(seg / 3.8f, 1f, 1f);
                }
            }
            foreach (var a in r.spL)
            {
                // Gai tren mat tuong, chia ve phia trai (tuong o ben phai cot o).
                int n = Mathf.Max(1, Mathf.RoundToInt(a.h / 4f));
                float seg = a.h / (float)n;
                for (int i = 0; i < n; i++)
                {
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(spikePf, props);
                    go.name = "Spikes_Wall";
                    go.transform.position = new Vector3(WX(a.x + a.w) - 0.375f, WTop(a.y) - seg * (i + 0.5f), 0f);
                    go.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    go.transform.localScale = new Vector3(seg / 3.8f, 1f, 1f);
                }
            }

            foreach (var a in r.spD)
            {
                // Gai treo tran, chia xuong (tran o phia tren hang o).
                int n = Mathf.Max(1, Mathf.RoundToInt(a.w / 4f));
                float seg = a.w / (float)n;
                for (int i = 0; i < n; i++)
                {
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(spikePf, props);
                    go.name = "Spikes_Ceiling";
                    go.transform.position = new Vector3(WX(a.x) + seg * (i + 0.5f), WTop(a.y) - 0.375f, 0f);
                    go.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                    go.transform.localScale = new Vector3(seg / 3.8f, 1f, 1f);
                }
            }

            // --- Tuong / san gia ---
            foreach (var fk in r.fk) BuildSecret(grid.transform, fk, Cell, WX, WTop);

            // --- Cho chua bay (de trong) ---
            foreach (var t in r.tr)
            {
                var go = new GameObject($"TrapSlot_{t.type}_{t.w}x{t.h}");
                go.transform.SetParent(props, false);
                go.transform.position = new Vector3(WX(t.x) + t.w * 0.5f, WTop(t.y) - t.h * 0.5f, 0f);
            }

            // --- Cua sap (Gate xoay ngang nam tren lo) ---
            var gatePf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Level/Gate.prefab");
            var builtGates = new List<Gate>();
            foreach (var g in r.gates)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(gatePf, props);
                go.name = "Gate_Hatch";
                // Goc Gate = mep tren, keo dai xuong; xoay +90 do -> keo dai sang phai.
                go.transform.position = new Vector3(WX(g.x), WTop(g.y) - 0.5f, 0f);
                go.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                var gso = new SerializedObject(go.GetComponent<Gate>());
                gso.FindProperty("height").floatValue = g.w;
                gso.FindProperty("width").floatValue = 1f;
                gso.FindProperty("startClosed").boolValue = true;
                gso.FindProperty("openIfWorldFlag").stringValue = g.flag ?? "";
                gso.ApplyModifiedPropertiesWithoutUndo();
                builtGates.Add(go.GetComponent<Gate>());
            }

            // --- Diem danh dau --- (tuong sap Z dat truoc de cong tac X noi vao)
            var builtWalls = new List<CollapsingWall>();
            var marks = new List<Mark>(r.mk);
            marks.Sort((a, b) => (a.k == "Z" ? 0 : 1).CompareTo(b.k == "Z" ? 0 : 1));
            foreach (var m in marks)
            {
                var foot = new Vector3(WX(m.x) + 0.5f, WTop(m.y + 1), 0f);
                if (m.k == "P") { spawn.position = foot; continue; }
                if (EmptySpots.TryGetValue(m.k, out var spotName))
                {
                    var spot = new GameObject(spotName);
                    spot.transform.SetParent(props, false);
                    spot.transform.position = foot + Vector3.up * 0.5f;
                    continue;
                }
                if (!MarkPrefabs.TryGetValue(m.k, out var pfPath)) { Debug.LogWarning($"Khong ro ky hieu '{m.k}'"); continue; }
                var pf = AssetDatabase.LoadAssetAtPath<GameObject>(pfPath);
                bool enemy = m.k == "g" || m.k == "b" || m.k == "c";
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pf, enemy ? enemies : props);
                bool floating = m.k == "b" || m.k == "K" || m.k == "M" || m.k == "W" || m.k == "O" || m.k == "N";
                inst.transform.position = floating ? foot + Vector3.up * 0.5f : foot;
                if (m.k == "Z")
                {
                    var wso = new SerializedObject(inst.GetComponent<CollapsingWall>());
                    wso.FindProperty("worldFlag").stringValue = m.flag ?? "";
                    wso.ApplyModifiedPropertiesWithoutUndo();
                    builtWalls.Add(inst.GetComponent<CollapsingWall>());
                }
                else if (m.k == "L")
                {
                    var lso = new SerializedObject(inst.GetComponent<ShortcutLever>());
                    lso.FindProperty("worldFlag").stringValue = m.flag ?? "";
                    lso.ApplyModifiedPropertiesWithoutUndo();
                }
                else if (m.k == "X")
                {
                    // Cong tac chi vo bang cau K; mo cac Gate co Open If World Flag = "[phong]/[flag]".
                    var dso = new SerializedObject(inst.GetComponent<BreakableDevice>());
                    dso.FindProperty("worldFlag").stringValue = m.flag ?? "";
                    dso.FindProperty("orbOnly").boolValue = true;
                    // Tuong sap gan nhat trong phong (neu co).
                    CollapsingWall nearest = null;
                    foreach (var w in builtWalls)
                        if (!nearest || Vector2.Distance(w.transform.position, inst.transform.position) < Vector2.Distance(nearest.transform.position, inst.transform.position)) nearest = w;
                    dso.FindProperty("target").objectReferenceValue = nearest;
                    var list = builtGates.FindAll(gt => new SerializedObject(gt).FindProperty("openIfWorldFlag").stringValue == $"{r.id}/{m.flag}");
                    var arr = dso.FindProperty("openGates");
                    arr.arraySize = list.Count;
                    for (int i = 0; i < list.Count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
                    dso.ApplyModifiedPropertiesWithoutUndo();
                }
                else if (m.k == "A")
                {
                    var shop = inst.GetComponentInChildren<LuHanh.UI.UpgradeShopUI>();
                    var sso = new SerializedObject(shop);
                    sso.FindProperty("map").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WorldMapData>(MapAsset);
                    var pieces = sso.FindProperty("mapPiecesForSale");
                    var ids = string.IsNullOrEmpty(m.flag) ? new string[0] : m.flag.Split(',');
                    pieces.arraySize = ids.Length;
                    for (int i = 0; i < ids.Length; i++) pieces.GetArrayElementAtIndex(i).stringValue = ids[i].Trim();
                    sso.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorSceneManager.MarkSceneDirty(s);
        }

        static bool Near(Door d, int x, int y)
        {
            int dx = x < d.x ? d.x - x : (x >= d.x + d.w ? x - (d.x + d.w - 1) : 0);
            int dy = y < d.y ? d.y - y : (y >= d.y + d.h ? y - (d.y + d.h - 1) : 0);
            return dx + dy <= Margin + 1;
        }

        static void PlaceSolidPlatforms(Transform parent, R4 a, Func<float, float> wx, Func<float, float> wtop)
        {
            string path = a.w >= 6 ? "Assets/Prefabs/Platform/Platform_Industrial_Bridge.prefab"
                                   : "Assets/Prefabs/Platform/Platform_Industrial_Worn.prefab";
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var probe = (GameObject)PrefabUtility.InstantiatePrefab(pf, parent);
            var b = probe.GetComponent<Collider2D>().bounds;
            float pw = b.size.x, topOff = b.max.y - probe.transform.position.y;
            Object.DestroyImmediate(probe);
            int n = Mathf.Max(1, Mathf.CeilToInt(a.w / pw - 0.15f));
            float step = n > 1 ? (a.w - pw) / (n - 1) : 0f;
            for (int i = 0; i < n; i++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(pf, parent);
                float cx = n > 1 ? wx(a.x) + pw * 0.5f + step * i : wx(a.x) + a.w * 0.5f;
                go.transform.position = new Vector3(cx, wtop(a.y) - topOff, 0f);
            }
        }

        static void BuildSecret(Transform grid, Fake fk, Func<int, int, Vector3Int> cell, Func<float, float> wx, Func<float, float> wtop)
        {
            bool floor = fk.mode == "Floor";
            var go = new GameObject(floor ? "SecretFloor" : "SecretWall") { layer = LayerMask.NameToLayer("Ground") };
            go.transform.SetParent(grid, false);
            var centre = new Vector3(wx(fk.x) + fk.w * 0.5f, wtop(fk.y) - fk.h * 0.5f, 0f);
            go.transform.position = centre;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(fk.w, fk.h);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var tilesGo = new GameObject("Tiles", typeof(Tilemap), typeof(TilemapRenderer));
            tilesGo.transform.SetParent(visual.transform, false);
            tilesGo.transform.position = Vector3.zero;   // trung luoi voi Grid
            var tm = tilesGo.GetComponent<Tilemap>();
            var groundRenderer = grid.Find("Tilemap_Ground").GetComponent<TilemapRenderer>();
            var tr = tilesGo.GetComponent<TilemapRenderer>();
            tr.sortingLayerID = groundRenderer.sortingLayerID;
            tr.sortingOrder = groundRenderer.sortingOrder;
            tr.sharedMaterial = groundRenderer.sharedMaterial;
            var fill = AssetDatabase.LoadAssetAtPath<TileBase>(FillTile);
            var top = AssetDatabase.LoadAssetAtPath<TileBase>(TopTile);
            for (int y = fk.y; y < fk.y + fk.h; y++)
            for (int x = fk.x; x < fk.x + fk.w; x++)
                tm.SetTile(cell(x, y), floor && y == fk.y ? top : fill);
            // Lop tilemap con dat lech theo goc cha: bu lai de o trung khop luoi.
            tilesGo.transform.localPosition = -centre;

            var crack = AssetDatabase.LoadAssetAtPath<Sprite>(CrackSprite);
            if (crack)
            {
                var cgo = new GameObject("Crack");
                cgo.transform.SetParent(visual.transform, false);
                var sr = cgo.AddComponent<SpriteRenderer>();
                sr.sprite = crack;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(fk.w, fk.h);
                sr.sortingLayerID = groundRenderer.sortingLayerID;
                sr.sortingOrder = groundRenderer.sortingOrder + 2;
            }

            if (!floor)
            {
                var hit = new GameObject("Hitbox") { layer = LayerMask.NameToLayer("Enemy") };
                hit.transform.SetParent(go.transform, false);
                var hb = hit.AddComponent<BoxCollider2D>();
                hb.isTrigger = true;
                hb.size = new Vector2(fk.w + 0.4f, fk.h);
            }

            var sw = go.AddComponent<SecretWall>();
            var so = new SerializedObject(sw);
            so.FindProperty("mode").enumValueIndex = floor ? 1 : 0;
            so.FindProperty("worldFlag").stringValue = string.IsNullOrEmpty(fk.flag) ? $"Secret_{fk.x}_{fk.y}" : fk.flag;
            so.FindProperty("visualRoot").objectReferenceValue = visual;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
