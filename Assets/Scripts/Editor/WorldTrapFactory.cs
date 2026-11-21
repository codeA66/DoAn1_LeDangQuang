using System;
using System.Collections.Generic;
using LuHanh.Combat;
using LuHanh.Level;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.EditorTools
{
    /// <summary>
    /// Dung bay Vung A (cua ray, laser, bua dap, hoi nong, san sap) tu art trong Assets/Art/Generated/Hazards/Foundry.
    /// - <see cref="ImportArt"/>: dat PPU 32, Point, cat sprite sheet theo khung (menu LuHanh/World/Nhap art bay).
    /// - <see cref="Create"/>: WorldRoomBuilder goi cho moi cho chua bay "tr" trong file bo cuc.
    /// Phan gay sat thuong dung HazardZone (mat mau + ve cho dung an toan, Pogo nay duoc).
    /// </summary>
    public static class WorldTrapFactory
    {
        const string Root = "Assets/Art/Generated/Hazards/Foundry/";
        // Tren lop toi VisionMask (Order 40): bay luon nhin ro (AGENTS: gai/bay phai de phan biet).
        const int TrapOrder = 42, BeamOrder = 45;
        static readonly Color Danger = new(0.918f, 0.396f, 0.278f);   // #EA6547

        // file -> (so khung, pivot, full rect de Tiled)
        static readonly (string file, int frames, Vector2 pivot, bool fullRect)[] Sheets =
        {
            ("Saw/Saw_Blade.png", 1, new Vector2(0.5f, 0.5f), false),
            ("Saw/Saw_Rail.png", 1, new Vector2(0.5f, 0.5f), true),
            ("Saw/Saw_RailEnd.png", 1, new Vector2(0.5f, 0.5f), false),
            ("Laser/Laser_Emitter.png", 2, new Vector2(0.5f, 0.5f), false),
            ("Laser/Laser_Beam.png", 4, new Vector2(0f, 0.5f), true),
            ("Laser/Laser_Warning.png", 1, new Vector2(0f, 0.5f), true),
            ("Crusher/Crusher_Head.png", 2, new Vector2(0.5f, 1f), false),
            ("Crusher/Crusher_Shaft.png", 1, new Vector2(0.5f, 0.5f), true),
            ("Steam/Steam_Vent.png", 2, new Vector2(0.5f, 0f), false),
            ("Steam/Steam_Jet.png", 7, new Vector2(0.5f, 0f), false),
            ("Steam/Steam_Puff.png", 4, new Vector2(0.5f, 0f), false),
            ("Crumble/Crumble_Platform.png", 3, new Vector2(0.5f, 0.5f), false),
            ("Crumble/Crumble_Break.png", 4, new Vector2(0.5f, 0.5f), false),
        };

        [MenuItem("LuHanh/World/Nhap art bay")]
        public static void ImportArt()
        {
            foreach (var s in Sheets)
            {
                string path = Root + s.file;
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) { Debug.LogWarning("Thieu art bay: " + path); continue; }
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                int w = tex.width / s.frames, h = tex.height;
                ti.textureType = TextureImporterType.Sprite;
                ti.spritePixelsPerUnit = 32;
                ti.filterMode = FilterMode.Point;
                ti.mipmapEnabled = false;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.wrapMode = s.fullRect ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                var settings = new TextureImporterSettings();
                ti.ReadTextureSettings(settings);
                settings.spriteMeshType = s.fullRect ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = s.pivot;
                ti.SetTextureSettings(settings);
                if (s.frames > 1)
                {
                    ti.spriteImportMode = SpriteImportMode.Multiple;
                    string baseName = System.IO.Path.GetFileNameWithoutExtension(path);
                    var metas = new SpriteMetaData[s.frames];
                    for (int i = 0; i < s.frames; i++)
                        metas[i] = new SpriteMetaData
                        {
                            name = $"{baseName}_{i}", rect = new Rect(i * w, 0, w, h),
                            alignment = (int)SpriteAlignment.Custom, pivot = s.pivot
                        };
#pragma warning disable CS0618
                    ti.spritesheet = metas;
#pragma warning restore CS0618
                }
                else ti.spriteImportMode = SpriteImportMode.Single;
                ti.SaveAndReimport();
            }
            Debug.Log("[WorldTrapFactory] Da nhap art bay.");
        }

        static Sprite[] Frames(string file)
        {
            var list = new List<Sprite>();
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(Root + file)) if (o is Sprite sp) list.Add(sp);
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list.ToArray();
        }

        static SpriteRenderer Renderer(Transform parent, string name, Sprite sprite, int order, Vector3 local)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        static void Hazard(GameObject go)
        {
            foreach (var c in go.GetComponents<Collider2D>()) c.isTrigger = true;
            go.AddComponent<HazardZone>();
        }

        static void Set(UnityEngine.Object o, Action<SerializedObject> edit)
        {
            var so = new SerializedObject(o);
            edit(so);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetArray(SerializedProperty p, UnityEngine.Object[] items)
        {
            p.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        /// <summary>
        /// Dung mot bay trong cho chua (o x, y, rong w, cao h - toa do so do).
        /// isSolid(x, y): o dat; wx(x) = canh trai o x; wtop(y) = canh tren o y. index dung lech pha.
        /// Tra ve null neu loai bay chua co.
        /// </summary>
        public static GameObject Create(string type, Transform parent, int x, int y, int w, int h,
            Func<int, int, bool> isSolid, Func<float, float> wx, Func<float, float> wtop, int index, float speed = 0f)
        {
            float phase = (index * 0.37f) % 1f;
            switch (type)
            {
                case "Saw": return Saw(parent, x, y, w, h, wx, wtop, phase, speed);
                case "Crumble": return Crumble(parent, x, y, w, h, wx, wtop);
                case "Laser": return Laser(parent, x, y, w, h, wx, wtop, phase);
                case "Crusher": return Crusher(parent, x, y, w, h, isSolid, wx, wtop, phase);
                case "Steam": return Steam(parent, x, y, w, h, wx, wtop, phase);
                default: return null;
            }
        }

        static void DangerLight(Transform parent, float radius)
        {
            var go = new GameObject("DangerLight");
            go.transform.SetParent(parent, false);
            var l = go.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.color = Danger;
            l.intensity = 0.6f;
            l.pointLightOuterRadius = radius;
            l.pointLightInnerRadius = radius * 0.25f;
            // Light2D tao bang script co m_LocalBounds = 0 -> bi cull (TUTORIAL 9.6).
            var so = new SerializedObject(l);
            var b = so.FindProperty("m_LocalBounds");
            if (b != null) { b.boundsValue = new Bounds(Vector3.zero, new Vector3(radius * 2f, radius * 2f, 0f)); so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        static GameObject Saw(Transform parent, int x, int y, int w, int h, Func<float, float> wx, Func<float, float> wtop, float phase, float speed)
        {
            bool vertical = h > w;
            float len = vertical ? h : w;
            var root = new GameObject($"Trap_Saw_{(vertical ? "V" : "H")}{len}");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(wx(x) + w * 0.5f, wtop(y) - h * 0.5f, 0f);
            if (vertical) root.transform.rotation = Quaternion.Euler(0f, 0f, 90f);   // ray theo truc x cuc bo
            w = Mathf.RoundToInt(len);
            var rail = Renderer(root.transform, "Rail", Frames("Saw/Saw_Rail.png")[0], TrapOrder - 1, Vector3.zero);
            rail.drawMode = SpriteDrawMode.Tiled;
            rail.size = new Vector2(w, 0.25f);
            var endSprite = Frames("Saw/Saw_RailEnd.png")[0];
            Renderer(root.transform, "RailEndL", endSprite, TrapOrder, new Vector3(-w * 0.5f, 0f));
            Renderer(root.transform, "RailEndR", endSprite, TrapOrder, new Vector3(w * 0.5f, 0f));
            var blade = Renderer(root.transform, "Blade", Frames("Saw/Saw_Blade.png")[0], TrapOrder + 1, Vector3.zero);
            var col = blade.gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.85f;
            Hazard(blade.gameObject);
            DangerLight(blade.transform, 2.2f);
            float half = Mathf.Max(0f, w * 0.5f - 1f);
            Set(root.AddComponent<SawTrap>(), so =>
            {
                if (speed > 0f) so.FindProperty("speed").floatValue = speed;
                so.FindProperty("blade").objectReferenceValue = blade.transform;
                so.FindProperty("pointA").vector2Value = new Vector2(-half, 0f);
                so.FindProperty("pointB").vector2Value = new Vector2(half, 0f);
                so.FindProperty("phase").floatValue = phase;
            });
            return root;
        }

        static GameObject Laser(Transform parent, int x, int y, int w, int h, Func<float, float> wx, Func<float, float> wtop, float phase)
        {
            bool vertical = h > w;
            float len = vertical ? h : w;
            var root = new GameObject($"Trap_Laser_{(vertical ? "V" : "H")}{len}");
            root.transform.SetParent(parent, false);
            if (vertical)
            {
                root.transform.position = new Vector3(wx(x) + w * 0.5f, wtop(y) - 0.5f, 0f);
                root.transform.rotation = Quaternion.Euler(0f, 0f, -90f);   // truc x cuc bo chieu xuong
            }
            else root.transform.position = new Vector3(wx(x) + 0.5f, wtop(y) - h * 0.5f, 0f);

            var emitterFrames = Frames("Laser/Laser_Emitter.png");
            var emitter = Renderer(root.transform, "Emitter", emitterFrames.Length > 0 ? emitterFrames[0] : null, TrapOrder + 1, Vector3.zero);
            float beamLen = len - 1f;
            var beamFrames = Frames("Laser/Laser_Beam.png");
            var beam = Renderer(root.transform, "Beam", beamFrames.Length > 0 ? beamFrames[0] : null, BeamOrder, new Vector3(0.5f, 0f));
            beam.drawMode = SpriteDrawMode.Tiled;
            beam.size = new Vector2(beamLen, 0.375f);
            var warn = Renderer(root.transform, "Warning", Frames("Laser/Laser_Warning.png")[0], BeamOrder, new Vector3(0.5f, 0f));
            warn.drawMode = SpriteDrawMode.Tiled;
            warn.size = new Vector2(beamLen, 0.125f);
            var hitGo = new GameObject("Hit");
            hitGo.transform.SetParent(root.transform, false);
            var box = hitGo.AddComponent<BoxCollider2D>();
            box.size = new Vector2(beamLen, 0.3f);
            box.offset = new Vector2(0.5f + beamLen * 0.5f, 0f);
            Hazard(hitGo);
            Set(root.AddComponent<LaserTrap>(), so =>
            {
                so.FindProperty("emitter").objectReferenceValue = emitter;
                SetArray(so.FindProperty("emitterFrames"), emitterFrames);
                so.FindProperty("beam").objectReferenceValue = beam;
                SetArray(so.FindProperty("beamFrames"), beamFrames);
                so.FindProperty("warning").objectReferenceValue = warn;
                so.FindProperty("hit").objectReferenceValue = box;
                so.FindProperty("offset").floatValue = phase * 3.2f;
            });
            return root;
        }

        static GameObject Crusher(Transform parent, int x, int y, int w, int h, Func<int, int, bool> isSolid,
            Func<float, float> wx, Func<float, float> wtop, float phase)
        {
            int cx = x + w / 2;
            int ceil = y - 1; while (ceil > -4 && !isSolid(cx, ceil)) ceil--;      // tran phia tren
            int floor = y + h; while (floor < y + h + 30 && !isSolid(cx, floor)) floor++;   // san phia duoi
            var root = new GameObject($"Trap_Crusher_{h}");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(wx(x) + w * 0.5f, wtop(ceil + 1), 0f);
            float startDrop = y - (ceil + 1);                 // dau bua bat dau o mep tren cho chua
            float travel = Mathf.Max(1f, floor - y - 2);      // dau bua cao 2 o, dap cham mat san

            var shaftSprite = Frames("Crusher/Crusher_Shaft.png")[0];
            var shaft = Renderer(root.transform, "Shaft", shaftSprite, TrapOrder - 1, Vector3.zero);
            shaft.drawMode = SpriteDrawMode.Tiled;
            shaft.size = new Vector2(1f, Mathf.Max(0.01f, startDrop));
            var head = new GameObject("Head");
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, -startDrop, 0f);
            var headFrames = Frames("Crusher/Crusher_Head.png");
            var headSr = Renderer(head.transform, "Visual", headFrames.Length > 0 ? headFrames[0] : null, TrapOrder, Vector3.zero);
            var lightAnchor = new GameObject("LightAnchor").transform;
            lightAnchor.SetParent(head.transform, false);
            lightAnchor.localPosition = new Vector3(0f, -1.6f, 0f);
            DangerLight(lightAnchor, 2.4f);
            var hitGo = new GameObject("Hit");
            hitGo.transform.SetParent(head.transform, false);
            var box = hitGo.AddComponent<BoxCollider2D>();
            box.size = new Vector2(2.7f, 0.6f);
            box.offset = new Vector2(0f, -1.7f);
            Hazard(hitGo);
            Set(root.AddComponent<CrusherTrap>(), so =>
            {
                so.FindProperty("head").objectReferenceValue = head.transform;
                so.FindProperty("headRenderer").objectReferenceValue = headSr;
                SetArray(so.FindProperty("headFrames"), headFrames);
                so.FindProperty("shaft").objectReferenceValue = shaft;
                so.FindProperty("hit").objectReferenceValue = box;
                so.FindProperty("travel").floatValue = travel;
                so.FindProperty("offset").floatValue = phase * 3f;
            });
            return root;
        }

        static GameObject Crumble(Transform parent, int x, int y, int w, int h, Func<float, float> wx, Func<float, float> wtop)
        {
            // Moi san sap rong 3 o, mat tren = canh tren hang y.
            var group = new GameObject($"Trap_Crumble_{w}");
            group.transform.SetParent(parent, false);
            group.transform.position = new Vector3(wx(x) + w * 0.5f, wtop(y), 0f);
            var states = Frames("Crumble/Crumble_Platform.png");
            var breaks = Frames("Crumble/Crumble_Break.png");
            int n = Mathf.Max(1, w / 3);
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject("Crumble") { layer = LayerMask.NameToLayer("Ground") };
                go.transform.SetParent(group.transform, false);
                go.transform.position = new Vector3(wx(x) + 1.5f + i * 3f, wtop(y) - 0.5f, 0f);
                var box = go.AddComponent<BoxCollider2D>();
                box.size = new Vector2(3f, 0.7f);
                box.offset = new Vector2(0f, 0.15f);
                var sr = Renderer(go.transform, "Visual", states.Length > 0 ? states[0] : null, TrapOrder, Vector3.zero);
                Set(go.AddComponent<CrumblePlatform>(), so =>
                {
                    so.FindProperty("visual").objectReferenceValue = sr;
                    SetArray(so.FindProperty("stateFrames"), states);
                    SetArray(so.FindProperty("breakFrames"), breaks);
                });
            }
            return group;
        }

        static GameObject Steam(Transform parent, int x, int y, int w, int h, Func<float, float> wx, Func<float, float> wtop, float phase)
        {
            var root = new GameObject("Trap_Steam");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(wx(x) + w * 0.5f, wtop(y + h), 0f);
            var ventFrames = Frames("Steam/Steam_Vent.png");
            var vent = Renderer(root.transform, "Vent", ventFrames.Length > 0 ? ventFrames[0] : null, TrapOrder, Vector3.zero);
            var jetFrames = Frames("Steam/Steam_Jet.png");
            var jet = Renderer(root.transform, "Jet", jetFrames.Length > 0 ? jetFrames[0] : null, BeamOrder, new Vector3(0f, 0.3f));
            var puffFrames = Frames("Steam/Steam_Puff.png");
            var puff = Renderer(root.transform, "Puff", puffFrames.Length > 0 ? puffFrames[0] : null, BeamOrder, new Vector3(0f, 0.3f));
            var hitGo = new GameObject("Hit");
            hitGo.transform.SetParent(root.transform, false);
            var box = hitGo.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.8f, 2.8f);
            box.offset = new Vector2(0f, 1.7f);
            Hazard(hitGo);
            var start = new Sprite[Mathf.Min(3, jetFrames.Length)];
            Array.Copy(jetFrames, start, start.Length);
            var loop = new Sprite[Mathf.Max(0, jetFrames.Length - start.Length)];
            Array.Copy(jetFrames, start.Length, loop, 0, loop.Length);
            Set(root.AddComponent<SteamTrap>(), so =>
            {
                so.FindProperty("vent").objectReferenceValue = vent;
                SetArray(so.FindProperty("ventFrames"), ventFrames);
                so.FindProperty("puff").objectReferenceValue = puff;
                SetArray(so.FindProperty("puffFrames"), puffFrames);
                so.FindProperty("jet").objectReferenceValue = jet;
                SetArray(so.FindProperty("jetStartFrames"), start);
                SetArray(so.FindProperty("jetLoopFrames"), loop);
                so.FindProperty("hit").objectReferenceValue = box;
                so.FindProperty("offset").floatValue = phase * 3.6f;
            });
            return root;
        }
    }
}
