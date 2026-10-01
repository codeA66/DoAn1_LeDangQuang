using LuHanh.Core.FSM;

namespace LuHanh.Player.States
{
    /// <summary>
    /// Nen chung cho moi trang thai cua nhan vat: giu san context va cho san
    /// cai mac dinh rong, de trang thai con chi viet dung phan no quan tam.
    /// </summary>
    public abstract class PlayerStateBase : IState
    {
        protected readonly PlayerContext Ctx;

        protected PlayerStateBase(PlayerContext context) => Ctx = context;

        public virtual void Enter() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void FixedTick(float fixedDeltaTime) { }
        public virtual void Exit() { }
    }

    /// <summary>
    /// Phan chung cua Idle va Run: di chuyen bang gia toc mat dat, van ap trong luc
    /// de giu nhan vat bam dat, va quay mat theo input.
    /// </summary>
    public abstract class GroundedStateBase : PlayerStateBase
    {
        protected GroundedStateBase(PlayerContext context) : base(context) { }

        public override void Tick(float deltaTime)
        {
            Ctx.Facing.FaceTowards(Ctx.Input.Horizontal);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            Ctx.Mover.Move(Ctx.Input.MoveX, grounded: true, deltaTime: fixedDeltaTime);
            Ctx.ApplyGravity(fixedDeltaTime);
        }
    }
}
