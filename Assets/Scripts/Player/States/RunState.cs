namespace LuHanh.Player.States
{
    /// <summary>Chay tren mat dat.</summary>
    public class RunState : GroundedStateBase
    {
        public RunState(PlayerContext context) : base(context) { }

        public override void Enter()
        {
            base.Enter();
            Ctx.Animation.Play(PlayerAnim.Walk);
        }
    }
}
