using UnityEngine;

namespace LuHanh.Enemies.States.Flying
{
    /// <summary>
    /// Trung don: bi day theo huong don (ca len/xuong vi dang bay), cham dan lai.
    /// Bi chem giua luc bao truoc thi phat ban do bi huy.
    /// </summary>
    public class FlyingHurtState : FlyingStateBase
    {
        public FlyingHurtState(FlyingEnemyController enemy) : base(enemy) { }

        Vector2 knockback;
        float timer;

        public bool Finished => timer <= 0f;

        public void Setup(Vector2 knockbackVelocity) => knockback = knockbackVelocity;

        public override void Enter()
        {
            timer = Enemy.Config.hurtTime;
            Enemy.Body.linearVelocity = knockback;
            Enemy.PlayFly();
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            timer -= fixedDeltaTime;
            Enemy.Brake(fixedDeltaTime);
        }
    }
}
