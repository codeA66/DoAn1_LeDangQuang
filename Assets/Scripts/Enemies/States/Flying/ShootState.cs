namespace LuHanh.Enemies.States.Flying
{
    /// <summary>
    /// Ban: dung lai va rung nhe telegraphTime giay de bao truoc, roi ban mot qua cau
    /// nham vao nguoi choi. Xong thi quay ve danh nhau.
    /// </summary>
    public class ShootState : FlyingStateBase
    {
        public ShootState(FlyingEnemyController enemy) : base(enemy) { }

        float timer;
        bool fired;

        public bool Finished => fired;

        public override void Enter()
        {
            timer = Enemy.Config.telegraphTime;
            fired = false;
            Enemy.Telegraphing = true;
            Enemy.PlayAttack();
        }

        public override void Tick(float deltaTime)
        {
            Enemy.FacePlayer();
            timer -= deltaTime;
            if (fired || timer > 0f) return;

            Enemy.Fire();
            fired = true;
        }

        public override void FixedTick(float fixedDeltaTime) => Enemy.Brake(fixedDeltaTime);

        public override void Exit()
        {
            Enemy.Telegraphing = false;
            Enemy.ShotCooldown = Enemy.Config.shootInterval;
        }
    }
}
