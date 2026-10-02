using UnityEngine;

namespace LuHanh.Enemies.States
{
    /// <summary>
    /// Don lao cua quai di bo: gong (dung yen, quay ve nguoi choi) -> lao thang (hitbox bat,
    /// trung toi da 1 lan) -> dung lai thoi. Gap tuong / mep vuc khi dang lao thi dung ngay.
    /// Chi gay sat thuong trong pha lao; cham vao luc khac khong mat mau.
    /// </summary>
    public class DashAttackState : EnemyStateBase
    {
        enum Phase { Windup, Dash, Recover }

        public DashAttackState(EnemyController enemy) : base(enemy) { }

        Phase phase;
        float timer;

        public bool Finished { get; private set; }

        public override void Enter()
        {
            Finished = false;
            phase = Phase.Windup;
            timer = Enemy.Config.dashWindup;
            if (Enemy.Player) Enemy.Face(Enemy.Player.position.x - Enemy.transform.position.x);
            Enemy.SetHorizontalSpeed(0f);
            Enemy.PlayAnim(EnemyAnim.Attack);
            Enemy.PlayWindupFx(true);
        }

        public override void Exit()
        {
            // Bi chem ngat giua chung cung phai tat khoi, hitbox va tinh hoi chieu.
            Enemy.PlayWindupFx(false);
            Enemy.SetAttackHitbox(false);
            Enemy.StartDashCooldown();
        }

        public override void Tick(float deltaTime)
        {
            timer -= deltaTime;
            if (timer > 0f) return;

            if (phase == Phase.Windup) BeginDash();
            else if (phase == Phase.Dash) BeginRecover();
            else Finished = true;
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            if (phase != Phase.Dash)
            {
                Enemy.SetHorizontalSpeed(0f);
                return;
            }
            if (Enemy.IsBlockedAhead())
            {
                BeginRecover();
                return;
            }
            Enemy.SetHorizontalSpeed(Enemy.FacingSign * Enemy.Config.dashSpeed);
        }

        void BeginDash()
        {
            phase = Phase.Dash;
            timer = Enemy.Config.dashTime;
            Enemy.SetAttackHitbox(true);
        }

        void BeginRecover()
        {
            phase = Phase.Recover;
            timer = Enemy.Config.dashRecover;
            Enemy.SetAttackHitbox(false);
            Enemy.SetHorizontalSpeed(0f);
            Enemy.PlayAnim(EnemyAnim.Idle);
        }
    }
}
