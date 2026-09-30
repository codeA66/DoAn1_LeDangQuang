namespace LuHanh.Player.States
{
    /// <summary>Dung yen tren mat dat. Ma sat mat dat tu keo van toc ve 0.</summary>
    public class IdleState : GroundedStateBase
    {
        public IdleState(PlayerContext context) : base(context) { }

        public override void Enter()
        {
            base.Enter();
            Ctx.Animation.Play(PlayerAnim.Idle);
        }
    }
}
