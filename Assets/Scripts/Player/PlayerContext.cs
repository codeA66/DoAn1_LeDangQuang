using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Nhung thu moi trang thai deu can. Gom vao mot doi tuong de constructor cua
    /// tung trang thai khong phai nhan 6-7 tham so, va de them thu moi ma khong
    /// phai sua chu ky cua tat ca cac trang thai.
    ///
    /// Trong luc tu tinh o day (Rigidbody2D.gravityScale = 0) de lam dung nhu Celeste:
    /// nhay giu phim, nua trong luc o dinh, roi nhanh khi giu phim xuong.
    /// </summary>
    public class PlayerContext
    {
        public PlayerContext(
            Rigidbody2D body,
            Collider2D collider,
            PlayerMovementConfig config,
            MovementInput input,
            GroundSensor ground,
            HorizontalMover mover,
            FacingController facing,
            PlayerAnimationDriver animation)
        {
            Body = body;
            Collider = collider;
            Config = config;
            Input = input;
            Ground = ground;
            Mover = mover;
            Facing = facing;
            Animation = animation;

            currentMaxFall = config.maxFall;
            DashesLeft = config.maxDashes;
        }

        public Rigidbody2D Body { get; }
        public Collider2D Collider { get; }
        public PlayerMovementConfig Config { get; }
        public MovementInput Input { get; }
        public GroundSensor Ground { get; }
        public HorizontalMover Mover { get; }
        public FacingController Facing { get; }
        public PlayerAnimationDriver Animation { get; }

        public float VerticalVelocity => Body.linearVelocity.y;

        /// <summary>Con nhay duoc khong: dang tren dat hoac con trong cua so coyote.</summary>
        public bool CanJump => Ground.CanCoyoteJump;

        /// <summary>Nguoi choi vua ra lenh nhay va lenh do con hieu luc.</summary>
        public bool WantsJump => Input.JumpBuffered;

        // ---- Nhay giu phim ----

        float varJumpTimer;
        float varJumpSpeed;
        float currentMaxFall;

        /// <summary>Bat len kieu Celeste: cong JumpHBoost, bat dau dem VarJumpTime.</summary>
        public void ExecuteJump()
        {
            var v = Body.linearVelocity;
            v.x += Config.jumpHBoost * Input.MoveX;
            v.y = Config.jumpSpeed;
            Body.linearVelocity = v;

            varJumpSpeed = v.y;
            varJumpTimer = Config.varJumpTime;

            Input.ConsumeJump();
            Ground.ConsumeCoyote();
        }

        /// <summary>
        /// Trong luc + giu phim nhay + roi nhanh. Goi moi buoc vat ly o moi trang thai
        /// tru dash (dash khong co trong luc).
        /// </summary>
        float airHoldTimer;

        /// <summary>Khung lo lung giua khong trung (vd ban cau tren khong): het trong luc va dung yen trong <paramref name="seconds"/> giay.</summary>
        public void HoldInAir(float seconds)
        {
            airHoldTimer = Mathf.Max(airHoldTimer, seconds);
            varJumpTimer = 0f;
        }

        public void ApplyGravity(float deltaTime)
        {
            if (airHoldTimer > 0f)
            {
                airHoldTimer -= deltaTime;
                Body.linearVelocity = Vector2.zero;
                return;
            }
            var v = Body.linearVelocity;

            // Buoc truoc da dat dung varJumpSpeed; nho hon nghia la vua dung tran -> het giu nhay,
            // neu khong nhan vat se bi dinh len tran cho toi het VarJumpTime.
            if (varJumpTimer > 0f && v.y < varJumpSpeed - 0.05f) varJumpTimer = 0f;
            if (varJumpTimer > 0f) varJumpTimer -= deltaTime;

            // Roi nhanh: dang roi het toc ma giu phim xuong thi nang dan gioi han roi.
            bool fastFalling = Input.MoveY == -1 && v.y <= -Config.maxFall;
            currentMaxFall = Mathf.MoveTowards(currentMaxFall,
                fastFalling ? Config.fastMaxFall : Config.maxFall,
                Config.fastMaxAccel * deltaTime);

            // Gan dinh ma con giu nhay thi chi nua trong luc - khoanh khac lo lung de canh duong.
            float mult = Mathf.Abs(v.y) < Config.halfGravThreshold && Input.JumpHeld ? 0.5f : 1f;
            v.y = Mathf.MoveTowards(v.y, -currentMaxFall, Config.gravity * mult * deltaTime);

            // Con giu phim trong VarJumpTime thi van toc di len khong duoc thap hon luc bat.
            if (varJumpTimer > 0f)
            {
                if (Input.JumpHeld) v.y = Mathf.Max(v.y, varJumpSpeed);
                else varJumpTimer = 0f;
            }

            Body.linearVelocity = v;
        }

        /// <summary>Huy nhay giu phim (vd khi bi danh bat ra) de khong bi keo len tiep.</summary>
        public void CancelJumpHold() => varJumpTimer = 0f;

        /// <summary>Hoi lai luot dash ngay (vd khi hoi sinh).</summary>
        public void RefillDash() => DashesLeft = Config.maxDashes;

        // ---- Dash ----

        float dashCooldownTimer;
        float dashRefillTimer;

        public int DashesLeft { get; private set; }

        public bool WantsDash => Input.DashBuffered;
        public bool CanDash => DashesLeft > 0 && dashCooldownTimer <= 0f;

        /// <summary>Huong dash 8 huong theo phim dang giu; khong giu gi thi theo huong dang nhin.</summary>
        public Vector2 DashDirection
        {
            get
            {
                var dir = new Vector2(Input.MoveX, Input.MoveY);
                if (dir == Vector2.zero) dir.x = Facing.FacingRight ? 1f : -1f;
                return dir.normalized;
            }
        }

        public void BeginDash()
        {
            DashesLeft--;
            dashCooldownTimer = Config.dashCooldown;
            dashRefillTimer = Config.dashRefillCooldown;
            varJumpTimer = 0f;
            Input.ConsumeDash();
        }

        /// <summary>Dem thoi gian hoi dash va hoi lai khi cham dat. Goi moi buoc vat ly.</summary>
        public void TickDash(float deltaTime, bool dashing)
        {
            if (dashCooldownTimer > 0f) dashCooldownTimer -= deltaTime;
            if (dashRefillTimer > 0f) dashRefillTimer -= deltaTime;

            if (!dashing && dashRefillTimer <= 0f && Ground.IsGrounded)
                DashesLeft = Config.maxDashes;
        }

        // ---- Bam tuong ----

        /// <summary>1 = tuong ben phai, -1 = ben trai, 0 = khong sat tuong. Cap nhat moi buoc vat ly.</summary>
        public int WallDir { get; private set; }

        /// <summary>Do tuong hai ben bang hop thu nho theo chieu doc (khong tinh platform mot chieu).</summary>
        /// <summary>Chan gan sat mat dat (khong cho bam tuong ngay tren san - tranh nhay qua lai Run/Fall/WallSlide).</summary>
        public bool NearGround { get; private set; }

        public void CheckWall()
        {
            WallDir = 0;
            NearGround = false;
            if (Collider == null) return;
            var b = Collider.bounds;
            // Tia thang xuong tu giua chan (hop rong se cham luon tuong dang ap sat).
            NearGround = Physics2D.Raycast(new Vector2(b.center.x, b.min.y + 0.05f), Vector2.down, 0.4f, Config.groundLayers);
            var size = new Vector2(b.size.x * 0.5f, b.size.y * 0.6f);
            int mask = Config.groundLayers;
            for (int dir = -1; dir <= 1; dir += 2)
            {
                var hit = Physics2D.BoxCast(b.center, size, 0f, new Vector2(dir, 0f), b.extents.x * 0.5f + Config.wallCheckDistance, mask);
                if (!hit.collider || hit.collider.usedByEffector || Mathf.Abs(hit.normal.x) < 0.9f) continue;
                // Tuong phai du cao: tia o 1/4 va 3/4 chieu cao than deu trung (canh be da mong khong tinh la tuong).
                float reach = b.extents.x + Config.wallCheckDistance + 0.05f;
                bool low = Physics2D.Raycast(new Vector2(b.center.x, b.min.y + b.size.y * 0.25f), new Vector2(dir, 0f), reach, mask);
                bool high = Physics2D.Raycast(new Vector2(b.center.x, b.min.y + b.size.y * 0.75f), new Vector2(dir, 0f), reach, mask);
                if (!low || !high) continue;
                WallDir = dir;
                return;
            }
        }

        /// <summary>Dap tuong bat ra phia nguoc tuong; giu nhay van keo dai nhu nhay thuong.</summary>
        public void ExecuteWallJump(int wallDir)
        {
            var v = new Vector2(-wallDir * Config.wallJumpHSpeed, Config.jumpSpeed * Config.wallJumpVRatio);
            Body.linearVelocity = v;
            varJumpSpeed = v.y;
            varJumpTimer = Config.varJumpTime;
            Input.ConsumeJump();
            Ground.ConsumeCoyote();
        }

        /// <summary>Truot tuong: trong luc binh thuong nhung khong roi nhanh hon wallSlideSpeed.</summary>
        public void ApplyWallSlide(float deltaTime)
        {
            var v = Body.linearVelocity;
            v.x = 0f;
            v.y = Mathf.Max(Mathf.MoveTowards(v.y, -Config.wallSlideSpeed, Config.gravity * deltaTime), -Config.wallSlideSpeed);
            Body.linearVelocity = v;
            currentMaxFall = Config.maxFall;
        }

        // ---- Corner correction ----

        /// <summary>
        /// Dang di len ma dau sap dung tran: thu dich sang trai / phai tung buoc nho,
        /// cho nao di len duoc thi truot sang do. Giong Celeste: dang di sang phai thi
        /// chi thu phia phai, dung yen thi thu ca hai phia.
        /// </summary>
        public void TryCornerCorrection(float deltaTime)
        {
            var v = Body.linearVelocity;
            if (v.y <= 0f || Collider == null) return;

            // Thu nho hop mot chut de khong tinh tuong dang ap sat hai ben la tran.
            var bounds = Collider.bounds;
            float skin = Physics2D.defaultContactOffset * 2f;
            var size = new Vector2(bounds.size.x - skin * 2f, bounds.size.y - skin * 2f);
            Vector2 centre = bounds.center;
            float distance = v.y * deltaTime + skin;
            int mask = Config.groundLayers;

            if (!Physics2D.BoxCast(centre, size, 0f, Vector2.up, distance, mask)) return;

            float step = Mathf.Max(0.01f, Config.cornerCorrectionStep);
            int steps = Mathf.RoundToInt(Config.cornerCorrection / step);

            for (int i = 1; i <= steps; i++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    if (side < 0 && v.x > 0f) continue;
                    if (side > 0 && v.x < 0f) continue;

                    var shift = new Vector2(side * i * step, 0f);
                    if (Physics2D.OverlapBox(centre + shift, size, 0f, mask)) continue;
                    if (Physics2D.BoxCast(centre + shift, size, 0f, Vector2.up, distance, mask)) continue;

                    Body.position += shift;
                    return;
                }
            }
        }
    }
}
