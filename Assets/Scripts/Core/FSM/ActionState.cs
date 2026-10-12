using System;

namespace LuHanh.Core.FSM
{
    /// <summary>
    /// Trang thai khai bao bang ham (khong can viet lop rieng cho trang thai don gian).
    /// <see cref="Elapsed"/> dem tu luc vao trang thai.
    /// </summary>
    public sealed class ActionState : IState
    {
        readonly Action enter, exit;
        readonly Action<float> tick, fixedTick;

        public float Elapsed { get; private set; }

        public ActionState(Action enter = null, Action<float> tick = null, Action<float> fixedTick = null, Action exit = null)
        {
            this.enter = enter;
            this.tick = tick;
            this.fixedTick = fixedTick;
            this.exit = exit;
        }

        public void Enter() { Elapsed = 0f; enter?.Invoke(); }
        public void Tick(float deltaTime) { Elapsed += deltaTime; tick?.Invoke(deltaTime); }
        public void FixedTick(float fixedDeltaTime) => fixedTick?.Invoke(fixedDeltaTime);
        public void Exit() => exit?.Invoke();
    }
}
