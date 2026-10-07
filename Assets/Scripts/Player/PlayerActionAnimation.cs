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

        enum ActionKind { None, Attack, Land, Death, Respawn }
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
            if ((kind == ActionKind.Attack || kind == ActionKind.Land) &&
                (!controller.enabled || controller.IsHurt || controller.IsDashing ||
                 (kind == ActionKind.Land && (!controller.IsGrounded || controller.IsMoving))))
            {
                Cancel();
                return;
            }
            float elapsed = (unscaled ? Time.unscaledTime : Time.time) - started;
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
