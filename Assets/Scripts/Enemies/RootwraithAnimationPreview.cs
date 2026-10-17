using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace LuHanh.Enemies
{
    /// <summary>Visual QA only. No combat AI, damage, or save-game interaction.</summary>
    public sealed class RootwraithAnimationPreview : MonoBehaviour
    {
        public Animator target;
        public SpriteRenderer body;
        public AnimationClip[] clips;
        public Font font;
        public bool autoCycle = true;
        public bool paused;
        public int selected;
        public float elapsed;
        GUIStyle labelStyle, buttonStyle;
        readonly List<GameObject> suspendedRoots = new List<GameObject>();

        void Awake()
        {
            // Additive test scene leaves the owner's open scene untouched on disk.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene == gameObject.scene) continue;
                foreach (var root in scene.GetRootGameObjects())
                    if (root.activeSelf) { suspendedRoots.Add(root); root.SetActive(false); }
            }
        }

        void OnDisable()
        {
            foreach (var root in suspendedRoots) if (root) root.SetActive(true);
            suspendedRoots.Clear();
        }

        void Start() { Select(0); }

        public void Select(int index)
        {
            if (clips == null || clips.Length == 0 || !target) return;
            selected = (index + clips.Length) % clips.Length;
            elapsed = 0;
            target.Play(clips[selected].name, 0, 0);
            target.Update(0);
            target.speed = paused ? 0 : 1;
        }

        void Update()
        {
            if (!target || clips == null || clips.Length == 0) return;
            target.speed = paused ? 0 : 1;
            if (paused || !autoCycle) return;
            elapsed += Time.unscaledDeltaTime;
            float duration = clips[selected].name == "Idle" ? clips[selected].length * 3 : clips[selected].length + .35f;
            if (elapsed >= duration) Select(selected + 1);
        }

        void OnGUI()
        {
            if (!font || clips == null || clips.Length == 0) return;
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 20, wordWrap = true };
                buttonStyle = new GUIStyle(GUI.skin.button) { font = font, fontSize = 17 };
            }
            float sx = Screen.width / 1280f, sy = Screen.height / 720f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(sx, sy, 1));
            GUI.Box(new Rect(16, 16, 330, 554), "");
            GUI.Label(new Rect(30, 28, 300, 34), "ROOTWRAITH - ANIMATION TEST", labelStyle);
            GUI.Label(new Rect(30, 65, 300, 54), "12 fps | Idle 130 x 216 px\nVisual preview - no combat AI", labelStyle);
            for (int i = 0; i < clips.Length; i++)
                if (GUI.Button(new Rect(30, 128 + i * 28, 300, 26), (i == selected ? "> " : "") + clips[i].name, buttonStyle)) { autoCycle = false; Select(i); }
            if (GUI.Button(new Rect(30, 475, 145, 30), paused ? "Resume" : "Pause", buttonStyle)) paused = !paused;
            if (GUI.Button(new Rect(185, 475, 145, 30), autoCycle ? "Auto: ON" : "Auto: OFF", buttonStyle)) autoCycle = !autoCycle;
            if (GUI.Button(new Rect(30, 515, 145, 30), "Restart clip", buttonStyle)) Select(selected);
            if (GUI.Button(new Rect(185, 515, 145, 30), "Next frame", buttonStyle))
            {
                paused = true; autoCycle = false;
                float t = target.GetCurrentAnimatorStateInfo(0).normalizedTime * clips[selected].length;
                t = (Mathf.Floor(t * 12 + .001f) + 1) / 12f;
                if (t >= clips[selected].length) t = 0;
                target.Play(clips[selected].name, 0, t / clips[selected].length); target.Update(0);
            }
            // Enlarged crop view uses original sprite UV, never the entire source sheet.
            if (body && body.sprite)
            {
                var s = body.sprite; var r = s.rect; var texture = s.texture;
                GUI.Box(new Rect(930, 16, 334, 338), "");
                GUI.Label(new Rect(946, 26, 300, 28), "FRAME INSPECTION", labelStyle);
                GUI.DrawTextureWithTexCoords(new Rect(937, 60, 320, 288), texture,
                    new Rect(r.x / texture.width, r.y / texture.height, r.width / texture.width, r.height / texture.height), true);
            }
            GUI.matrix = Matrix4x4.identity;
        }
    }
}
