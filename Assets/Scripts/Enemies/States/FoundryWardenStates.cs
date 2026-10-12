using LuHanh.Core.FSM;

namespace LuHanh.Enemies.States
{
    internal abstract class WardenState : IState
    {
        protected readonly FoundryWardenController Boss;
        protected float Elapsed;
        protected WardenState(FoundryWardenController boss) => Boss = boss;
        public virtual void Enter() => Elapsed = 0f;
        public virtual void Tick(float deltaTime) => Elapsed += deltaTime;
        public virtual void FixedTick(float deltaTime) => Boss.HoldPosition();
        public virtual void Exit() { }
    }

    internal sealed class WardenIdleState : WardenState
    {
        internal bool Ready => Boss.RestReady;
        public WardenIdleState(FoundryWardenController boss) : base(boss) { }
        public override void Enter() { base.Enter(); Boss.ClearEffects(); Boss.ShowIdle(0f); }
        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            Boss.FacePlayer();
            Boss.ShowIdle(Elapsed);
        }
    }

    internal sealed class WardenAttackState : WardenState
    {
        internal bool Finished => Elapsed >= Boss.Config.ChargeDuration +
            Boss.AttackActiveDuration + Boss.Config.RecoveryDuration;
        public WardenAttackState(FoundryWardenController boss) : base(boss) { }
        public override void Enter() { base.Enter(); Boss.BeginAttack(); }
        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            Boss.DrawAttack(Elapsed);
        }
        public override void FixedTick(float deltaTime)
        {
            base.FixedTick(deltaTime);
            Boss.StepAttack(Elapsed, deltaTime);
        }
        public override void Exit() => Boss.EndAttack();
    }

    internal sealed class WardenHurtState : WardenState
    {
        internal bool Finished => Elapsed >= Boss.Config.HurtDuration;
        public WardenHurtState(FoundryWardenController boss) : base(boss) { }
        public override void Enter() { base.Enter(); Boss.ClearEffects(); Boss.ShowHurt(0f); }
        public override void Tick(float deltaTime) { base.Tick(deltaTime); Boss.ShowHurt(Elapsed); }
    }

    internal sealed class WardenDeathState : WardenState
    {
        public WardenDeathState(FoundryWardenController boss) : base(boss) { }
        public override void Enter() { base.Enter(); Boss.ClearEffects(); Boss.ShowDeath(0f); }
        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            Boss.ShowDeath(Elapsed);
            if (Elapsed >= Boss.Config.DeathDuration) Boss.gameObject.SetActive(false);
        }
    }
}
