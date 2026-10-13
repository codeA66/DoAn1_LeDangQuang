namespace LuHanh.Player.States
{
    /// <summary>
    /// Bam tuong truot cham (Mantis Claw cua Hollow Knight). Vao khi dang roi, sat tuong va bam phim ve phia tuong.
    /// Hinh WallSlide ve tuong o ben phai va mat quay ra ngoai, nen quay "mat" theo huong tuong.
    /// Cham tuong la hoi luot dash.
    /// </summary>
    public class WallSlideState : PlayerStateBase
    {
        public WallSlideState(PlayerContext context) : base(context) { }

        public int WallDir { get; private set; }

        public override void Enter()
        {
            WallDir = Ctx.WallDir;
            Ctx.Facing.FaceTowards(WallDir);
            Ctx.CancelJumpHold();
            Ctx.RefillDash();
            Ctx.Animation.Play(PlayerAnim.WallSlide);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            if (Ctx.WallDir != 0) WallDir = Ctx.WallDir;
            Ctx.ApplyWallSlide(fixedDeltaTime);
        }
    }
}
