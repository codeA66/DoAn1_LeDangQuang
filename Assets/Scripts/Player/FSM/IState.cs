namespace LuHanh.Player.FSM
{
    /// <summary>Mot trang thai trong may trang thai. Vong doi: Enter -> Tick/FixedTick* -> Exit.</summary>
    public interface IState
    {
        /// <summary>Goi dung mot lan khi vua buoc vao trang thai.</summary>
        void Enter();

        /// <summary>Goi moi frame. Dung cho input va hinh anh.</summary>
        void Tick(float deltaTime);

        /// <summary>Goi moi buoc vat ly. Dung cho moi thu dung toi Rigidbody.</summary>
        void FixedTick(float fixedDeltaTime);

        /// <summary>Goi dung mot lan khi roi khoi trang thai.</summary>
        void Exit();
    }
}
