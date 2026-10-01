using LuHanh.Core.FSM;

namespace LuHanh.Enemies.States.Flying
{
    /// <summary>Nen chung cho trang thai quai bay: giu san controller, mac dinh rong.</summary>
    public abstract class FlyingStateBase : IState
    {
        protected readonly FlyingEnemyController Enemy;

        protected FlyingStateBase(FlyingEnemyController enemy) => Enemy = enemy;

        public virtual void Enter() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void FixedTick(float fixedDeltaTime) { }
        public virtual void Exit() { }
    }
}
