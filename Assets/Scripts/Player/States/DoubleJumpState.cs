namespace LuHanh.Player.States
{
    /// <summary>
    /// Nhay lan hai tren khong (ky nang Nhay doi, kieu Monarch Wings cua Hollow Knight): cao bang nhay thuong,
    /// giu phim thi cao hon. Chi mot lan moi lan roi khoi mat dat; hoi lai khi cham dat hoac bam tuong
    /// (<see cref="PlayerContext.AirJumpsLeft"/>). Di len xong thi FSM chuyen sang <see cref="FallState"/> nhu nhay thuong.
    /// Hieu ung hat sang: PlayerDashFx nghe StateChanged.
    /// </summary>
    public class DoubleJumpState : PlayerStateBase
    {
        public DoubleJumpState(PlayerContext context) : base(context) { }

        public override void Enter()
        {
            Ctx.ExecuteAirJump();
            Ctx.Animation.Play(PlayerAnim.Jump);
        }

        public override void Tick(float deltaTime)
        {
            Ctx.Facing.FaceTowards(Ctx.Input.Horizontal);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            Ctx.Mover.Move(Ctx.Input.MoveX, grounded: false, deltaTime: fixedDeltaTime);
            Ctx.ApplyGravity(fixedDeltaTime);
            Ctx.TryCornerCorrection(fixedDeltaTime);
        }
    }
}
