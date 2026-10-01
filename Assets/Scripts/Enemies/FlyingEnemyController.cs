using LuHanh.Combat;
using LuHanh.Core.FSM;
using LuHanh.Enemies.States.Flying;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Quai bay ban cau (doi): luon quanh cho xuat phat -> thay nguoi choi thi bay giu
    /// khoang cach, cu vai giay dung lai bao truoc roi ban mot qua cau nham thang.
    /// Khong chiu trong luc nhung van va vao tuong.
    /// </summary>
    public class FlyingEnemyController : EnemyBase
    {
        [SerializeField] FlyingEnemyConfig config;

        HoverState hover;
        CombatState combat;
        ShootState shoot;
        FlyingHurtState hurt;

        ProjectilePool pool;
        Transform visual;
        Vector3 visualBasePosition;
        int flyHash, attackHash;

        public FlyingEnemyConfig Config => config;
        protected override EnemyConfigBase BaseConfig => config;
        protected override IState InitialState => hover;

        /// <summary>Thoi gian con lai toi phat ban ke tiep (chi dem khi dang danh).</summary>
        public float ShotCooldown { get; set; }
        /// <summary>Dang bao truoc (rung hinh) truoc khi ban.</summary>
        public bool Telegraphing { get; set; }

        protected override void Awake()
        {
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<FlyingEnemyConfig>();
                Debug.LogWarning("FlyingEnemyController chua duoc gan config, dang dung bo so mac dinh.", this);
            }

            flyHash = Animator.StringToHash(config.flyState);
            attackHash = Animator.StringToHash(config.attackState);

            base.Awake();

            body.gravityScale = 0f;
            visual = sprite ? sprite.transform : null;
            if (visual) visualBasePosition = visual.localPosition;
            ShotCooldown = config.shootInterval * 0.5f;

            // Tao san dan ngay tu dau, luc ban khong phai cap phat gi.
            if (config.projectilePrefab)
                pool = ProjectilePool.For(config.projectilePrefab, config.projectileBurstPrefab, config.poolPrewarm);
        }

        protected override void BuildStates(StateMachine machine)
        {
            hover = new HoverState(this);
            combat = new CombatState(this);
            shoot = new ShootState(this);
            hurt = new FlyingHurtState(this);

            machine.AddAnyTransition(hurt, () => HurtRequested);
            machine.AddTransition(hover, combat, CanDetectPlayer);
            machine.AddTransition(combat, hover, () => combat.LostPlayer);
            machine.AddTransition(combat, shoot, () => ShotCooldown <= 0f && HasLineOfSight());
            machine.AddTransition(shoot, combat, () => shoot.Finished);
            machine.AddTransition(hurt, combat, () => hurt.Finished);
        }

        protected override void OnHurt(HitInfo hit) => hurt.Setup(hit.direction * hit.knockback);

        protected override void OnResetState()
        {
            ShotCooldown = config.shootInterval * 0.5f;
            Telegraphing = false;
            PlayFly();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (!visual) return;

            // Nhap nho + rung bao truoc chi la hinh anh, khong dung toi vat ly.
            float bob = Mathf.Sin(Time.time * config.bobFrequency * Mathf.PI * 2f) * config.bobAmplitude;
            Vector3 offset = new Vector3(0f, bob, 0f);
            if (Telegraphing) offset += (Vector3)(Random.insideUnitCircle * config.telegraphShake);
            visual.localPosition = visualBasePosition + offset;
        }

        // ---- Tien ich cho cac trang thai ----

        /// <summary>Bay toi <paramref name="target"/>, cham dan khi gan toi, doi huong muot theo gia toc.</summary>
        public void FlyTowards(Vector2 target, float maxSpeed, float deltaTime)
        {
            Vector2 toTarget = target - body.position;
            float distance = toTarget.magnitude;
            // Gan dich thi cham lai de khong vot qua roi quay lai.
            float speed = Mathf.Min(maxSpeed, distance * 2f);
            Vector2 desired = distance > 0.001f ? toTarget / distance * speed : Vector2.zero;
            body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, desired, config.acceleration * deltaTime);
        }

        public void Brake(float deltaTime) =>
            body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, Vector2.zero, config.acceleration * deltaTime);

        public Vector2 PlayerAimPoint => Player ? (Vector2)Player.position + Vector2.up * config.aimHeight : body.position;

        public bool HasLineOfSight()
        {
            if (!Player) return false;
            return !Physics2D.Linecast(body.position, PlayerAimPoint, config.groundLayers);
        }

        public bool CanDetectPlayer() =>
            PlayerDistanceSqr <= config.detectRadius * config.detectRadius && HasLineOfSight();

        public void FacePlayer()
        {
            if (!Player) return;
            float dx = Player.position.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.2f) Face(dx);
        }

        public void Fire()
        {
            if (!pool) return;
            Vector2 muzzle = body.position + new Vector2(config.muzzleOffset.x * FacingSign, config.muzzleOffset.y);
            Vector2 dir = (PlayerAimPoint - muzzle).normalized;
            pool.Spawn(muzzle, dir * config.projectileSpeed);
        }

        public void PlayFly() => PlayAnimHash(flyHash);
        public void PlayAttack() => PlayAnimHash(attackHash);

        void OnDrawGizmosSelected()
        {
            if (config == null) return;
            Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, config.detectRadius);
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.4f);
            Vector3 home = Application.isPlaying ? StartPosition : transform.position;
            Gizmos.DrawLine(home - Vector3.right * config.hoverRange, home + Vector3.right * config.hoverRange);
        }
    }
}
