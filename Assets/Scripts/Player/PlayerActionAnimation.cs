using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>Dong tac than ngan, phat sau Animator ma khong khoa FSM di chuyen.</summary>
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerActionAnimation : MonoBehaviour
    {
        [System.Serializable]
        public class Sequence
        {
            [Tooltip("Cac sprite theo thu tu phat, cung ty le va diem neo.")]
            public Sprite[] frames;
            [Min(0.01f), Tooltip("Thoi luong ca dong tac, tinh bang giay.")]
            public float duration = 0.18f;
            public bool Ready => frames != null && frames.Length > 0;
        }

        [SerializeField, Tooltip("SpriteRenderer than nhan vat.")]
        SpriteRenderer visual;
        [SerializeField, Tooltip("Animator di chuyen tren Visual.")]
        Animator animator;
        [SerializeField, Tooltip("Chem ngang, vet chem do PlayerAttack ve rieng.")]
        Sequence side = new Sequence();
        [SerializeField, Tooltip("Chem len.")]
        Sequence up = new Sequence();
        [SerializeField, Tooltip("Chem xuong tren khong.")]
        Sequence down = new Sequence();
        [SerializeField, Tooltip("Tiep dat khi dung yen, bo qua neu dang chay.")]
        Sequence land = new Sequence { duration = 0.15f };
        [SerializeField, Tooltip("Tan bien khi chet.")]
        Sequence death = new Sequence { duration = 0.6f };
        [SerializeField, Tooltip("Tu lai khi hoi sinh.")]
        Sequence respawn = new Sequence { duration = 0.5f };
        [SerializeField, Tooltip("Ban cau khi dung / chay.")]
        Sequence cast = new Sequence { duration = 0.4f };
        [SerializeField, Tooltip("Ban cau tren khong.")]
        Sequence castAir = new Sequence { duration = 0.4f };
        [SerializeField, Tooltip("Ngoi xuong ghe checkpoint.")]
        Sequence sitDown = new Sequence { duration = 0.45f };
        [SerializeField, Tooltip("Ngoi nghi (lap cho toi khi dung day).")]
        Sequence sit = new Sequence { duration = 1.2f };
        [SerializeField, Tooltip("Dung day khoi ghe.")]
        Sequence standUp = new Sequence { duration = 0.45f };

        enum ActionKind { None, Attack, Land, Death, Respawn, Cast, SitDown, Sit, StandUp }
        PlayerController controller;
        Sequence active;
        ActionKind kind;
        float started, duration;
        bool unscaled;

        void Awake() => controller = GetComponent<PlayerController>();

        public bool PlayAttack(PlayerAttack.Direction direction)
        {
            if (!controller || controller.IsHurt || controller.IsDashing ||
                kind == ActionKind.Death || kind == ActionKind.Respawn) return false;
            return Begin(direction == PlayerAttack.Direction.Up ? up :
                direction == PlayerAttack.Direction.Down ? down : side, ActionKind.Attack);
        }

        public bool PlayLanding()
        {
            if (!controller || controller.IsMoving || controller.IsHurt || kind != ActionKind.None)
                return false;
            return Begin(land, ActionKind.Land);
        }

        /// <summary>Dong tac ban cau. Bi danh / dash thi ngat nhu don chem.</summary>
        public bool PlayCast(bool air)
        {
            if (!controller || controller.IsHurt || controller.IsDashing ||
                kind == ActionKind.Death || kind == ActionKind.Respawn) return false;
            return Begin(air ? castAir : cast, ActionKind.Cast);
        }

        /// <summary>Ngoi ghe: ngoi xuong roi tu chuyen sang ngoi nghi lap cho toi khi goi <see cref="PlayStandUp"/>.</summary>
        public void PlaySit() => Begin(sitDown, ActionKind.SitDown);

        /// <summary>Dung day khoi ghe. Tra ve thoi luong dong tac (giay).</summary>
        public float PlayStandUp() => Begin(standUp, ActionKind.StandUp) ? standUp.duration : 0f;

        public void PlayDeath(float seconds) => Begin(death, ActionKind.Death, true, seconds);
        public void PlayRespawn(float seconds) => Begin(respawn, ActionKind.Respawn, true, seconds);

        bool Begin(Sequence sequence, ActionKind next, bool realTime = false, float seconds = -1f)
        {
            if (!isActiveAndEnabled || !visual || sequence == null || !sequence.Ready) return false;
            active = sequence;
            kind = next;
            unscaled = realTime;
            started = realTime ? Time.unscaledTime : Time.time;
            duration = Mathf.Max(0.01f, seconds >= 0f ? seconds : sequence.duration);
            visual.sprite = active.frames[0];
            return true;
        }

        public void Cancel()
        {
            bool wasActive = kind != ActionKind.None;
            kind = ActionKind.None;
            active = null;
            if (wasActive && animator && animator.isActiveAndEnabled)
                animator.Update(0f);
        }

        void LateUpdate()
        {
            if (kind == ActionKind.None || !visual) return;
            if ((kind == ActionKind.Attack || kind == ActionKind.Land || kind == ActionKind.Cast) &&
                (!controller.enabled || controller.IsHurt || controller.IsDashing ||
                 (kind == ActionKind.Land && (!controller.IsGrounded || controller.IsMoving))))
            {
                Cancel();
                return;
            }
            float elapsed = (unscaled ? Time.unscaledTime : Time.time) - started;
            if (kind == ActionKind.SitDown && elapsed >= duration)
            {
                Begin(sit, ActionKind.Sit);
                elapsed = 0f;
            }
            if (kind == ActionKind.Sit)
            {
                // Ngoi nghi lap vo han.
                int loop = Mathf.FloorToInt(elapsed / duration * active.frames.Length) % active.frames.Length;
                visual.sprite = active.frames[loop];
                return;
            }
            if (elapsed >= duration && kind != ActionKind.Death && kind != ActionKind.Respawn)
            {
                Cancel();
                return;
            }
            // Chet/hoi sinh giu khung cuoi cho den khi callback chuyen canh hoan tat.
            int index = Mathf.Clamp(Mathf.FloorToInt(elapsed / duration * active.frames.Length),
                0, active.frames.Length - 1);
            visual.sprite = active.frames[index];
        }

        void OnDisable() => Cancel();
    }
}
