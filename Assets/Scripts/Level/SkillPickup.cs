using LuHanh.Player;
using LuHanh.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LuHanh.Level
{
    /// <summary>
    /// Vat pham mo khoa ky nang (bam tuong / ban cau): lo lung nhap nho, nguoi choi cham vao thi mo khoa,
    /// luu game va hien loi huong dan vai giay. Da mo khoa tu truoc thi tu an.
    /// Can Collider2D trigger tren cung object.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SkillPickup : MonoBehaviour
    {
        [SerializeField] Skill skill;
        [SerializeField] SpriteRenderer visual;
        [Tooltip("Lat frame cho hinh vat pham (de trong = giu sprite dang co).")]
        [SerializeField] Sprite[] frames;
        [SerializeField] float fps = 10f;
        [SerializeField] float bobHeight = 0.15f;
        [SerializeField] string title = "Mantis Claw";
        [SerializeField] string hint = "Press toward a wall in the air to cling, Space to wall jump";
        [SerializeField] float promptTime = 4f;
        [SerializeField] Font font;

        CanvasGroup prompt;
        Vector3 basePos;
        float promptTimer, frameTimer;
        int frame;
        bool taken;

        void Start()
        {
            basePos = visual ? visual.transform.localPosition : Vector3.zero;
            BuildPrompt();
            if (PlayerSkills.Has(skill)) Hide();
        }

        void Update()
        {
            if (promptTimer > 0f)
            {
                promptTimer -= Time.unscaledDeltaTime;
                if (prompt) prompt.alpha = Mathf.Clamp01(Mathf.Min(promptTimer, promptTime - promptTimer) * 2f);
            }
            if (taken || !visual) return;
            visual.transform.localPosition = basePos + Vector3.up * Mathf.Sin(Time.time * 2f) * bobHeight;
            if (frames == null || frames.Length == 0) return;
            frameTimer += Time.deltaTime;
            if (frameTimer < 1f / fps) return;
            frameTimer -= 1f / fps;
            frame = (frame + 1) % frames.Length;
            visual.sprite = frames[frame];
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (taken || !other.CompareTag("Player")) return;
            PlayerSkills.Unlock(skill);
            Hide();
            promptTimer = promptTime;
        }

        void Hide()
        {
            taken = true;
            if (visual) visual.enabled = false;
            foreach (var c in GetComponents<Collider2D>()) c.enabled = false;
            foreach (var l in GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>()) l.enabled = false;
        }

        void BuildPrompt()
        {
            var go = new GameObject("SkillPrompt", typeof(CanvasGroup));
            go.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(go, 50);
            prompt = go.GetComponent<CanvasGroup>();
            prompt.alpha = 0f;
            prompt.blocksRaycasts = false;
            MakeLine(go.transform, title, 54, new Color(0.72f, 1f, 0.94f), -150f);
            MakeLine(go.transform, hint, 34, new Color(0.9f, 0.88f, 0.97f), -215f);
        }

        void MakeLine(Transform parent, string value, int size, Color color, float y)
        {
            var t = UIBuild.MakeText("Text", parent, font, size, color);
            t.text = value;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            t.gameObject.AddComponent<Outline>().effectColor = new Color(0.05f, 0.04f, 0.08f, 0.9f);
        }
    }
}
