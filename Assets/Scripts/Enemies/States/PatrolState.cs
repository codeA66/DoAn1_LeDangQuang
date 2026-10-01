namespace LuHanh.Enemies.States
{
    /// <summary>Di qua lai; gap tuong hoac mep vuc thi quay dau.</summary>
    public class PatrolState : EnemyStateBase
    {
        public PatrolState(EnemyController enemy) : base(enemy) { }

        public override void Enter() => Enemy.PlayAnim(EnemyAnim.Walk);

        public override void FixedTick(float fixedDeltaTime)
        {
            if (Enemy.IsBlockedAhead()) Enemy.Face(-Enemy.FacingSign);
            Enemy.SetHorizontalSpeed(Enemy.FacingSign * Enemy.Config.patrolSpeed);
        }
    }
}
