using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using LuHanh.Enemies;
using LuHanh.Effects;

public static class RootwraithHandDrawnBuilder
{
    public const string Root = "Assets/Art/Generated/Enemies/Rootwraith/HandDrawn_v2";
    public const string ScenePath = "Assets/Scenes/Test/Test_Rootwraith_HandDrawn.unity";
    public static readonly string[] Names = { "Idle", "Hurt", "Teleport_Out", "Teleport_In", "Dive_Windup", "Dive", "Volley_Windup", "Volley_Fire", "Pillar_Windup", "Pillar_Slam", "Phase_Shift", "Death" };

    [MenuItem("Tools/Rootwraith/Build Hand Drawn Animation Test")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before building.");
        if (SceneManager.GetSceneByPath(ScenePath).isLoaded) throw new InvalidOperationException("Test scene already open; rebuild requires explicit handling.");
        var player = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        var visual = player.GetComponentsInChildren<SpriteRenderer>(true).First(s => s.name == "Visual");
        // Visible player texture rect is 148 px high at 80.38835 PPU; boss is 216 px.
        float ppu = 216f / (visual.sprite.bounds.size.y * visual.transform.lossyScale.y * 1.5f);
        Directory.CreateDirectory(Root + "/Anim");
        AssetDatabase.Refresh();
        foreach (var file in Directory.GetFiles(Root + "/Frames", "*.png", SearchOption.AllDirectories))
        {
            var path = file.Replace('\\', '/');
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.spritePivot = new Vector2(.5f, 36f / 288f);
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = new Vector2(.5f, 36f / 288f);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None; importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
        var controllerPath = Root + "/Rootwraith_HandDrawn.controller";
        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath)) throw new InvalidOperationException("Controller already exists.");
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var clips = Names.Select(name =>
        {
            var sprites = Directory.GetFiles(Root + "/Frames/" + name, "*.png").OrderBy(p => p).Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\', '/'))).ToArray();
            if (sprites.Length < 2 || sprites.Any(s => !s)) throw new InvalidOperationException("Missing frames: " + name);
            var clip = new AnimationClip { name = name, frameRate = 12 };
            var keys = sprites.Select((s, i) => new ObjectReferenceKeyframe { time = i / 12f, value = s }).ToList();
            keys.Add(new ObjectReferenceKeyframe { time = sprites.Length / 12f, value = sprites.Last() });
            AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, keys.ToArray());
            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip); clipSettings.loopTime = name == "Idle";
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
            AssetDatabase.CreateAsset(clip, Root + "/Anim/" + name + ".anim");
            var state = controller.layers[0].stateMachine.AddState(name); state.motion = clip;
            if (name == "Idle") controller.layers[0].stateMachine.defaultState = state;
            return clip;
        }).ToArray();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        var boss = new GameObject("Rootwraith_HandDrawn");
        var sr = boss.AddComponent<SpriteRenderer>(); sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Frames/Idle/Idle_00.png"); sr.sortingOrder = 20;
        sr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        AssetDatabase.CreateAsset(sr.sharedMaterial, Root + "/Rootwraith_Lit.mat");
        var animator = boss.AddComponent<Animator>(); animator.runtimeAnimatorController = controller;
        var lightObject = new GameObject("SelfLight"); lightObject.transform.SetParent(boss.transform, false); lightObject.transform.localPosition = new Vector3(0, 1.3f, 0);
        var selfLight = lightObject.AddComponent<Light2D>(); selfLight.lightType = Light2D.LightType.Point; selfLight.color = Hex("E8EEF2"); selfLight.intensity = .9f; selfLight.pointLightOuterRadius = 2.5f;
        var outline = new GameObject("Outline"); outline.transform.SetParent(boss.transform, false);
        var os = outline.AddComponent<SpriteRenderer>(); os.sprite = sr.sprite; os.sortingOrder = 19; os.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shaders/M_SpriteOutline.mat"); outline.AddComponent<SpriteOutline>();
        PrefabUtility.SaveAsPrefabAsset(boss, Root + "/Rootwraith_HandDrawn.prefab");
        UnityEngine.Object.DestroyImmediate(boss);

        var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CameraRig.prefab"), scene);
        foreach (var behaviour in rig.GetComponentsInChildren<MonoBehaviour>(true)) behaviour.enabled = false;
        var camera = rig.GetComponentInChildren<Camera>(true);
        foreach (Transform child in rig.transform) if (child.gameObject != camera.gameObject) child.gameObject.SetActive(false);
        foreach (Transform child in camera.transform) child.gameObject.SetActive(false);
        foreach (var listener in rig.GetComponentsInChildren<AudioListener>(true)) listener.enabled = false;
        // Camera projection and size remain inherited from CameraRig.prefab.
        var globalObject = new GameObject("Map2_GlobalLight"); SceneManager.MoveGameObjectToScene(globalObject, scene);
        var global = globalObject.AddComponent<Light2D>(); global.lightType = Light2D.LightType.Global; global.color = Hex("A9C2A8"); global.intensity = .55f;
        var backdrop = new GameObject("Map2_Backdrop"); var backdropRenderer = backdrop.AddComponent<SpriteRenderer>();
        var square = Texture2D.whiteTexture; var backgroundSprite = Sprite.Create(square, new Rect(0, 0, square.width, square.height), Vector2.one * .5f, square.width);
        // Store a local solid texture so the scene has persistent sprite references.
        var tex = new Texture2D(2, 2); tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white }); tex.Apply();
        File.WriteAllBytes(Root + "/TestBackdrop.png", tex.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(Root + "/TestBackdrop.png"); var bi = (TextureImporter)AssetImporter.GetAtPath(Root + "/TestBackdrop.png"); bi.textureType = TextureImporterType.Sprite; bi.spritePixelsPerUnit = 2; bi.SaveAndReimport();
        backdropRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/TestBackdrop.png"); backdropRenderer.color = Hex("121A14"); backdropRenderer.sortingOrder = -100;
        backdrop.transform.position = new Vector3(0, 4, 5); backdrop.transform.localScale = new Vector3(40, 24, 1);
        UnityEngine.Object.DestroyImmediate(backgroundSprite);
        var grid = new GameObject("Map2_Earth_Grid"); grid.AddComponent<Grid>();
        var ground = new GameObject("Earth"); ground.transform.SetParent(grid.transform, false);
        var tilemap = ground.AddComponent<Tilemap>(); var tr = ground.AddComponent<TilemapRenderer>(); tr.sortingOrder = -5;
        var tile = ScriptableObject.CreateInstance<Tile>(); tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Environment/BuriedDepths/Tiles/Tiles32/Earth_r0_c1.png");
        AssetDatabase.CreateAsset(tile, Root + "/TestEarth.asset");
        for (int x = -20; x < 20; x++) for (int y = -4; y < 0; y++) tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Rootwraith_HandDrawn.prefab"), scene); instance.transform.position = new Vector3(2, 0, 0);
        var reference = new GameObject("Player_Size_Reference"); var ps = reference.AddComponent<SpriteRenderer>(); ps.sprite = visual.sprite; ps.sharedMaterial = sr ? sr.sharedMaterial : AssetDatabase.LoadAssetAtPath<Material>(Root + "/Rootwraith_Lit.mat"); ps.sortingOrder = 20;
        reference.transform.localScale = visual.transform.lossyScale;
        reference.transform.position = new Vector3(-1f - visual.sprite.bounds.center.x, -visual.sprite.bounds.min.y, 0);
        var harness = new GameObject("Animation_Test_Controls"); var preview = harness.AddComponent<RootwraithAnimationPreview>(); preview.target = instance.GetComponent<Animator>(); preview.body = instance.GetComponent<SpriteRenderer>(); preview.clips = clips;
        preview.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Fonts/beholden-book-font/BeholdenBook-Regular.ttf");
        Directory.CreateDirectory("Assets/Scenes/Test"); EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
        Selection.activeGameObject = instance;
        Debug.Log("Rootwraith test built: " + clips.Length + " clips; PPU=" + ppu + "; scene=" + ScenePath);
    }
    static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }
}
