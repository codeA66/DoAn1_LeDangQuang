using UnityEngine;

namespace LuHanh.Enemies.States
{
    /// <summary>Trung don: bi day lui theo huong don, khung lai mot luc.</summary>
    public class HurtState : EnemyStateBase
    {
        public HurtState(EnemyController enemy) : base(enemy) { }

        Vector2 knockback;
        float timer;

        public bool Finished => timer <= 0f;

        public void Setup(Vector2 knockbackVelocity) => knockback = knockbackVelocity;

        public override void Enter()
        {
            timer = Enemy.Config.hurtTime;
            // Chem ngang: bi danh bay ra sau va nay len. Chem len/xuong: khong hat quai bay.
            if (Mathf.Abs(knockback.x) > 0.01f)
                Enemy.Body.linearVelocity = new Vector2(knockback.x, Enemy.Config.knockbackUp);
            else
                Enemy.SetHorizontalSpeed(0f);
            Enemy.PlayAnim(EnemyAnim.Idle);
        }

        public override void FixedTick(float fixedDeltaTime) => timer -= fixedDeltaTime;
    }
}
