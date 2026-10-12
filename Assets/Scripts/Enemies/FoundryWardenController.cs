using LuHanh.Combat;
using LuHanh.Core.FSM;
using LuHanh.Effects;
using LuHanh.Enemies.States;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>Boss bốn đòn tuần tự, có húc ngang; dùng FSM và vòng đời EnemyBase hiện có.</summary>
    public sealed class FoundryWardenController : EnemyBase, IDamageable
    {
        [Tooltip("Cấu hình riêng của boss.")][SerializeField] FoundryWardenConfig config;
        [Tooltip("Các hiệu ứng đặt sẵn trong prefab, tái sử dụng mỗi đòn.")]
        [SerializeField] FoundryWardenFx effects;
        [Tooltip("Cục đá và VFX đặt sẵn trong prefab boss, tái sử dụng mỗi lần ném.")]
        [SerializeField] FoundryWardenRock rock;
        WardenIdleState idle;
        WardenAttackState attack;
        WardenHurtState hurt;
        WardenDeathState death;
        Collider2D playerBody;
        IDamageable target;
        bool deathStarted, hitThisAttack;
        int attackIndex, resetVersion;
        float attackSign;
        Vector2 attackFoot;
        readonly float[] waveTravel = new float[2];
        readonly bool[] waveStopped = new bool[2];
        float gravityAtStart;
        float currentWidthScale = 1f;
        float nextAttackTime;
        float shieldCycleStart;
        float blockedShieldTime = float.NegativeInfinity;
        bool ramStarted, ramStopped;
        float ramTravel, ramStopTime;
        bool rockLaunched;
        public float AttackActiveDuration => CurrentAttack == WardenAttack.Ram && ramStopped ?
            ramStopTime : config.ActiveDuration(CurrentAttack);

        public FoundryWardenConfig Config => config;
        public WardenAttack CurrentAttack { get; private set; }
        public bool RestReady => Time.time >= nextAttackTime;
        float ShieldCycleTime => Mathf.Repeat(Time.time - shieldCycleStart,
            Mathf.Max(0.01f, config.ShieldDuration + config.VulnerableDuration));
        public bool IsInvulnerable => config && config.ShieldCycleEnabled && !deathStarted &&
            ShieldCycleTime < config.ShieldDuration;
        protected override EnemyConfigBase BaseConfig => config;
        protected override IState InitialState => idle;

        protected override void OnEnable()
        {
            base.OnEnable();
            shieldCycleStart = Time.time;
            blockedShieldTime = float.NegativeInfinity;
        }

        // Reimplement IDamageable riêng: EnemyBase.TakeHit không virtual.
        // PlayerAttack hiện truy cập mục tiêu qua interface; không sửa lớp dùng chung.
        public new void TakeHit(HitInfo hit)
        {
            if (deathStarted) return;
            if (IsInvulnerable)
            {
                blockedShieldTime = Time.time;
                return;
            }
            base.TakeHit(hit);
        }
        void IDamageable.TakeHit(HitInfo hit) => TakeHit(hit);

        protected override void Awake()
        {
            if (!config || !bodyCollider || !sprite)
            {
                Debug.LogError("Foundry Warden thiếu Config, Body Collider hoặc Visual.", this);
                enabled = false;
                return;
            }
            gravityAtStart = GetComponent<Rigidbody2D>().gravityScale;
            nextAttackTime = Time.time + config.RestDuration;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            if (Player)
            {
                playerBody = Player.GetComponent<Collider2D>();
                target = Player.GetComponent<IDamageable>();
            }
            FacePlayer();
        }

        protected override void BuildStates(StateMachine machine)
        {
            idle = new WardenIdleState(this);
            attack = new WardenAttackState(this);
            hurt = new WardenHurtState(this);
            death = new WardenDeathState(this);
            machine.AddTransition(idle, attack, () => idle.Ready && CanAttack());
            machine.AddTransition(idle, hurt, () => HurtRequested && !deathStarted);
            machine.AddTransition(hurt, idle, () => hurt.Finished);
            machine.AddTransition(attack, idle, () => attack.Finished);
        }

        bool CanAttack() => !deathStarted && Player && Player.gameObject.activeInHierarchy &&
            playerBody && playerBody.enabled && (!playerBody.attachedRigidbody || playerBody.attachedRigidbody.simulated) &&
            PlayerDistanceSqr <= config.DetectRange * config.DetectRange;

        protected override void OnHurt(HitInfo hit) => HoldPosition();

        protected override bool OnDeath(HitInfo hit)
        {
            if (deathStarted) return true;
            deathStarted = true;
            if (rock) rock.Clear();
            resetVersion++;
            body.linearVelocity = Vector2.zero;
            body.gravityScale = 0f;
            bodyCollider.enabled = false;
            if (flash) flash.enabled = false;
            Machine.SetState(death);
            return true;
        }

        protected override void OnResetState()
        {
            resetVersion++;
            deathStarted = false;
            hitThisAttack = false;
            attackIndex = 0;
            if (rock) rock.Clear();
            ramStarted = ramStopped = false;
            ramTravel = 0f;
            nextAttackTime = Time.time + config.RestDuration;
            bodyCollider.enabled = true;
            ReapplyIgnorePlayer(); // bat lai collider lam mat IgnoreCollision
            shieldCycleStart = Time.time;
            blockedShieldTime = float.NegativeInfinity;
            body.gravityScale = gravityAtStart;
            body.simulated = true;
            idle.Enter();
            ClearEffects();
        }

        void OnDisable() { ClearEffects(); if (rock) rock.Clear(); }
        public void HoldPosition()
        {
            if (body) body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        }
        /// <summary>Hoan don dau tien them <paramref name="seconds"/> giay (luc xuat hien, dang hien ten boss).</summary>
        public void DelayAttacks(float seconds) => nextAttackTime = Time.time + seconds;
        public void FacePlayer() { if (Player) Face(Player.position.x - transform.position.x); }
        public void ClearEffects() { if (effects) effects.Clear(); }
        public void EndAttack()
        {
            HoldPosition();
            ClearEffects();
            if (!deathStarted) nextAttackTime = Time.time + config.RestDuration;
        }

        void Show(FoundryWardenConfig.AnimationSet set, int index)
        {
            if (set == null || set.frames == null || set.frames.Length == 0) return;
            sprite.sprite = set.frames[Mathf.Clamp(index, 0, set.frames.Length - 1)];
            currentWidthScale = set.widthScale;
            sprite.transform.localScale = new Vector3(currentWidthScale, 1f, 1f);
        }
        protected override void LateUpdate()
        {
            base.LateUpdate();
            // EnemyBase lưu scale Idle để squash; boss không squash và mỗi bộ ảnh có tỷ lệ đã đo riêng.
            if (sprite) sprite.transform.localScale = new Vector3(currentWidthScale, 1f, 1f);
            if (effects && bodyCollider)
                effects.DrawShield(IsInvulnerable, bodyCollider.bounds,
                    IsInvulnerable ? ShieldCycleTime / config.ShieldDuration : 0f,
                    Mathf.Clamp01(1f - (Time.time - blockedShieldTime) / Mathf.Max(0.01f, config.flashTime)));
        }
        void ShowTimed(FoundryWardenConfig.AnimationSet set, float elapsed, float duration)
        {
            if (set?.frames == null) return;
            Show(set, Mathf.FloorToInt(Mathf.Clamp01(elapsed / duration) * set.frames.Length));
        }
        public void ShowIdle(float elapsed)
        {
            int count = config.Idle?.frames?.Length ?? 0;
            if (count > 0) Show(config.Idle, Mathf.FloorToInt(elapsed * config.IdleFps) % count);
        }
        public void ShowHurt(float elapsed) => ShowTimed(config.Hurt, elapsed, config.HurtDuration);
        public void ShowDeath(float elapsed) => ShowTimed(config.Die, elapsed, config.DeathDuration);

        public void BeginAttack()
        {
            FacePlayer();
            attackSign = FacingSign;
            attackFoot = new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y);
            CurrentAttack = (WardenAttack)attackIndex;
            attackIndex = (attackIndex + 1) % 4;
            hitThisAttack = false;
            rockLaunched = false;
            ramStarted = ramStopped = false;
            ramTravel = 0f;
            ramStopTime = config.ActiveDuration(CurrentAttack);
            for (int i = 0; i < 2; i++) { waveTravel[i] = 0f; waveStopped[i] = false; }
            DrawAttack(0f);
        }

        public Rect MeleeRect => new((Vector2)bodyCollider.bounds.min, (Vector2)bodyCollider.bounds.size);

        public void DrawAttack(float elapsed)
        {
            float active = AttackActiveDuration;
            float t = elapsed - config.ChargeDuration;
            int frame;
            Vector2? source = null;
            if (config.SynchronizedAttacks)
            {
                var clip = config.Attack(CurrentAttack);
                var timing = config.Timing(CurrentAttack);
                int count = clip?.frames?.Length ?? 0;
                if (count == 0) return;
                int impact = Mathf.Clamp(timing.impactFrame, 1, count - 1);
                int recover = Mathf.Clamp(timing.recoveryFrame, impact + 1, count);
                float motion = CurrentAttack == WardenAttack.Ram ? active : config.BodyMotionDuration(CurrentAttack);
                if (t < 0f) frame = Mathf.Min(impact - 1, Mathf.FloorToInt(elapsed / config.ChargeDuration * impact));
                else if (t < motion)
                    frame = impact + Mathf.Min(recover - impact - 1, Mathf.FloorToInt(t / motion * (recover - impact)));
                else frame = recover + Mathf.Min(count - recover - 1,
                    Mathf.FloorToInt((t - motion) / Mathf.Max(0.001f, config.RecoveryDuration) * (count - recover)));
                if (t >= motion + config.RecoveryDuration) ShowIdle(t - motion - config.RecoveryDuration);
                else Show(clip, frame);
                source = transform.TransformPoint(new Vector3(timing.strikeSource.x, timing.strikeSource.y, 0f));
            }
            else
            {
                if (t < 0f) frame = Mathf.Min(4, Mathf.FloorToInt(elapsed / config.ChargeDuration * 5f));
                else if (t < active) frame = 5 + Mathf.Min(2, Mathf.FloorToInt(t / active * 3f));
                else frame = 8 + Mathf.Min(1, Mathf.FloorToInt((t - active) / Mathf.Max(0.001f, config.RecoveryDuration) * 2f));
                Show(config.Attack(CurrentAttack), frame);
            }
            if (!effects) return;
            Vector2 foot = CurrentAttack == WardenAttack.Ram ?
                new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y) : attackFoot;
            if (t < 0f) effects.Charge(CurrentAttack, foot, MeleeRect, config, elapsed / config.ChargeDuration);
            else if (t < active) effects.Active(CurrentAttack, foot, MeleeRect, config,
                t / Mathf.Max(0.001f, active), waveTravel, waveStopped, source,
                CurrentAttack == WardenAttack.Ram ? attackSign : 0f);
            else effects.Finish();
        }

        public void StepAttack(float elapsed, float deltaTime)
        {
            if (deathStarted) return;
            float t = elapsed - config.ChargeDuration;
            if (t < 0f || t >= AttackActiveDuration) return;
            // Cùng mốc elapsed cho hình/VFX và sát thương, kể cả FixedUpdate đến trước Update.
            if (config.SynchronizedAttacks) DrawAttack(elapsed);
            int version = resetVersion;
            switch (CurrentAttack)
            {
                case WardenAttack.RockThrow:
                    if (!rockLaunched && rock && playerBody)
                    {
                        rockLaunched = true;
                        FacePlayer();
                        Vector2 point = transform.TransformPoint(config.Timing(CurrentAttack).strikeSource);
                        rock.Launch(config, point, playerBody, target, gameObject);
                    }
                    break;
                case WardenAttack.Ram:
                    StepRam(t, deltaTime);
                    break;
                case WardenAttack.DoubleSlam:
                    for (int i = 0; i < 2; i++)
                    {
                        if (waveStopped[i]) continue;
                        float sign = i == 0 ? -1f : 1f;
                        float old = waveTravel[i];
                        float next = Mathf.Min(config.WaveDistance, old + config.WaveSpeed * deltaTime);
                        Vector2 origin = attackFoot + new Vector2(sign * old, config.WaveSize.y * 0.5f + 0.02f);
                        var wall = Physics2D.BoxCast(origin, config.WaveSize * 0.96f, 0f,
                            new Vector2(sign, 0f), next - old, config.groundLayers);
                        if (wall.collider) { waveStopped[i] = true; continue; }
                        Vector2 nextFoot = attackFoot + new Vector2(sign * next, 0f);
                        var floor = Physics2D.Raycast(nextFoot + Vector2.up * 0.2f, Vector2.down, 0.4f, config.groundLayers);
                        if (!floor.collider) { waveStopped[i] = true; continue; }
                        waveTravel[i] = next;
                        float x = attackFoot.x + sign * (old + next) * 0.5f;
                        Vector2 size = new(config.WaveSize.x + next - old, config.WaveSize.y);
                        DamageRect(new Rect(new Vector2(x - size.x * 0.5f, attackFoot.y), size));
                        if (version != resetVersion) return;
                    }
                    break;
                case WardenAttack.Vent:
                    for (int i = 0; i < config.VentOffsets.Length; i++)
                    {
                        float localTime = t - config.VentDelay(i);
                        if (localTime < 0f || localTime >= config.VentDuration) continue;
                        float offset = config.VentOffsets[i];
                        DamageRect(new Rect(attackFoot + new Vector2(offset - config.VentSize.x * 0.5f, 0f), config.VentSize));
                        if (version != resetVersion) return;
                    }
                    break;
            }
        }

        void StepRam(float activeElapsed, float deltaTime)
        {
            if (!ramStarted)
            {
                ramStarted = true;
                FacePlayer(); // Chốt hướng sau tụ lực; không bẻ hướng giữa cú húc.
                attackSign = FacingSign;
            }
            Bounds bounds = bodyCollider.bounds;
            float move = Mathf.Min(config.RamSpeed * deltaTime, config.RamDistance - ramTravel);
            Vector2 direction = new(attackSign, 0f);
            // Kẹp quãng dịch trước tường, bỏ mặt sàn khỏi box dò ngang.
            var wall = Physics2D.BoxCast(bounds.center,
                new Vector2(bounds.size.x - 0.04f, bounds.size.y - 0.04f),
                0f, direction, move + 0.02f, config.groundLayers);
            if (wall.collider) move = Mathf.Min(move, Mathf.Max(0f, wall.distance - 0.02f));
            float front = bounds.center.x + attackSign * bounds.extents.x;
            float safe = 0f;
            // Dò mặt sàn từ mép trước theo đoạn nhỏ để không vượt một khe hẹp.
            for (float d = 0f; d < move;)
            {
                d = Mathf.Min(move, d + 0.05f);
                var floor = Physics2D.Raycast(new Vector2(front + attackSign * d, bounds.min.y + 0.2f),
                    Vector2.down, 0.4f, config.groundLayers);
                if (!floor.collider) break;
                safe = d;
            }
            bool obstacle = safe < config.RamSpeed * deltaTime - 0.001f;
            move = safe;
            Vector2 shift = direction * move;
            int version = resetVersion;
            DamageRect(new Rect(new Vector2(bounds.min.x + Mathf.Min(0f, shift.x), bounds.min.y),
                new Vector2(bounds.size.x + move, bounds.size.y)));
            if (version != resetVersion || deathStarted) return;
            body.MovePosition(body.position + shift);
            ramTravel += move;
            if (obstacle || ramTravel >= config.RamDistance - 0.001f)
            {
                ramStopped = true;
                ramStopTime = Mathf.Min(config.ActiveDuration(CurrentAttack), activeElapsed + deltaTime);
                HoldPosition();
            }
        }

        void DamageRect(Rect rect)
        {
            if (hitThisAttack || target == null || !playerBody || !playerBody.enabled ||
                (playerBody.attachedRigidbody && !playerBody.attachedRigidbody.simulated)) return;
            var area = new Bounds(new Vector3(rect.center.x, rect.center.y, playerBody.bounds.center.z),
                new Vector3(rect.width, rect.height, 2f));
            if (!area.Intersects(playerBody.bounds)) return;
            hitThisAttack = true;
            target.TakeHit(new HitInfo { damage = config.AttackDamage, source = gameObject,
                direction = new Vector2(playerBody.bounds.center.x >= attackFoot.x ? 1f : -1f, 0f),
                reaction = config.hitReaction, knockback = 0f });
        }
    }
}
