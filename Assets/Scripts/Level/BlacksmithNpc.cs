using LuHanh.Player;
using LuHanh.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LuHanh.Level
{
    /// <summary>
    /// NPC tho ren "Old Anvil": dung gan hien "E - Talk"; bam E -> khoa dieu khien nguoi choi, hoi "Want to upgrade?"
    /// voi Yes / No (<see cref="ChoiceDialog"/>). Yes -> <see cref="onAccepted"/> (mo bang nang cap), No -> tra dieu khien.
    /// Bang nang cap tu goi <see cref="EndTalk"/> khi dong de tra dieu khien.
    /// </summary>
    public class BlacksmithNpc : MonoBehaviour
    {
        [SerializeField] string npcName = "Old Anvil";
        [SerializeField] string question = "Want to upgrade?";
        [SerializeField] string yesLabel = "Yes";
        [SerializeField] string noLabel = "No";
        [Tooltip("Khoang cach ngang toi da de noi chuyen (unit).")]
        [SerializeField] float talkRange = 2.2f;
        [SerializeField] Font font;
        [Tooltip("Hinh NPC (de quay mat ve phia nguoi choi). Ve goc quay sang phai.")]
        [SerializeField] SpriteRenderer visual;
        [Tooltip("Nguoi choi chon Yes. Bang nang cap phai goi EndTalk() khi dong.")]
        [SerializeField] UnityEvent onAccepted;

        [Header("Animation (ten state trong Animator, de trong = khong doi)")]
        [SerializeField] Animator animator;
        [Tooltip("Nguoi choi o xa: dang lam viec.")]
        [SerializeField] string farState = "Hammer";
        [Tooltip("Nguoi choi lai gan: dung tay nhin.")]
        [SerializeField] string nearState = "Idle";
        [Tooltip("Dang noi chuyen / mo bang nang cap.")]
        [SerializeField] string talkState = "Talk";
        string currentState;

        PlayerController controller;
        PlayerInput input;
        InputAction interact;
        MonoBehaviour[] blockWhileTalking;
        CanvasGroup prompt;
        bool talking;
        int endFrame = -1;   // frame vua ket thuc hoi thoai: bo qua phim E cua chinh frame do (khong mo lai ngay)

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p)
            {
                controller = p.GetComponent<PlayerController>();
                input = p.GetComponent<PlayerInput>();
                if (input) interact = input.actions.FindAction("Interact");
                blockWhileTalking = new MonoBehaviour[]
                {
                    controller, p.GetComponent<PlayerAttack>(), p.GetComponent<PlayerOrbShot>(), p.GetComponent<PlayerShadow>(),
                    p.GetComponent<PlayerFocus>()
                };
            }
            BuildPrompt();
        }

        void Update()
        {
            if (!controller) return;
            if (talking) { PlayState(talkState); return; }
            Vector2 d = controller.transform.position - transform.position;
            bool near = controller.enabled && controller.IsGrounded && Mathf.Abs(d.x) <= talkRange && Mathf.Abs(d.y) <= 1.5f;
            PlayState(near ? nearState : farState);
            if (prompt) prompt.alpha = Mathf.MoveTowards(prompt.alpha, near ? 1f : 0f, Time.unscaledDeltaTime * 6f);
            if (visual && near) visual.flipX = d.x < 0f;
            if (near && Time.frameCount != endFrame && interact != null && interact.WasPressedThisFrame()) BeginTalk();
        }

        void BeginTalk()
        {
            talking = true;
            if (prompt) prompt.alpha = 0f;
            foreach (var b in blockWhileTalking) if (b) b.enabled = false;
            var body = controller.GetComponent<Rigidbody2D>();
            if (body) body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            controller.Face(transform.position.x - controller.transform.position.x);
            ChoiceDialog.Show(input, font, npcName, question, new[] { yesLabel, noLabel }, OnChosen);
        }

        void OnChosen(int choice)
        {
            if (choice == 0 && onAccepted.GetPersistentEventCount() > 0) onAccepted.Invoke();
            else EndTalk();
        }

        /// <summary>Ket thuc hoi thoai, tra dieu khien cho nguoi choi.</summary>
        public void EndTalk()
        {
            talking = false;
            endFrame = Time.frameCount;
            foreach (var b in blockWhileTalking) if (b) b.enabled = true;
            if (controller) controller.ClearBufferedInput();
        }

        void OnDisable()
        {
            if (talking) EndTalk();
            currentState = null;
        }

        /// <summary>Doi state Animator neu khac state dang phat (khong goi Play moi frame).</summary>
        void PlayState(string state)
        {
            if (!animator || string.IsNullOrEmpty(state) || state == currentState) return;
            currentState = state;
            animator.Play(state, 0, 0f);
        }

        void BuildPrompt()
        {
            var go = new GameObject("NpcPrompt", typeof(CanvasGroup));
            go.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(go, 40);
            prompt = go.GetComponent<CanvasGroup>();
            prompt.alpha = 0f;
            prompt.blocksRaycasts = false;
            var t = UIBuild.MakeText("Text", go.transform, font, 34, new Color(0.72f, 1f, 0.94f));
            t.text = "E  -  Talk";
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 140f);
            t.gameObject.AddComponent<Outline>().effectColor = new Color(0.05f, 0.04f, 0.08f, 0.9f);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.72f, 1f, 0.94f, 0.4f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.75f, new Vector3(talkRange * 2f, 3f, 0f));
        }
    }
}
