using LuHanh.Combat;
using LuHanh.Core.FSM;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Quai ban xa (Bo cap): tuan tra -> thay nguoi choi thi giu khoang cach (gan qua thi lui) ->
    /// bao truoc ro (shootWindup) roi phun noc thang vao nguoi choi -> hoi (cua so phan cong).
    /// </summary>
    public class RangedEnemyController : BuriedEnemyBase
    {
        ActionState patrol, engage, windup, recover;
        ProjectilePool pool;
        float nextShotTime;

        protected override IState InitialState => patrol;

        protected override void BuildStates(StateMachine machine)
        {
            if (config.projectilePrefab)
                pool = ProjectilePool.For(config.projectilePrefab, config.projectileBurstPrefab);

            patrol = new ActionState(enter: PlayWalk, fixedTick: _ => PatrolStep());
            engage = new ActionState(fixedTick: _ => KeepDistance());
            windup = new ActionState(
                enter: () => { Stop(); FacePlayer(); PlayAttack(); PlayWindupFx(true, config.shootWindup); },
                fixedTick: _ => Stop(),
                exit: () => PlayWindupFx(false));
            recover = new ActionState(enter: () => { Fire(); PlayIdle(); }, fixedTick: _ => Stop());

            machine.AddAnyTransition(hurt, () => ShouldStagger);
            machine.AddTransition(hurt, engage, () => HurtFinished);
            machine.AddTransition(patrol, engage, () => PlayerInRange(config.detectRange));
            machine.AddTransition(engage, patrol, () => !PlayerInRange(config.detectRange * 1.3f));
            machine.AddTransition(engage, windup, () => Time.time >= nextShotTime && Mathf.Abs(PlayerDx) <= config.preferredRange + 1.5f);
            machine.AddTransition(windup, recover, () => windup.Elapsed >= config.shootWindup);
            machine.AddTransition(recover, engage, () => recover.Elapsed >= config.shootRecover);
        }

        enum Move { Hold, Retreat, Approach }
        Move move;

        /// <summary>
        /// Lui khi nguoi choi qua gan, tien khi qua xa, con lai dung yen nhin nguoi choi.
        /// Co vung dem (bat dau / dung o hai nguong khac nhau) de khong quay dau lien tuc o ranh gioi.
        /// </summary>
        void KeepDistance()
        {
            float dist = Mathf.Abs(PlayerDx);
            float toward = Mathf.Sign(PlayerDx);
            switch (move)
            {
                case Move.Hold:
                    if (dist < config.retreatRange) move = Move.Retreat;
                    else if (dist > config.preferredRange + 1.5f) move = Move.Approach;
                    break;
                case Move.Retreat:
                    if (dist > config.retreatRange + 1.5f || IsBlocked(-toward)) move = Move.Hold;
                    break;
                case Move.Approach:
                    if (dist < config.preferredRange || IsBlocked(toward)) move = Move.Hold;
                    break;
            }
            if (move == Move.Hold)
            {
                FacePlayer();
                PlayIdle();
                Stop();
                return;
            }
            float dir = move == Move.Retreat ? -toward : toward;
            Face(dir);
            PlayWalk();
            SetHorizontalSpeed(dir * config.moveSpeed * (move == Move.Retreat ? 1.3f : 1f));
        }

        void Fire()
        {
            nextShotTime = Time.time + config.shootCooldown;
            if (pool == null || !Player) return;
            Vector2 muzzle = (Vector2)transform.position + new Vector2(config.muzzleOffset.x * FacingSign, config.muzzleOffset.y);
            Vector2 aim = (Vector2)Player.position + Vector2.up * 0.8f - muzzle;
            pool.Spawn(muzzle, aim.normalized * config.projectileSpeed).reaction = config.hitReaction;
        }

        protected override void OnResetState()
        {
            base.OnResetState();
            nextShotTime = 0f;
            move = Move.Hold;
        }
    }
}
