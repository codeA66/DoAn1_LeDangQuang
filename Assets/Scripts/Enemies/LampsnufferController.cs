using LuHanh.Combat;
using LuHanh.Core.FSM;
using LuHanh.Effects;
using LuHanh.Level;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Ke nuot den (Lampsnuffer): bay luon quanh cho dat. Nguoi choi o cung phong thi cu vai giay khung lai hut sang
    /// (loi do mo dan - bao truoc): moi lan hut xong anh sang chung cua man giam them (xem <see cref="AmbientDarkness"/>)
    /// va Dive nhanh them. Thay nguoi choi trong tam thi khung lai, loi do sang (bao truoc) roi bo nhao toi cho
    /// nguoi choi dung luc bat dau bo, bay nguoc len nghi roi bo tiep. Chet / nguoi choi roi phong thi sang lai.
    /// </summary>
    public class LampsnufferController : BuriedEnemyBase
    {
        [Header("Bay")]
        [SerializeField] float wanderRadius = 3f;
        [SerializeField] float acceleration = 20f;

        [Header("Hut sang")]
        [SerializeField] float drainTime = 0.8f;
        [SerializeField] float drainCooldown = 4f;
        [Tooltip("Moi lan hut lam anh sang chung giam bao nhieu (0.25 = 25%).")]
        [SerializeField] float darknessPerDrain = 0.25f;
        [SerializeField] int maxDrainStacks = 3;
        [Tooltip("Moi lan hut Dive nhanh them bao nhieu (0.15 = 15%).")]
        [SerializeField] float drainSpeedBonus = 0.15f;

        [Header("Bo nhao")]
        [SerializeField] float diveRange = 6f;
        [SerializeField] float diveWindup = 0.4f;
        [SerializeField] float diveSpeed = 14f;
        [SerializeField] float maxDiveTime = 0.7f;
        [SerializeField] float diveCooldown = 1.5f;
        [Tooltip("Sau khi bo, bay len cao hon nguoi choi bao nhieu.")]
        [SerializeField] float recoverHeight = 3f;

        [Header("Chet")]
        [SerializeField] float dieTime = 0.67f;

        ActionState wander, drain, windup, dive, recover, dead;
        Room ownRoom;
        Vector2 wanderTarget, diveTarget, diveDir;
        float cooldown, drainTimer;
        int stacks;

        static readonly int FlyHash = Animator.StringToHash("fly");
        static readonly int DrainHash = Animator.StringToHash("drain");
        static readonly int HurtHash = Animator.StringToHash("hurt");
        static readonly int DieHash = Animator.StringToHash("die");

        protected override IState InitialState => wander;

        // Khung chuan bi va bo nhao: co giap. Dang hut sang van danh ngat duoc (cach khac che).
        protected override bool CanStagger() => Machine.Current != windup && Machine.Current != dive;

        protected override void Awake()
        {
            base.Awake();
            body.gravityScale = 0f;
        }

        protected override void Start()
        {
            base.Start();
            ownRoom = Room.At(StartPosition);
            drainTimer = drainCooldown;
        }

        void OnDisable() => AmbientDarkness.Remove(this);

        bool PlayerInRoom =>
            Player && Player.gameObject.activeInHierarchy && (!ownRoom || Room.At(Player.position) == ownRoom);

        bool PlayerInDiveRange =>
            PlayerInRoom && PlayerDistanceSqr <= diveRange * diveRange &&
            !Physics2D.Linecast(body.position, PlayerAim, config.groundLayers);

        Vector2 PlayerAim => Player ? (Vector2)Player.position + Vector2.up * 0.8f : body.position;

        float DiveSpeed => diveSpeed * (1f + drainSpeedBonus * stacks) * SpeedScale;

        bool ReadyToDrain => PlayerInRoom && stacks < maxDrainStacks && drainTimer <= 0f;

        protected override void BuildStates(StateMachine machine)
        {
            wander = new ActionState(
                enter: () => { PlayAnimHash(FlyHash); PickWanderTarget(); },
                tick: Cooldowns,
                fixedTick: dt =>
                {
                    if (Vector2.Distance(body.position, wanderTarget) < 0.3f) PickWanderTarget();
                    FlyTowards(wanderTarget, config.moveSpeed * 0.6f, dt);
                });
            drain = new ActionState(
                enter: () => { PlayAnimHash(DrainHash); PlayWindupFx(true, drainTime); },
                fixedTick: dt => Brake(dt),
                exit: () =>
                {
                    drainTimer = drainCooldown;
                    // Bi ngat (trung don) thi khong tinh.
                    if (drain.Elapsed < drainTime) return;
                    stacks++;
                    AmbientDarkness.Set(this, darknessPerDrain * stacks);
                });
            windup = new ActionState(
                enter: () => { PlayAttack(); FacePlayer(); PlayWindupFx(true, diveWindup); diveTarget = PlayerAim; },
                tick: _ => diveTarget = PlayerAim,
                fixedTick: dt => Brake(dt));
            dive = new ActionState(
                enter: () =>
                {
                    diveDir = (diveTarget - body.position).normalized;
                    Face(diveDir.x);
                    SetAttackHitbox(true);
                },
                fixedTick: _ => body.linearVelocity = diveDir * DiveSpeed,
                exit: () => { SetAttackHitbox(false); cooldown = diveCooldown; });
            recover = new ActionState(
                enter: () => PlayAnimHash(FlyHash),
                tick: Cooldowns,
                fixedTick: dt =>
                {
                    var above = Player ? (Vector2)Player.position + Vector2.up * recoverHeight : (Vector2)StartPosition;
                    FlyTowards(above, config.moveSpeed, dt);
                    if (Player) Face(PlayerDx);
                });
            dead = new ActionState(enter: () => { SetAttackHitbox(false); PlayWindupFx(false); PlayAnimHash(DieHash); });

            machine.AddAnyTransition(hurt, () => ShouldStagger);
            machine.AddTransition(hurt, recover, () => HurtFinished);

            machine.AddTransition(wander, windup, () => cooldown <= 0f && PlayerInDiveRange);
            machine.AddTransition(wander, drain, () => ReadyToDrain);
            machine.AddTransition(drain, wander, () => drain.Elapsed >= drainTime);
            machine.AddTransition(windup, dive, () => windup.Elapsed >= diveWindup);
            machine.AddTransition(dive, recover, () =>
                dive.Elapsed >= maxDiveTime || Vector2.Dot(diveTarget - body.position, diveDir) <= 0f ||
                (dive.Elapsed > 0.1f && body.linearVelocity.sqrMagnitude < 1f));
            machine.AddTransition(recover, drain, () => cooldown <= 0f && ReadyToDrain);
            machine.AddTransition(recover, windup, () => cooldown <= 0f && PlayerInDiveRange);
            machine.AddTransition(recover, wander, () => cooldown <= 0f && !PlayerInDiveRange);
        }

        void Cooldowns(float dt)
        {
            cooldown -= dt;
            drainTimer -= dt;
            // Nguoi choi roi phong: man sang lai (giu so lan hut, quay lai van toi nhu cu).
            AmbientDarkness.Set(this, PlayerInRoom ? darknessPerDrain * stacks : 0f);
        }

        void PickWanderTarget() =>
            wanderTarget = (Vector2)StartPosition + Random.insideUnitCircle * wanderRadius;

        void FlyTowards(Vector2 target, float maxSpeed, float dt)
        {
            Vector2 to = target - body.position;
            float dist = to.magnitude;
            float speed = Mathf.Min(maxSpeed * SpeedScale, dist * 2f);
            Vector2 desired = dist > 0.001f ? to / dist * speed : Vector2.zero;
            body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, desired, acceleration * dt);
            if (Mathf.Abs(desired.x) > 0.1f) Face(desired.x);
        }

        void Brake(float dt) => body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, Vector2.zero, acceleration * dt);

        // Bay: bi day lui theo ca hai truc.
        protected override void OnHurt(HitInfo hit) => body.linearVelocity = hit.direction * hit.knockback;

        protected override void PlayHurtAnim() => PlayAnimHash(HurtHash);

        protected override bool OnDeath(HitInfo hit)
        {
            stacks = 0;
            AmbientDarkness.Remove(this);
            Machine.SetState(dead);
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            body.linearVelocity = Vector2.zero;
            body.gravityScale = 1f; // xac roi xuong
            CancelInvoke();
            Invoke(nameof(Vanish), dieTime);
            return true;
        }

        void Vanish() => gameObject.SetActive(false);

        protected override void OnResetState()
        {
            CancelInvoke();
            stacks = 0;
            AmbientDarkness.Remove(this);
            body.gravityScale = 0f;
            foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.enabled = true;
            ReapplyIgnorePlayer();
            cooldown = 0f;
            drainTimer = drainCooldown;
            base.OnResetState();
        }
    }
}
