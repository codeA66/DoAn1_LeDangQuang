using System;
using System.Collections.Generic;

namespace LuHanh.Core.FSM
{
    /// <summary>
    /// May trang thai huu han dieu khien bang dieu kien.
    ///
    /// Trang thai khong tu goi ten trang thai ke tiep; chung chi lo phan viec cua minh.
    /// Moi duong chuyen nam o mot cho duy nhat (noi goi <see cref="AddTransition"/>),
    /// nen doc mot cho la thay toan bo luat chuyen.
    /// </summary>
    public class StateMachine
    {
        class Transition
        {
            public readonly IState To;
            public readonly Func<bool> Condition;

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        static readonly List<Transition> Empty = new List<Transition>(0);

        // Theo tung trang thai (khong theo kieu): nhieu trang thai cung kieu (vd ActionState) van co luat rieng.
        readonly Dictionary<IState, List<Transition>> transitions = new();
        readonly List<Transition> anyTransitions = new();

        List<Transition> currentTransitions = Empty;

        public IState Current { get; private set; }

        /// <summary>Ban ra sau moi lan doi trang thai: (cu, moi). Tien cho debug va am thanh.</summary>
        public event Action<IState, IState> Changed;

        /// <summary>Chuyen thang sang mot trang thai, bo qua moi dieu kien.</summary>
        public void SetState(IState state)
        {
            if (state == null || state == Current) return;

            var previous = Current;
            previous?.Exit();
            Current = state;

            if (!transitions.TryGetValue(Current, out currentTransitions))
                currentTransitions = Empty;

            Current.Enter();
            Changed?.Invoke(previous, Current);
        }

        /// <summary>Chi chuyen tu <paramref name="from"/> sang <paramref name="to"/>.</summary>
        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (!transitions.TryGetValue(from, out var list))
            {
                list = new List<Transition>();
                transitions[from] = list;
            }
            list.Add(new Transition(to, condition));
        }

        /// <summary>Chuyen duoc tu bat ky trang thai nao. Duoc xet truoc cac duong rieng.</summary>
        public void AddAnyTransition(IState to, Func<bool> condition)
        {
            anyTransitions.Add(new Transition(to, condition));
        }

        public void Tick(float deltaTime)
        {
            // Xet chuyen truoc khi chay trang thai: phan ung ngay trong frame dieu kien
            // thoa man, khong tre mot frame - day la cho de sinh ra do tre cam nhan duoc.
            var next = FindTransition();
            if (next != null) SetState(next);

            Current?.Tick(deltaTime);
        }

        public void FixedTick(float fixedDeltaTime) => Current?.FixedTick(fixedDeltaTime);

        IState FindTransition()
        {
            foreach (var t in anyTransitions)
                if (t.To != Current && t.Condition()) return t.To;

            foreach (var t in currentTransitions)
                if (t.Condition()) return t.To;

            return null;
        }
    }
}
