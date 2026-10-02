using UnityEngine;

namespace LuHanh.Enemies.States
{
    /// <summary>
    /// Duoi theo nguoi choi; toi mep vuc/tuong thi dung lai cho. Nguoi choi ra xa
    /// qua loseRange lien tuc loseTime giay thi bao mat dau.
    /// </summary>
    public class ChaseState : EnemyStateBase
    {
        public ChaseState(EnemyController enemy) : base(enemy) { }

        float lostTimer;

        public bool LostPlayer => lostTimer >= Enemy.Config.loseTime;

        public override void Enter() => lostTimer = 0f;

        public override void Tick(float deltaTime)
        {
            var c = Enemy.Config;
            float distSqr = Enemy.PlayerDistanceSqr;
            lostTimer = distSqr > c.loseRange * c.loseRange ? lostTimer + deltaTime : 0f;

            // Vung chet nho de khong quay qua quay lai khi dung ngay tren dau nguoi choi.
            if (Enemy.Player)
            {
                float dx = Enemy.Player.position.x - Enemy.transform.position.x;
                if (Mathf.Abs(dx) > 0.2f) Enemy.Face(dx);
            }
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            var c = Enemy.Config;
            if (Enemy.IsBlockedAhead())
            {
                Enemy.SetHorizontalSpeed(0f);
                Enemy.PlayAnim(EnemyAnim.Idle);
                return;
            }

            Enemy.SetHorizontalSpeed(Enemy.FacingSign * c.chaseSpeed);
            Enemy.PlayAnim(EnemyAnim.Walk);
        }
    }
}
