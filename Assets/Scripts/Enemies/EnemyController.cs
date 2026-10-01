using LuHanh.Combat;
using LuHanh.Core.FSM;
using LuHanh.Enemies.States;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>Cac clip ma trang thai cua quai di bo co the yeu cau phat.</summary>
    public enum EnemyAnim { Idle, Walk, Attack }

    /// <summary>
    /// Quai di bo: tuan tra -> thay nguoi choi thi duoi -> mat dau thi quay lai.
    /// Phan chung (mau, nhay trang, hoi sinh, ngu) nam o <see cref="EnemyBase"/>;
    /// lop nay chi khai bao trang thai, luat chuyen va cach do tuong / mep vuc.
    /// </summary>
    public class EnemyController : EnemyBase
    {
        [SerializeField] EnemyConfig config;

        PatrolState patrol;
        ChaseState chase;
        HurtState hurt;
        int idleHash, walkHash, attackHash;

        public EnemyConfig Config => config;
        protected override EnemyConfigBase BaseConfig => config;
        protected override IState InitialState => patrol;

        protected override void Awake()
        {
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<EnemyConfig>();
                Debug.LogWarning("EnemyController chua duoc gan config, dang dung bo so mac dinh.", this);
            }

            idleHash = Animator.StringToHash(config.idleState);
            walkHash = Animator.StringToHash(config.walkState);
            attackHash = Animator.StringToHash(config.attackState);

            base.Awake();
        }

        protected override void BuildStates(StateMachine machine)
        {
            patrol = new PatrolState(this);
            chase = new ChaseState(this);
            hurt = new HurtState(this);

            machine.AddAnyTransition(hurt, () => HurtRequested);
            machine.AddTransition(patrol, chase, CanSeePlayer);
            machine.AddTransition(chase, patrol, () => chase.LostPlayer);
            // Vua bi chem nghia la nguoi choi o gan -> duoi luon.
            machine.AddTransition(hurt, chase, () => hurt.Finished);
        }

        protected override void OnHurt(HitInfo hit) => hurt.Setup(hit.direction * hit.knockback);

        // Dang o Patrol san thi SetState khong goi lai Enter, nen tu phat lai clip.
        protected override void OnResetState() => PlayAnim(EnemyAnim.Walk);

        // ---- Tien ich cho cac trang thai ----

        public void SetHorizontalSpeed(float vx)
        {
            var v = body.linearVelocity;
            v.x = vx;
            body.linearVelocity = v;
        }

        /// <summary>Nguoi choi o phia truoc, trong tam va khong chenh do cao qua nhieu.</summary>
        public bool CanSeePlayer()
        {
            if (!Player) return false;
            Vector2 d = Player.position - transform.position;
            if (Mathf.Abs(d.y) > config.detectHeight) return false;
            if (Mathf.Abs(d.x) > config.detectRange) return false;
            return d.x * FacingSign >= 0f;
        }

        public bool IsGrounded()
        {
            var b = bodyCollider.bounds;
            return Physics2D.Raycast(new Vector2(b.center.x, b.min.y + 0.05f), Vector2.down, 0.15f, config.groundLayers);
        }

        /// <summary>Phia truoc la tuong, hoac (khi dang dung tren dat) la mep vuc.</summary>
        public bool IsBlockedAhead()
        {
            var b = bodyCollider.bounds;
            float front = FacingRight ? b.max.x : b.min.x;

            if (Physics2D.Raycast(new Vector2(front, b.center.y), new Vector2(FacingSign, 0f),
                    config.wallCheckDistance, config.groundLayers))
                return true;

            if (!IsGrounded()) return false;
            var edgeOrigin = new Vector2(front + FacingSign * 0.1f, b.min.y + 0.05f);
            return !Physics2D.Raycast(edgeOrigin, Vector2.down, config.edgeCheckDepth, config.groundLayers);
        }

        public void PlayAnim(EnemyAnim anim) =>
            PlayAnimHash(anim == EnemyAnim.Walk ? walkHash : anim == EnemyAnim.Attack ? attackHash : idleHash);

        void OnDrawGizmosSelected()
        {
            var c = config;
            if (c == null) return;
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.5f);
            float side = Application.isPlaying ? FacingSign : 1f;
            Gizmos.DrawWireCube(transform.position + new Vector3(side * c.detectRange * 0.5f, 0f, 0f),
                new Vector3(c.detectRange, c.detectHeight * 2f, 0f));
        }
    }
}
