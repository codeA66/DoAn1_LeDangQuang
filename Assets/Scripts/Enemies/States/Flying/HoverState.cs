using UnityEngine;

namespace LuHanh.Enemies.States.Flying
{
    /// <summary>Luon qua lai quanh cho xuat phat, cham, quay mat theo huong bay.</summary>
    public class HoverState : FlyingStateBase
    {
        public HoverState(FlyingEnemyController enemy) : base(enemy) { }

        float time;

        public override void Enter()
        {
            time = 0f;
            Enemy.PlayFly();
        }

        public override void Tick(float deltaTime)
        {
            float vx = Enemy.Body.linearVelocity.x;
            if (Mathf.Abs(vx) > 0.1f) Enemy.Face(vx);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            var c = Enemy.Config;
            time += fixedDeltaTime;
            float sweep = Mathf.Sin(time * Mathf.PI * 2f / Mathf.Max(0.1f, c.hoverPeriod)) * c.hoverRange;
            Vector2 target = (Vector2)Enemy.StartPosition + new Vector2(sweep, 0f);
            Enemy.FlyTowards(target, c.hoverSpeed, fixedDeltaTime);
        }
    }
}
