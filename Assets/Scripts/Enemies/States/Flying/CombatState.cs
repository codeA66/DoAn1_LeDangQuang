using UnityEngine;

namespace LuHanh.Enemies.States.Flying
{
    /// <summary>
    /// Danh nhau: bay giu khoang cach voi nguoi choi (dung ben dang dung, cao hon nguoi choi),
    /// dem gio toi phat ban ke tiep. Nguoi choi ra xa qua loseRange lien tuc loseTime giay
    /// thi bao mat dau.
    /// </summary>
    public class CombatState : FlyingStateBase
    {
        public CombatState(FlyingEnemyController enemy) : base(enemy) { }

        float lostTimer;

        public bool LostPlayer => lostTimer >= Enemy.Config.loseTime;

        public override void Enter()
        {
            lostTimer = 0f;
            Enemy.PlayFly();
        }

        public override void Tick(float deltaTime)
        {
            var c = Enemy.Config;
            lostTimer = Enemy.PlayerDistanceSqr > c.loseRange * c.loseRange ? lostTimer + deltaTime : 0f;
            Enemy.ShotCooldown -= deltaTime;
            Enemy.FacePlayer();
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            if (!Enemy.Player) return;
            var c = Enemy.Config;
            Vector2 player = Enemy.Player.position;
            Vector2 me = Enemy.Body.position;

            // Giu nguyen ben dang dung; khoang cach ngang ep vao [min, max].
            float side = me.x >= player.x ? 1f : -1f;
            float dx = Mathf.Clamp(Mathf.Abs(me.x - player.x), c.keepDistanceMin, c.keepDistanceMax);
            Vector2 target = new Vector2(player.x + side * dx, player.y + c.heightAbovePlayer);
            Enemy.FlyTowards(target, c.maxSpeed, fixedDeltaTime);
        }
    }
}
