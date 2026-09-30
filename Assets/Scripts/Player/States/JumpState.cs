namespace LuHanh.Player.States
{
    /// <summary>
    /// Pha di len cua cu nhay. Bat len kieu Celeste: giu phim thi van toc di len duoc
    /// giu nguyen toi da VarJumpTime, tha phim thi trong luc keo xuong ngay (nhay thap),
    /// khong cat van toc dot ngot. Phan tinh nam trong <see cref="PlayerContext.ApplyGravity"/>.
    /// Khi van toc doc khong con duong nua thi FSM tu chuyen sang <see cref="FallState"/>.
    /// </summary>
    public class JumpState : PlayerStateBase
    {
        public JumpState(PlayerContext context) : base(context) { }

        public override void Enter()
        {
            Ctx.ExecuteJump();
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
