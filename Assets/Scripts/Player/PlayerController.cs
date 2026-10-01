using LuHanh.Core.FSM;
using LuHanh.Player.States;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LuHanh.Player
{
    /// <summary>
    /// Diem lap rap cua nhan vat: tao cac bo phan, noi chung lai, khai bao luat chuyen
    /// trang thai, roi moi frame chi day nhip cho FSM chay.
    ///
    /// Ban than lop nay khong chua logic di chuyen - moi phep tinh nam trong
    /// <see cref="HorizontalMover"/>, <see cref="GroundSensor"/> va cac trang thai.
    ///
    /// De o global namespace de khong lam dut tham chieu component da gan san trong scene
    /// va cac UnityEvent cua PlayerInput dang tro toi <see cref="onMove"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Cau hinh")]
        [Tooltip("Bo so chuyen dong. De trong thi tu tao mot bo mac dinh luc chay.")]
        [SerializeField] PlayerMovementConfig config;

        [Header("Animation")]
        [SerializeField] PlayerAnimationDriver animations = new PlayerAnimationDriver();

        [Header("Input")]
        [Tooltip("Doc Space thang tu ban phim. Bat cho toi khi Input Actions asset co action 'Jump' " +
                 "noi vao onJump(); luc do tat di de tranh nhan hai lan.")]
        [SerializeField] bool readJumpFromKeyboard = true;

        [Header("Debug")]
        [SerializeField] bool drawGroundGizmo = true;

        Rigidbody2D body;
        Animator animatorComponent;

        MovementInput input;
        GroundSensor ground;
        HorizontalMover mover;
        FacingController facing;
        PlayerContext context;
        StateMachine machine;

        IdleState idle;
        RunState run;
        JumpState jump;
        FallState fall;
        DashState dash;

        /// <summary>Ten trang thai dang chay - tien cho HUD debug.</summary>
        public string CurrentStateName => machine?.Current?.GetType().Name ?? "-";

        public bool IsMoving => input != null && input.HasHorizontal;
        public bool IsGrounded => ground != null && ground.IsGrounded;

        public PlayerMovementConfig Config => config;

        public bool FacingRight => facing != null && facing.FacingRight;
        public bool IsDashing => machine != null && machine.Current == dash;
        /// <summary>-1 dang giu xuong, 1 dang giu len, 0 khong giu.</summary>
        public int MoveY => input != null ? input.MoveY : 0;

        /// <summary>Ban ra moi khi doi trang thai: (cu, moi). Hieu ung, am thanh nghe o day.</summary>
        public event System.Action<IState, IState> StateChanged;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            // Animator nam tren object con Visual de co gian sprite khong dung toi scale lat huong.
            animatorComponent = GetComponentInChildren<Animator>();

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<PlayerMovementConfig>();
                Debug.LogWarning("PlayerController chua duoc gan config, dang dung bo so mac dinh.", this);
            }

            body.freezeRotation = true;
            // Trong luc tu tinh trong PlayerContext.ApplyGravity.
            body.gravityScale = 0f;

            input = new MovementInput(config.inputBuffer);
            ground = new GroundSensor(transform, config);
            mover = new HorizontalMover(body, config);
            facing = new FacingController(transform, config.spriteFacesRight);

            animations.Initialise(animatorComponent);

            context = new PlayerContext(body, GetComponent<Collider2D>(), config, input, ground, mover, facing, animations);

            BuildStateMachine();
        }

        /// <summary>
        /// Toan bo luat chuyen trang thai nam o day - mot cho duy nhat.
        /// Thu tu quan trong: duong "bat ky" duoc xet truoc, va trong moi nhom thi
        /// duong khai bao truoc duoc uu tien.
        /// </summary>
        void BuildStateMachine()
        {
            machine = new StateMachine();
            machine.Changed += (from, to) => StateChanged?.Invoke(from, to);

            idle = new IdleState(context);
            run = new RunState(context);
            jump = new JumpState(context);
            fall = new FallState(context);
            dash = new DashState(context);

            // Dash uu tien cao nhat, giong Celeste xet dash truoc nhay.
            machine.AddAnyTransition(dash, () => context.WantsDash && context.CanDash);

            // Nhay: dung o dau cung nhay duoc mien la con coyote va con lenh trong dem,
            // tru luc dang dash. Nho vay bam nhay luon phan hoi ngay, khong tre frame nao.
            machine.AddAnyTransition(jump, () => machine.Current != dash && context.WantsJump && context.CanJump);

            // Het dash
            machine.AddTransition(dash, fall, () => dash.Finished && !ground.IsGrounded);
            machine.AddTransition(dash, run, () => dash.Finished && input.HasHorizontal);
            machine.AddTransition(dash, idle, () => dash.Finished && !input.HasHorizontal);

            // Roi khoi mep ma khong nhay
            machine.AddTransition(idle, fall, () => !ground.IsGrounded);
            machine.AddTransition(run, fall, () => !ground.IsGrounded);

            // Het da di len thi chuyen sang roi
            machine.AddTransition(jump, fall, () => context.VerticalVelocity <= 0f);

            // Tiep dat: vao thang Idle hay Run tuy con giu phim khong
            machine.AddTransition(fall, run, () => ground.IsGrounded && input.HasHorizontal);
            machine.AddTransition(fall, idle, () => ground.IsGrounded && !input.HasHorizontal);
            // Chi tiep dat khi da het di len: ngay sau khi nhay cam bien van con bao cham dat vai frame.
            machine.AddTransition(jump, run, () => ground.IsGrounded && context.VerticalVelocity <= 0f && input.HasHorizontal);
            machine.AddTransition(jump, idle, () => ground.IsGrounded && context.VerticalVelocity <= 0f && !input.HasHorizontal);

            // Qua lai giua dung va chay
            machine.AddTransition(idle, run, () => input.HasHorizontal);
            machine.AddTransition(run, idle, () => !input.HasHorizontal);

            machine.SetState(idle);
        }

        void Update()
        {
            if (readJumpFromKeyboard) PollKeyboardJump();
            PollKeyboardDash();
            machine.Tick(Time.deltaTime);
        }

        void PollKeyboardJump()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.spaceKey.wasPressedThisFrame) input.PressJump();
            else if (kb.spaceKey.wasReleasedThisFrame) input.ReleaseJump();
        }

        // Input Actions asset chua co action Dash nen doc Shift thang tu ban phim.
        void PollKeyboardDash()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.leftShiftKey.wasPressedThisFrame) input.PressDash();
        }

        void FixedUpdate()
        {
            // Cam bien chay truoc: trang thai va luat chuyen deu doc ket qua cua frame nay.
            ground.FixedTick();
            context.TickDash(Time.fixedDeltaTime, dashing: machine.Current == dash);
            machine.FixedTick(Time.fixedDeltaTime);
        }

        // ---- Input System: giu nguyen ten ham de khong phai noi lai UnityEvent trong scene ----

        public void onMove(InputAction.CallbackContext callback)
        {
            input?.SetMove(callback.ReadValue<Vector2>());
        }

        public void onJump(InputAction.CallbackContext callback)
        {
            if (input == null) return;
            if (callback.started) input.PressJump();
            else if (callback.canceled) input.ReleaseJump();
        }

        void OnDrawGizmosSelected()
        {
            if (drawGroundGizmo && ground != null) ground.DrawGizmos();
        }
    }
}
