using LuHanh.Combat;
using LuHanh.Core.FSM;
using LuHanh.Effects;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Phan chung cua quai long dat co ky nang rieng: doc config, di bo, do tuong/mep vuc,
    /// bat tat hitbox can can, khoi bao truoc va trang thai bi danh (bat lui ngan roi tiep tuc).
    /// Lop con chi khai bao cac trang thai ky nang.
    /// </summary>
    public abstract class BuriedEnemyBase : EnemyBase
    {
        [SerializeField] protected BuriedEnemyConfig config;
        [Tooltip("Vung gay sat thuong khi tan cong (object con Hitbox). Chi bat trong luc ra don.")]
        [SerializeField] protected ContactDamage attackHitbox;
        [Tooltip("Khoi / anh sang bao truoc don (object con WindupFx).")]
        [SerializeField] protected FrameAnimation windupFx;

        protected ActionState hurt;
        Vector2 hurtVelocity;
        int idleHash, walkHash, attackHash, emergeHash, burrowHash;

        public BuriedEnemyConfig Config => config;
        protected override EnemyConfigBase BaseConfig => config;

        protected override void Awake()
        {
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BuriedEnemyConfig>();
                Debug.LogWarning($"{GetType().Name} chua duoc gan config, dang dung bo so mac dinh.", this);
            }
            idleHash = Animator.StringToHash(config.idleState);
            walkHash = Animator.StringToHash(config.walkState);
            attackHash = Animator.StringToHash(config.attackState);
            emergeHash = Animator.StringToHash(config.emergeState);
            burrowHash = Animator.StringToHash(config.burrowState);
            if (attackHitbox == null) attackHitbox = GetComponentInChildren<ContactDamage>(true);
            if (attackHitbox) attackHitbox.reaction = config.hitReaction;
            SetAttackHitbox(false);

            hurt = new ActionState(
                enter: () => { SetAttackHitbox(false); PlayWindupFx(false); SetVelocity(hurtVelocity); PlayHurtAnim(); },
                // Ham dan luc day lui, khong truot mai.
                fixedTick: dt => SetHorizontalSpeed(Mathf.MoveTowards(body.linearVelocity.x, 0f, 30f * dt)));
            base.Awake();
        }

        protected override void OnHurt(HitInfo hit) =>
            hurtVelocity = new Vector2(hit.direction.x * hit.knockback, 0f);

        protected bool HurtFinished => hurt.Elapsed >= config.hurtTime;

        /// <summary>Vua trung don va duoc phep khung lai. Quai nang tra false luc dang ra don (khong bi ngat don).</summary>
        protected bool ShouldStagger => HurtRequested && CanStagger();
        protected virtual bool CanStagger() => true;

        // ---- Tien ich ----

        protected void SetVelocity(Vector2 v) => body.linearVelocity = v;
        protected void SetHorizontalSpeed(float vx) => body.linearVelocity = new Vector2(vx, body.linearVelocity.y);
        protected void Stop() => SetHorizontalSpeed(0f);

        protected float PlayerDx => Player ? Player.position.x - transform.position.x : 0f;
        protected float PlayerDy => Player ? Player.position.y - transform.position.y : 0f;

        /// <summary>Nguoi choi trong tam phat hien (ca hai phia) va khong chenh do cao qua nhieu.</summary>
        protected bool PlayerInRange(float range) =>
            Player && Player.gameObject.activeInHierarchy &&
            Mathf.Abs(PlayerDx) <= range && Mathf.Abs(PlayerDy) <= config.detectHeight;

        protected void FacePlayer() => Face(PlayerDx);

        protected bool IsGrounded()
        {
            var b = bodyCollider.bounds;
            return Physics2D.Raycast(new Vector2(b.center.x, b.min.y + 0.05f), Vector2.down, 0.2f, config.groundLayers);
        }

        /// <summary>Phia <paramref name="dir"/> la tuong hoac mep vuc.</summary>
        protected bool IsBlocked(float dir)
        {
            var b = bodyCollider.bounds;
            float front = dir > 0f ? b.max.x : b.min.x;
            if (Physics2D.Raycast(new Vector2(front, b.center.y), new Vector2(Mathf.Sign(dir), 0f), config.wallCheckDistance, config.groundLayers))
                return true;
            var edge = new Vector2(front + Mathf.Sign(dir) * 0.1f, b.min.y + 0.05f);
            return !Physics2D.Raycast(edge, Vector2.down, config.edgeCheckDepth, config.groundLayers);
        }

        /// <summary>Tuan tra qua lai, quay dau o tuong / mep vuc.</summary>
        protected void PatrolStep()
        {
            if (IsGrounded() && IsBlocked(FacingSign)) Face(-FacingSign);
            SetHorizontalSpeed(FacingSign * config.moveSpeed * SpeedScale);
        }

        protected void SetAttackHitbox(bool on) { if (attackHitbox) attackHitbox.enabled = on; }

        protected void PlayWindupFx(bool on, float duration = 0.5f)
        {
            if (!windupFx) return;
            if (on) windupFx.Play(duration); else windupFx.Stop();
        }

        /// <summary>Animation luc bi danh. Mac dinh dung Idle; quai co clip Hurt rieng thi ghi de.</summary>
        protected virtual void PlayHurtAnim() => PlayIdle();
        protected void PlayIdle() => PlayAnimHash(idleHash);
        protected void PlayWalk() => PlayAnimHash(walkHash);
        protected void PlayAttack() => PlayAnimHash(attackHash);
        protected void PlayEmerge() => PlayAnimHash(emergeHash);
        protected void PlayBurrow() => PlayAnimHash(burrowHash);

        protected override void OnResetState()
        {
            SetAttackHitbox(false);
            PlayWindupFx(false);
            PlayWalk();
        }
    }
}
