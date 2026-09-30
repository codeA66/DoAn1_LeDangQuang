namespace LuHanh.Player.States
{
    /// <summary>
    /// Dang roi xuong - vao day ca khi roi khoi mep ma khong nhay.
    /// Cua so coyote van mo trong nhung frame dau, nen FSM co the chuyen thang
    /// tu day sang JumpState neu nguoi choi bam nhay kip.
    /// </summary>
    public class FallState : PlayerStateBase
    {
        public FallState(PlayerContext context) : base(context) { }

        public override void Enter() => Ctx.Animation.Play(PlayerAnim.Fall);

        public override void Tick(float deltaTime)
        {
            Ctx.Facing.FaceTowards(Ctx.Input.Horizontal);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            Ctx.Mover.Move(Ctx.Input.MoveX, grounded: false, deltaTime: fixedDeltaTime);
            Ctx.ApplyGravity(fixedDeltaTime);
        }
    }
}
