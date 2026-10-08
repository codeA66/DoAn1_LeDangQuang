using LuHanh.Combat;
using LuHanh.Effects;
using LuHanh.Core.FSM;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Quai may dung yen. Player vao tam thi tu luc va ban ngang;
    /// khi het mau, loi than duoi Player va no lon hon truoc khi tat.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(BoxCollider2D))]
    public sealed class CinderHuskController : EnemyBase
    {
        [Tooltip("Config animation, mau va ban kinh no.")]
        [SerializeField] CinderHuskConfig config;
        [Tooltip("Bo lua va loi than tao san trong prefab.")]
        [SerializeField] CinderHuskExplosionFx effects;
        [Tooltip("Hai cau lua tao san, dung luan phien; khong Instantiate khi choi.")]
        [SerializeField] CinderHuskFireball[] fireballs;

        IdleState idle;
        HurtState hurt;
        AttackState attack;
        DeathState death;
        readonly Collider2D[] overlapBuffer = new Collider2D[8];
        Vector3 bodyArtScale = Vector3.one;
        Vector3 effectsStartPosition;
        float deathElapsed;
        bool coreStarted, coreDetonated;
        float initialGravityScale;
        bool deathStarted;
        Collider2D playerBody;

        protected override EnemyConfigBase BaseConfig => config;
        protected override IState InitialState => idle;

        protected override void Awake()
        {
            if (config == null)
            {
                Debug.LogError("CinderHuskController thieu CinderHuskConfig.", this);
                enabled = false;
                return;
            }

            if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
            if (bodyCollider == null) bodyCollider = GetComponent<BoxCollider2D>();
            initialGravityScale = GetComponent<Rigidbody2D>().gravityScale;
            base.Awake();
            if (sprite) bodyArtScale = sprite.transform.localScale;
            if (effects) effectsStartPosition = effects.transform.localPosition;
        }

        protected override void BuildStates(StateMachine machine)
        {
            idle = new IdleState(this);
            hurt = new HurtState(this);
            attack = new AttackState(this);
            death = new DeathState(this);

            // Hurt khong kich hoat ban va khong reset lan tu luc dang dien ra.
            machine.AddAnyTransition(hurt, () => HurtRequested && !deathStarted &&
                machine.Current != hurt && machine.Current != attack);
            machine.AddTransition(hurt, idle, () => hurt.Finished);
            machine.AddTransition(idle, attack, CanShootPlayer);
            machine.AddTransition(attack, idle, () => attack.Finished);
        }

        protected override void Start()
        {
            base.Start();
            if (Player) playerBody = Player.GetComponent<Collider2D>();
            FacePlayer();
        }

        bool CanShootPlayer()
        {
            if (deathStarted || !Player || !Player.gameObject.activeInHierarchy) return false;
            if (playerBody && !playerBody.enabled) return false;
            if (playerBody && playerBody.attachedRigidbody && !playerBody.attachedRigidbody.simulated) return false;
            Vector2 origin = FireballOrigin;
            Vector2 target = playerBody ? (Vector2)playerBody.bounds.center : (Vector2)Player.position;
            Vector2 delta = target - origin;
            if (delta.sqrMagnitude > config.AttackRange * config.AttackRange) return false;
            float targetHalfHeight = playerBody ? playerBody.bounds.extents.y : 0f;
            if (Mathf.Abs(delta.y) > config.FireballRadius + targetHalfHeight) return false;
            if (Physics2D.Linecast(origin, target, config.groundLayers).collider) return false;
            if (fireballs != null)
                foreach (var ball in fireballs) if (ball && ball.Available) return true;
            return false;
        }

        protected override void OnHurt(HitInfo hit)
        {
            // Quai nay nhan sat thuong nhung khong nhan luc day tu don chem.
            Body.linearVelocity = new Vector2(0f, Body.linearVelocity.y);
            FacePlayer();
        }

        void FacePlayer()
        {
            if (Player) Face(Player.position.x - transform.position.x);
        }

        protected override bool OnDeath(HitInfo hit)
        {
            if (deathStarted) return true;
            deathStarted = true;
            deathElapsed = 0f;
            coreStarted = coreDetonated = false;
            Machine.SetState(death);
            return true;
        }

        protected override void OnResetState()
        {
            deathStarted = false;
            if (fireballs != null) foreach (var ball in fireballs) if (ball) ball.Cancel();
            if (Body)
            {
                Body.gravityScale = initialGravityScale;
                Body.linearVelocity = Vector2.zero;
            }
            if (bodyCollider) { bodyCollider.enabled = true; bodyCollider.isTrigger = false; }
            if (sprite) sprite.enabled = true;
            if (effects) { effects.ResetEffects(); effects.transform.localPosition = effectsStartPosition; }
            if (sprite) sprite.transform.localScale = bodyArtScale;
        }

        void ShowIdle(float elapsed)
        {
            var frames = config.IdleFrames;
            if (frames == null || frames.Length == 0) return;

            int count = frames.Length;
            int phaseLength = count <= 1 ? 1 : count * 2 - 2;
            int phase = Mathf.FloorToInt(elapsed * config.IdleFramesPerSecond) % phaseLength;
            int index = phase < count ? phase : phaseLength - phase;
            SetFrame(frames[index]);
        }

        void ShowHurt(float elapsed)
        {
            ShowTimedFrame(config.HurtFrames, elapsed, config.HurtDuration, 1f);
        }

        void ShowAttack(float elapsed)
        {
            var frames = config.AttackFrames;
            if (frames == null || frames.Length == 0) return;
            if (elapsed < config.AttackChargeDuration)
            {
                int count = Mathf.Max(1, frames.Length - 3);
                SetFrame(frames[Mathf.Min(count - 1, Mathf.FloorToInt(
                    elapsed / Mathf.Max(0.001f, config.AttackChargeDuration) * count))]);
            }
            else SetTimedTail(frames, elapsed - config.AttackChargeDuration,
                config.FireBurstDuration + config.AttackRecoveryDuration);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            // Countdown survives the base enemy sleeping off camera.
            if (!deathStarted) return;
            deathElapsed += Time.deltaTime;
            if (!coreStarted)
            {
                if (deathElapsed < config.DeathBurstDelay)
                {
                    var frames = config.DieFrames;
                    if (frames != null && frames.Length > 0)
                    {
                        int count = Mathf.Max(1, frames.Length - 3);
                        SetFrame(frames[Mathf.Min(count - 1, Mathf.FloorToInt(
                            deathElapsed / Mathf.Max(0.001f, config.DeathBurstDelay) * count))]);
                    }
                    return;
                }
                coreStarted = true;
                if (sprite) sprite.enabled = false;
                if (flash) flash.enabled = false;
                if (effects)
                {
                    var centre = CurrentBlastCentre;
                    if (bodyCollider) centre.y = bodyCollider.bounds.min.y + config.CoreCollisionRadius + 0.06f;
                    effects.transform.position = centre;
                    effects.BeginCore();
                }
            }
            float fuseElapsed = deathElapsed - config.DeathBurstDelay;
            if (fuseElapsed >= config.CoreFuseDuration)
            {
                if (!coreDetonated)
                {
                    coreDetonated = true;
                    Vector2 centre = effects ? (Vector2)effects.transform.position : CurrentBlastCentre;
                    if (effects) effects.PlayBlast(config.DeathBlastRadius);
                    Detonate(config.DeathBlastRadius, centre);
                }
                // A hit can cause a synchronous respawn/reset.
                if (deathStarted && fuseElapsed >= config.CoreFuseDuration + config.DeathCleanupTime)
                    FinishDeath();
                return;
            }
            Vector2 velocity = Vector2.zero;
            if (effects && Player && Time.deltaTime > 0f)
            {
                Vector2 centre = effects.transform.position;
                Vector2 target = playerBody ? (Vector2)playerBody.bounds.center : (Vector2)Player.position;
                Vector2 delta = target - centre;
                float distance = Mathf.Min(delta.magnitude, config.CoreChaseSpeed * Time.deltaTime);
                Vector2 direction = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector2.zero;
                // Loi khong co collider va khong quet vat can: duoi xuyen tuong/san.
                Vector2 movement = direction * distance;
                effects.transform.position += (Vector3)movement;
                velocity = effects.transform.InverseTransformVector(movement / Time.deltaTime);
            }
            if (effects) effects.UpdateCoreWarning(
                fuseElapsed / config.CoreFuseDuration, Time.deltaTime, velocity);
        }

        void ShowTimedFrame(Sprite[] frames, float elapsed, float duration, float firstFrameScale)
        {
            if (frames == null || frames.Length == 0) return;
            int index = Mathf.Min(frames.Length - 1,
                Mathf.FloorToInt(elapsed / Mathf.Max(0.001f, duration) * frames.Length));
            SetFrame(frames[index], index == 0 ? firstFrameScale : 1f);
        }

        void SetTimedTail(Sprite[] frames, float elapsed, float duration)
        {
            int tailStart = Mathf.Min(4, frames.Length - 1);
            int tailCount = frames.Length - tailStart;
            int tailIndex = duration <= 0f ? tailCount - 1 :
                Mathf.Min(tailCount - 1, Mathf.FloorToInt(elapsed / duration * tailCount));
            SetFrame(frames[tailStart + tailIndex], 1f);
        }

        void SetFrame(Sprite frame, float scale = 1f)
        {
            if (!sprite || !frame) return;
            if (sprite.sprite != frame) sprite.sprite = frame;
            sprite.transform.localScale = bodyArtScale * scale;
        }

        void Detonate(float radius, Vector2 centre)
        {
            if (!Player) return;
            int playerMask = 1 << Player.gameObject.layer;
            var contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(playerMask);
            contactFilter.useTriggers = true;
            int count = Physics2D.OverlapCircle(centre, radius, contactFilter, overlapBuffer);
            if (count <= 0) return;

            var damageable = Player.GetComponent<IDamageable>();
            if (damageable == null) damageable = Player.GetComponentInChildren<IDamageable>();
            if (damageable == null) return;

            for (int i = 0; i < count; i++)
            {
                var hitCollider = overlapBuffer[i];
                if (!hitCollider || (hitCollider.transform != Player && !hitCollider.transform.IsChildOf(Player)))
                    continue;

                Vector2 direction = (Vector2)Player.position - centre;
                if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;
                else direction.Normalize();

                damageable.TakeHit(new HitInfo
                {
                    damage = config.BlastDamage,
                    direction = direction,
                    knockback = 0f,
                    source = gameObject,
                    reaction = config.hitReaction
                });
                break;
            }
        }

        void FinishDeath() => gameObject.SetActive(false);

        sealed class IdleState : IState
        {
            readonly CinderHuskController owner;
            float elapsed;
            public IdleState(CinderHuskController owner) => this.owner = owner;
            public void Enter() { elapsed = 0f; owner.ShowIdle(elapsed); }
            public void Tick(float deltaTime) { elapsed += deltaTime; owner.FacePlayer(); owner.ShowIdle(elapsed); }
            public void FixedTick(float fixedDeltaTime) { }
            public void Exit() { }
        }

        sealed class HurtState : IState
        {
            readonly CinderHuskController owner;
            float elapsed;
            public bool Finished => elapsed >= owner.config.HurtDuration;
            public HurtState(CinderHuskController owner) => this.owner = owner;
            public void Enter()
            {
                elapsed = 0f;
                owner.Body.linearVelocity = new Vector2(0f, owner.Body.linearVelocity.y);
                owner.ShowHurt(elapsed);
            }
            public void Tick(float deltaTime) { elapsed += deltaTime; owner.FacePlayer(); owner.ShowHurt(elapsed); }
            public void FixedTick(float fixedDeltaTime) { }
            public void Exit() { }
        }

        sealed class AttackState : IState
        {
            readonly CinderHuskController owner;
            float elapsed;
            bool detonated;
            public bool Finished => elapsed >= owner.config.AttackChargeDuration +
                owner.config.FireBurstDuration + owner.config.AttackRecoveryDuration;
            public AttackState(CinderHuskController owner) => this.owner = owner;
            public void Enter()
            {
                elapsed = 0f;
                detonated = false;
                owner.ShowAttack(elapsed);
                if (owner.effects) owner.effects.BeginCharge();
            }
            public void Tick(float deltaTime)
            {
                elapsed += deltaTime;
                owner.FacePlayer();
                owner.ShowAttack(elapsed);
                if (elapsed >= owner.config.AttackChargeDuration && !detonated)
                {
                    detonated = true;
                    if (owner.effects) owner.effects.StopCharge();
                    if (owner.fireballs != null)
                        foreach (var ball in owner.fireballs)
                            if (ball && ball.Available)
                            {
                                ball.Launch(owner.config, owner.Player, owner.gameObject,
                                    owner.FireballOrigin, owner.FacingSign);
                                break;
                            }
                }
            }
            public void FixedTick(float fixedDeltaTime) { }
            public void Exit() { if (owner.effects) owner.effects.StopCharge(); }
        }

        sealed class DeathState : IState
        {
            readonly CinderHuskController owner;
            public DeathState(CinderHuskController owner) => this.owner = owner;
            public void Enter()
            {
                owner.Body.linearVelocity = Vector2.zero;
                owner.Body.gravityScale = 0f;
                if (owner.bodyCollider) owner.bodyCollider.isTrigger = true;
                if (owner.effects) owner.effects.ResetEffects();
            }
            public void Tick(float deltaTime) { }
            public void FixedTick(float fixedDeltaTime) { }
            public void Exit() { }
        }

        Vector2 CurrentBlastCentre => bodyCollider ? bodyCollider.bounds.center : transform.position;
        Vector2 FireballOrigin
        {
            get
            {
                var centre = CurrentBlastCentre;
                if (bodyCollider) centre.y = bodyCollider.bounds.min.y + config.FireballRadius + 0.06f;
                return centre;
            }
        }
    }
}
