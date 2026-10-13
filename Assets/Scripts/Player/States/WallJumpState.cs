namespace LuHanh.Player.States
{
    /// <summary>
    /// Dap tuong bat ra. Trong <see cref="PlayerMovementConfig.wallJumpLockTime"/> dau bo qua phim trai/phai
    /// (giu da bat ra khoi tuong), sau do dieu khien tren khong nhu nhay thuong. Het di len thi sang Fall.
    /// </summary>
    public class WallJumpState : PlayerStateBase
    {
        readonly System.Func<int> wallDirSource;
        float elapsed;
        int pushDir;

        public WallJumpState(PlayerContext context, System.Func<int> wallDir) : base(context) => wallDirSource = wallDir;

        public override void Enter()
        {
            elapsed = 0f;
            int wall = wallDirSource();
            pushDir = -wall;
            Ctx.ExecuteWallJump(wall);
            Ctx.Facing.FaceTowards(pushDir);
            Ctx.Animation.Play(PlayerAnim.WallJump);
        }

        public override void Tick(float deltaTime)
        {
            elapsed += deltaTime;
            if (elapsed >= Ctx.Config.wallJumpLockTime) Ctx.Facing.FaceTowards(Ctx.Input.Horizontal);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            if (elapsed >= Ctx.Config.wallJumpLockTime)
                Ctx.Mover.Move(Ctx.Input.MoveX, grounded: false, deltaTime: fixedDeltaTime);
            Ctx.ApplyGravity(fixedDeltaTime);
            Ctx.TryCornerCorrection(fixedDeltaTime);
        }
    }
}
