using LuHanh.Core.FSM;

namespace LuHanh.Enemies.States
{
    /// <summary>Nen chung: giu san controller, mac dinh rong.</summary>
    public abstract class EnemyStateBase : IState
    {
        protected readonly EnemyController Enemy;

        protected EnemyStateBase(EnemyController enemy) => Enemy = enemy;

        public virtual void Enter() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void FixedTick(float fixedDeltaTime) { }
        public virtual void Exit() { }
    }
}
