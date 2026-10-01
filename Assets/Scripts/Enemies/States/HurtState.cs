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
            // Chi day ngang; chem len/xuong khong hat quai bay len.
            Enemy.SetHorizontalSpeed(knockback.x);
            Enemy.PlayAnim(EnemyAnim.Idle);
        }

        public override void FixedTick(float fixedDeltaTime) => timer -= fixedDeltaTime;
    }
}
