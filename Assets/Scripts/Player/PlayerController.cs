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
    /// Luu y: UnityEvent cua PlayerInput tro toi <see cref="onMove"/> / <see cref="onJump"/>
    /// theo ten class kem namespace - doi namespace thi sua m_TargetAssemblyTypeName trong prefab.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Cau hinh")]
        [Tooltip("Bo so chuyen dong. De trong thi tu tao mot bo mac dinh luc chay.")]
        [SerializeField] PlayerMovementConfig config;

        [Header("Animation")]
        [SerializeField] PlayerAnimationDriver animations = new PlayerAnimationDriver();

        [Header("Debug")]
        [SerializeField] bool drawGroundGizmo = true;

        Rigidbody2D body;
        Animator animatorComponent;

        MovementInput input;
        GroundSensor ground;
        HorizontalMover mover;
        FacingController facing;
        PlayerContext context;

        /// <summary>Khung lo lung giua khong trung (ban cau tren khong).</summary>
        public void HoldInAir(float seconds) => context?.HoldInAir(seconds);
        StateMachine machine;

        IdleState idle;
        RunState run;
        JumpState jump;
        FallState fall;
        DashState dash;
        HurtState hurt;
        WallSlideState wallSlide;
        WallJumpState wallJump;
        bool hurtRequested;

        public bool IsMoving => input != null && input.HasHorizontal;
        public bool IsGrounded => ground != null && ground.IsGrounded;
        /// <summary>Collider dang dung len (null neu dang tren khong).</summary>
        public Collider2D GroundCollider => ground?.GroundCollider;

        public PlayerMovementConfig Config => config;

        public bool FacingRight => facing != null && facing.FacingRight;
        public bool IsDashing => machine != null && machine.Current == dash;
        /// <summary>-1 dang giu xuong, 1 dang giu len, 0 khong giu.</summary>
        public int MoveY => input != null ? input.MoveY : 0;
        public bool IsHurt => machine != null && machine.Current == hurt;
        public bool IsWallSliding => machine != null && machine.Current == wallSlide;
        /// <summary>Tuong dang bam: 1 phai, -1 trai (chi co nghia khi IsWallSliding).</summary>
        public int WallDir => wallSlide != null ? wallSlide.WallDir : 0;

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
            hurt = new HurtState(context);
            wallSlide = new WallSlideState(context);
            wallJump = new WallJumpState(context, () => wallSlide.WallDir);

            // Bi danh trung de len moi thu khac.
            machine.AddAnyTransition(hurt, () => hurtRequested);

            // Dash uu tien cao nhat, giong Celeste xet dash truoc nhay.
            machine.AddAnyTransition(dash, () => machine.Current != hurt && context.WantsDash && context.CanDash);

            // Dang bam tuong ma bam nhay: dap tuong bat ra (xet truoc nhay thuong).
            // Con trong cua so coyote (vua roi khoi mep) thi de nhay thuong xu ly: bam nhay ngay sau khi buoc khoi mep
            // ma lo cham mat ben mep dat thi khong bi dap tuong bat ra xa.
            machine.AddAnyTransition(wallJump, () => machine.Current == wallSlide && context.WantsJump && !context.CanJump);

            // Nhay: dung o dau cung nhay duoc mien la con coyote va con lenh trong dem,
            // tru luc dang dash. Nho vay bam nhay luon phan hoi ngay, khong tre frame nao.
            machine.AddAnyTransition(jump, () => machine.Current != dash && machine.Current != hurt && context.WantsJump && context.CanJump);

            // Het dash
            machine.AddTransition(dash, fall, () => dash.Finished && !ground.IsGrounded);
            machine.AddTransition(dash, run, () => dash.Finished && input.HasHorizontal);
            machine.AddTransition(dash, idle, () => dash.Finished && !input.HasHorizontal);

            // Het bi thuong
            machine.AddTransition(hurt, fall, () => hurt.Finished && !ground.IsGrounded);
            machine.AddTransition(hurt, run, () => hurt.Finished && input.HasHorizontal);
            machine.AddTransition(hurt, idle, () => hurt.Finished && !input.HasHorizontal);

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

            // Bam tuong (ky nang mo khoa): dang roi, sat tuong, bam phim ve phia tuong.
            machine.AddTransition(fall, wallSlide, CanWallSlide);
            machine.AddTransition(wallJump, wallSlide, () => context.VerticalVelocity <= 0f && CanWallSlide());
            machine.AddTransition(wallSlide, run, () => ground.IsGrounded && input.HasHorizontal);
            machine.AddTransition(wallSlide, idle, () => ground.IsGrounded && !input.HasHorizontal);
            machine.AddTransition(wallSlide, fall, () => context.WallDir == 0 || input.MoveX == -wallSlide.WallDir);
            machine.AddTransition(wallJump, fall, () => context.VerticalVelocity <= 0f);

            // Qua lai giua dung va chay
            machine.AddTransition(idle, run, () => input.HasHorizontal);
            machine.AddTransition(run, idle, () => !input.HasHorizontal);

            machine.SetState(idle);
        }

        void Update()
        {
            machine.Tick(Time.deltaTime);
            hurtRequested = false;
        }

        void FixedUpdate()
        {
            // Cam bien chay truoc: trang thai va luat chuyen deu doc ket qua cua frame nay.
            ground.FixedTick();
            context.CheckWall();
            context.TickDash(Time.fixedDeltaTime, dashing: machine.Current == dash);
            machine.FixedTick(Time.fixedDeltaTime);
        }

        bool CanWallSlide() =>
            PlayerSkills.Has(Skill.WallCling) && !ground.IsGrounded && !context.NearGround && context.VerticalVelocity <= 0f &&
            context.WallDir != 0 && input.MoveX == context.WallDir;

        /// <summary>Nhan toc do chay toi da (dang Shadow). 1 = binh thuong.</summary>
        public void SetRunMultiplier(float multiplier) => mover.SpeedMultiplier = multiplier;

        /// <summary>Quay mat theo dau cua <paramref name="direction"/> (vd chem tu dinh vi).</summary>
        public void Face(float direction) => facing.FaceTowards(direction);

        /// <summary>Bi danh trung: bat ra voi van toc cho truoc, mat dieu khien trong <paramref name="seconds"/>.</summary>
        public void Hurt(Vector2 launchVelocity, float seconds)
        {
            hurt.Setup(launchVelocity, seconds);
            hurtRequested = true;
        }

        /// <summary>Dua nhan vat ve vi tri hoi sinh: dung yen, dung tren dat, day dash.</summary>
        public void Respawn(Vector2 position)
        {
            body.position = position;
            transform.position = position;
            body.linearVelocity = Vector2.zero;
            hurtRequested = false;
            context.CancelJumpHold();
            context.RefillDash();
            machine.SetState(idle);
        }

        // ---- Input System: giu nguyen ten ham de khong phai noi lai UnityEvent trong scene ----

        /// <summary>Huong WASD dang bam - PlayerLook doc de nhin len/xuong.</summary>
        public Vector2 RawMove => rawMove;

        Vector2 rawMove;

        public void onMove(InputAction.CallbackContext callback)
        {
            rawMove = callback.ReadValue<Vector2>();
            input?.SetMove(rawMove);
        }

        public void onJump(InputAction.CallbackContext callback)
        {
            if (input == null) return;
            if (callback.started) input.PressJump();
            else if (callback.canceled) input.ReleaseJump();
        }

        public void onDash(InputAction.CallbackContext callback)
        {
            if (callback.started) input?.PressDash();
        }

        void OnDrawGizmosSelected()
        {
            if (drawGroundGizmo && ground != null) ground.DrawGizmos();
        }
    }
}
