using System.Collections.Generic;
using LuHanh.Core;
using LuHanh.Level;
using LuHanh.Player;
using LuHanh.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LuHanh.UI
{
    /// <summary>
    /// Ban do the gioi kieu Hollow Knight: dung tren dat, GIU phim Map (M) thi hien ban do, game van chay nhung
    /// nhan vat dung yen (khong di / chem / ban); tha phim, bi danh hoac roi khoi dat thi tat.
    /// Ve tu <see cref="WorldMapData"/>: chi khu co manh ban do (WorldMapData.IsShown); phong da di qua sang hon,
    /// chua di qua mo. Dau: ghe (vang), tram (xanh duong), boss (do), cho nguoi choi (xanh ngoc, dung vi tri trong phong),
    /// tui Shards roi khi chet (xanh nhat). Dat trong Core (CoreEssentials/GameSystems) - Level1-3 khong co.
    /// </summary>
    public class WorldMapUI : MonoBehaviour
    {
        [SerializeField] WorldMapData map;
        [SerializeField] Font font;
        [SerializeField] string actionName = "Map";
        [Tooltip("Khoang cach giua tam hai o phong tren man hinh (px, man hinh chuan 1920x1080).")]
        [SerializeField] Vector2 pitch = new(92f, 58f);
        [Tooltip("Kich thuoc moi o phong (px).")]
        [SerializeField] Vector2 roomSize = new(74f, 42f);
        [SerializeField] float fadeSpeed = 8f;

        [Header("Mau (quy chuan mau AGENTS.md)")]
        [SerializeField] Color backColor = new(0.047f, 0.039f, 0.078f, 0.88f);   // #0C0A14
        [SerializeField] Color visitedColor = new(0.27f, 0.25f, 0.32f, 1f);      // #454052
        [SerializeField] Color unvisitedColor = new(0.16f, 0.15f, 0.20f, 0.75f); // #292734
        [SerializeField] Color linkColor = new(0.27f, 0.25f, 0.32f, 1f);
        [SerializeField] Color benchColor = new(0.91f, 0.67f, 0.35f, 1f);        // #E7AA59
        [SerializeField] Color stationColor = new(0.29f, 0.5f, 0.76f, 1f);       // #4A7FC1
        [SerializeField] Color bossColor = new(0.92f, 0.4f, 0.28f, 1f);          // #EA6547
        [SerializeField] Color playerColor = new(0.27f, 0.86f, 0.77f, 1f);       // #45DCC4
        [SerializeField] Color shardsColor = new(0.75f, 0.85f, 0.95f, 1f);       // #BFD8F2
        [SerializeField] Color textColor = new(0.95f, 0.96f, 0.97f, 1f);
        [SerializeField] Color mutedColor = new(0.55f, 0.55f, 0.62f, 1f);

        class RoomView
        {
            public WorldMapData.Room data;
            public Image box, icon;
        }

        readonly List<RoomView> views = new();
        readonly List<(Image line, string a, string b)> links = new();
        readonly Dictionary<string, Text> zoneTitles = new();
        CanvasGroup group;
        RectTransform content, playerMark, shardsMark;
        Text emptyText;
        InputAction action;
        PlayerController controller;
        MonoBehaviour[] blockWhileOpen;
        RoomManager rooms;
        bool open;

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p)
            {
                controller = p.GetComponent<PlayerController>();
                blockWhileOpen = new MonoBehaviour[]
                {
                    p.GetComponent<PlayerAttack>(), p.GetComponent<PlayerOrbShot>(), p.GetComponent<PlayerFocus>()
                };
                var input = p.GetComponent<PlayerInput>();
                if (input) action = input.actions.FindAction(actionName);
            }
            rooms = FindAnyObjectByType<RoomManager>();
            if (!map) { Debug.LogWarning("WorldMapUI: chua gan WorldMapData.", this); enabled = false; return; }
            Build();
        }

        void OnDisable()
        {
            if (open) SetOpen(false);
        }

        void Update()
        {
            if (!controller || !WorldLoader.Active) return;
            bool want = action != null && action.IsPressed();
            bool can = controller.enabled && !controller.IsHurt && !WorldLoader.IsBusy &&
                       !(RespawnManager.Instance && RespawnManager.Instance.IsRespawning) &&
                       (open || controller.IsGrounded);
            if (want && can && !open) SetOpen(true);
            else if (open && (!want || !can)) SetOpen(false);

            group.alpha = Mathf.MoveTowards(group.alpha, open ? 1f : 0f, Time.unscaledDeltaTime * fadeSpeed);
            if (open) UpdateMarkers();
        }

        void SetOpen(bool value)
        {
            open = value;
            if (value)
            {
                controller.ForceMove(0f);   // dung yen, khong nhay / dash
                foreach (var b in blockWhileOpen) if (b) b.enabled = false;
                Refresh();
            }
            else
            {
                if (controller) controller.ReleaseForcedMove();
                foreach (var b in blockWhileOpen) if (b) b.enabled = true;
            }
        }

        void Refresh()
        {
            var shownIds = new HashSet<string>();
            Vector2 min = new(float.MaxValue, float.MaxValue), max = -min;
            foreach (var v in views)
            {
                bool shown = map.IsShown(v.data);
                v.box.gameObject.SetActive(shown);
                if (!shown) continue;
                shownIds.Add(v.data.id);
                v.box.color = WorldMapData.Visited(v.data.id) ? visitedColor : unvisitedColor;
                var p = v.box.rectTransform.anchoredPosition;
                min = Vector2.Min(min, p); max = Vector2.Max(max, p);
            }
            // Can giua phan ban do dang hien (chua co manh vung sau thi khong bi lech goc).
            content.anchoredPosition = shownIds.Count > 0 ? -(min + max) * 0.5f : Vector2.zero;
            foreach (var (line, a, b) in links) line.gameObject.SetActive(shownIds.Contains(a) && shownIds.Contains(b));
            foreach (var kv in zoneTitles)
            {
                bool any = false;
                foreach (var v in views) if (v.data.zone == kv.Key && shownIds.Contains(v.data.id)) { any = true; break; }
                kv.Value.gameObject.SetActive(any);
            }

            string here = WorldLoader.Instance ? WorldLoader.Instance.CurrentRoom : null;
            var hereRoom = here != null ? map.GetRoom(here) : null;
            bool hereShown = hereRoom != null && shownIds.Contains(hereRoom.id);
            emptyText.gameObject.SetActive(!hereShown);
            if (!hereShown)
            {
                var zone = hereRoom != null ? map.GetZone(hereRoom.zone) : null;
                emptyText.text = "You have no map of " + (zone != null ? zone.title : "this area");
            }
        }

        void UpdateMarkers()
        {
            string here = WorldLoader.Instance ? WorldLoader.Instance.CurrentRoom : null;
            var view = views.Find(v => v.data.id == here);
            bool showPlayer = view != null && view.box.gameObject.activeSelf;
            playerMark.gameObject.SetActive(showPlayer);
            if (showPlayer)
            {
                // Dung vi tri trong phong: chuan hoa theo khung Room hien tai.
                Vector2 k = new(0.5f, 0.5f);
                if (rooms && rooms.Current)
                {
                    var b = rooms.Current.Bounds;
                    Vector2 pos = controller.transform.position;
                    k = new Vector2(Mathf.InverseLerp(b.xMin, b.xMax, pos.x), Mathf.InverseLerp(b.yMin, b.yMax, pos.y));
                }
                var rt = view.box.rectTransform;
                playerMark.anchoredPosition = rt.anchoredPosition + new Vector2((k.x - 0.5f) * roomSize.x, (k.y - 0.5f) * roomSize.y);
            }

            var d = SaveGame.Data;
            var cacheView = d.lostShards > 0 ? views.Find(v => v.data.id == d.lostScene) : null;
            bool showCache = cacheView != null && cacheView.box.gameObject.activeSelf;
            shardsMark.gameObject.SetActive(showCache);
            if (showCache) shardsMark.anchoredPosition = cacheView.box.rectTransform.anchoredPosition + new Vector2(roomSize.x * 0.3f, 0f);
        }

        // ---------- Dung giao dien ----------

        void Build()
        {
            var root = new GameObject("WorldMap", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            // Tren HUD va the ten vung / chuyen man (SceneTransition) de khong bi de.
            UIBuild.SetupOverlay(root, 1100);
            group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            var back = MakeImage("Back", root.transform, backColor, Vector2.zero);
            back.rectTransform.anchorMin = Vector2.zero; back.rectTransform.anchorMax = Vector2.one;

            content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(root.transform, false);

            // Can giua toan bo luoi.
            Vector2 min = new(float.MaxValue, float.MaxValue), max = -min;
            foreach (var r in map.rooms) { var c = (Vector2)map.GlobalCell(r); min = Vector2.Min(min, c); max = Vector2.Max(max, c); }
            Vector2 centre = (min + max) * 0.5f;
            Vector2 ToScreen(Vector2Int cell) => new((cell.x - centre.x) * pitch.x, -(cell.y - centre.y) * pitch.y);

            // Duong noi (ve truoc de nam duoi o phong).
            var done = new HashSet<string>();
            foreach (var r in map.rooms)
                foreach (var other in r.links)
                {
                    var o = map.GetRoom(other);
                    if (o == null) continue;
                    string key = string.CompareOrdinal(r.id, o.id) < 0 ? r.id + "|" + o.id : o.id + "|" + r.id;
                    if (!done.Add(key)) continue;
                    Vector2 a = ToScreen(map.GlobalCell(r)), b = ToScreen(map.GlobalCell(o));
                    var line = MakeImage("Link_" + key, content, linkColor, Vector2.zero);
                    var lrt = line.rectTransform;
                    lrt.anchoredPosition = (a + b) * 0.5f;
                    bool vertical = Mathf.Abs(a.x - b.x) < 0.5f;
                    lrt.sizeDelta = vertical ? new Vector2(6f, Mathf.Abs(a.y - b.y)) : new Vector2(Mathf.Abs(a.x - b.x), 6f);
                    links.Add((line, r.id, o.id));
                }

            foreach (var r in map.rooms)
            {
                var box = MakeImage("Room_" + r.id, content, visitedColor, roomSize);
                box.rectTransform.anchoredPosition = ToScreen(map.GlobalCell(r));
                Image icon = null;
                Color? ic = r.kind switch
                {
                    WorldMapData.RoomKind.Bench => benchColor,
                    WorldMapData.RoomKind.Station => stationColor,
                    WorldMapData.RoomKind.Boss => bossColor,
                    _ => null
                };
                if (ic.HasValue)
                {
                    icon = MakeImage("Icon", box.transform, ic.Value, new Vector2(12f, 12f));
                    icon.rectTransform.anchoredPosition = new Vector2(-roomSize.x * 0.3f, 0f);
                }
                views.Add(new RoomView { data = r, box = box, icon = icon });
            }

            foreach (var z in map.zones)
            {
                Vector2 zmin = new(float.MaxValue, float.MaxValue);
                Vector2 zmax = -zmin;
                foreach (var r in map.rooms)
                    if (r.zone == z.id) { var p = ToScreen(map.GlobalCell(r)); zmin = Vector2.Min(zmin, p); zmax = Vector2.Max(zmax, p); }
                var t = UIBuild.MakeText("Zone_" + z.id, content, font, 34, mutedColor);
                t.supportRichText = true;
                t.text = UIBuild.SmallCaps(z.title, 42);
                t.alignment = TextAnchor.MiddleLeft;
                t.rectTransform.sizeDelta = new Vector2(600f, 50f);
                t.rectTransform.pivot = new Vector2(0f, 0.5f);
                t.rectTransform.anchoredPosition = new Vector2(zmin.x - roomSize.x * 0.5f, zmax.y + 40f);
                zoneTitles[z.id] = t;
            }

            shardsMark = MakeImage("LostShards", content, shardsColor, new Vector2(12f, 12f)).rectTransform;
            shardsMark.localRotation = Quaternion.Euler(0f, 0f, 45f);
            playerMark = MakeImage("Player", content, playerColor, new Vector2(14f, 14f)).rectTransform;

            emptyText = UIBuild.MakeText("Empty", root.transform, font, 40, textColor);
            emptyText.rectTransform.anchoredPosition = new Vector2(0f, 0f);

            var hint = UIBuild.MakeText("Hint", root.transform, font, 26, mutedColor);
            hint.text = "Hold M  -  Map";
            hint.rectTransform.anchoredPosition = new Vector2(0f, -470f);
        }

        static Image MakeImage(string name, Transform parent, Color color, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            img.rectTransform.sizeDelta = size;
            return img;
        }
    }
}
