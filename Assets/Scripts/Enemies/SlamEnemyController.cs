using LuHanh.Combat;
using LuHanh.Core;
using LuHanh.Core.FSM;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Quai nang dap dat (Golem): tuan tra cham -> thay nguoi choi thi ap sat -> gong ro (slamWindup) ->
    /// dap: trung gan + rung camera + hai song xung kich chay doc mat dat ve hai phia -> hoi lau.
    /// </summary>
    public class SlamEnemyController : BuriedEnemyBase
    {
        [Tooltip("Bui bung ra luc dap (Emit).")]
        [SerializeField] ParticleSystem slamDust;
        [SerializeField] int slamDustCount = 30;
        [SerializeField] float slamShake = 0.3f;

        ActionState patrol, chase, windup, slam, recover;
        ProjectilePool wavePool;
        float nextSlamTime;

        protected override IState InitialState => patrol;

        protected override void BuildStates(StateMachine machine)
        {
            if (config.wavePrefab) wavePool = ProjectilePool.For(config.wavePrefab);

            patrol = new ActionState(enter: PlayWalk, fixedTick: _ => PatrolStep());
            chase = new ActionState(enter: PlayWalk, fixedTick: _ => ChaseStep());
            windup = new ActionState(
                enter: () => { Stop(); FacePlayer(); PlayAttack(); PlayWindupFx(true, config.slamWindup); },
                fixedTick: _ => Stop(),
                exit: () => PlayWindupFx(false));
            slam = new ActionState(enter: Slam, fixedTick: _ => Stop(), exit: () => SetAttackHitbox(false));
            recover = new ActionState(enter: PlayIdle, fixedTick: _ => Stop());

            machine.AddAnyTransition(hurt, () => ShouldStagger);
            machine.AddTransition(hurt, chase, () => HurtFinished);
            machine.AddTransition(patrol, chase, () => PlayerInRange(config.detectRange));
            machine.AddTransition(chase, patrol, () => !PlayerInRange(config.detectRange * 1.3f));
            machine.AddTransition(chase, windup, () => Time.time >= nextSlamTime && Mathf.Abs(PlayerDx) <= config.slamRange);
            machine.AddTransition(windup, slam, () => windup.Elapsed >= config.slamWindup);
            machine.AddTransition(slam, recover, () => slam.Elapsed >= config.slamActive);
            machine.AddTransition(recover, chase, () => recover.Elapsed >= config.slamRecover);
        }

        // Quai nang: dang gong / dap thi trung don van mat mau nhung khong bi ngat don.
        protected override bool CanStagger() => Machine.Current != windup && Machine.Current != slam;

        void ChaseStep()
        {
            float dir = Mathf.Sign(PlayerDx);
            Face(dir);
            if (Mathf.Abs(PlayerDx) < 0.5f || IsBlocked(dir)) { Stop(); PlayIdle(); return; }
            PlayWalk();
            SetHorizontalSpeed(dir * config.moveSpeed);
        }

        void Slam()
        {
            nextSlamTime = Time.time + config.slamCooldown;
            SetAttackHitbox(true);
            CameraShake.Kick(slamShake, 0.35f);
            var b = bodyCollider.bounds;
            if (slamDust)
            {
                slamDust.transform.position = new Vector3(b.center.x, b.min.y, 0f);
                slamDust.Emit(slamDustCount);
            }
            if (wavePool == null) return;
            float y = b.min.y + config.waveHeight;
            foreach (float dir in new[] { -1f, 1f })
                wavePool.Spawn(new Vector2(b.center.x + dir * b.extents.x, y), new Vector2(dir * config.waveSpeed, 0f)).reaction = config.hitReaction;
        }

        protected override void OnResetState()
        {
            base.OnResetState();
            nextSlamTime = 0f;
        }
    }
}
