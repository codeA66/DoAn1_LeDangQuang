using System.Collections.Generic;
using LuHanh.Combat;
using LuHanh.Core.FSM;
using LuHanh.Player;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Bong vong (Echoling): khong tu danh, cham vao khong mat mau. Long lung giu cach nguoi choi 4-6 o.
    /// Thay nguoi choi trong tam -> Record 2 giay (khoi keo dai, loi sang dan - bao truoc), ghi vi tri + tu the
    /// nguoi choi moi buoc vat ly -> Release: tha <see cref="EchoGhost"/> di lai dung duong do tu cho bat dau ghi.
    /// Bi danh luc dang ghi thi ngat. Hoi chieu 4 giay, moi luc chi 1 ban sao. Chet thi ban sao tan.
    /// </summary>
    public class EcholingController : BuriedEnemyBase
    {
        [Header("Bay")]
        [SerializeField] float acceleration = 12f;
        [SerializeField] float keepDistanceMin = 4f;
        [SerializeField] float keepDistanceMax = 6f;
        [SerializeField] float hoverHeight = 2f;
        [SerializeField] float idleRadius = 1.5f;

        [Header("Ghi / phat lai")]
        [SerializeField] float detectRange = 8f;
        [SerializeField] float recordTime = 2f;
        [SerializeField] float releaseTime = 0.67f;
        [SerializeField] float cooldownTime = 4f;
        [Tooltip("Prefab ban sao bong (Prefabs/Enemies/Man2/Echo).")]
        [SerializeField] EchoGhost echoPrefab;

        [Header("Chet")]
        [SerializeField] float dieTime = 0.67f;

        ActionState drift, record, release, dead;
        readonly List<EchoGhost.Sample> path = new List<EchoGhost.Sample>();
        EchoGhost echo;
        PlayerController playerController;
        PlayerAttack playerAttack;
        Rigidbody2D playerBody;
        Vector2 idleTarget;
        float cooldown;

        static readonly int IdleHash = Animator.StringToHash("idle");
        static readonly int RecordHash = Animator.StringToHash("record");
        static readonly int ReleaseHash = Animator.StringToHash("release");
        static readonly int HurtHash = Animator.StringToHash("hurt");
        static readonly int DieHash = Animator.StringToHash("die");

        protected override IState InitialState => drift;

        protected override void Awake()
        {
            if (echoPrefab) echo = Instantiate(echoPrefab);
            base.Awake();
            body.gravityScale = 0f;
        }

        protected override void Start()
        {
            base.Start();
            if (!Player) return;
            playerController = Player.GetComponent<PlayerController>();
            playerAttack = Player.GetComponent<PlayerAttack>();
            playerBody = Player.GetComponent<Rigidbody2D>();
        }

        protected override void OnDestroy()
        {
            if (echo) Destroy(echo.gameObject);
            base.OnDestroy();
        }

        void OnDisable() { if (echo) echo.Stop(); }

        bool CanSeePlayer =>
            Player && Player.gameObject.activeInHierarchy &&
            PlayerDistanceSqr <= detectRange * detectRange &&
            !Physics2D.Linecast(body.position, (Vector2)Player.position + Vector2.up * 0.8f, config.groundLayers);

        protected override void BuildStates(StateMachine machine)
        {
            drift = new ActionState(
                enter: () => { PlayAnimHash(IdleHash); idleTarget = StartPosition; },
                tick: dt => cooldown -= dt,
                fixedTick: Drift);
            record = new ActionState(
                enter: () => { path.Clear(); PlayAnimHash(RecordHash); PlayWindupFx(true, recordTime); },
                fixedTick: dt => { Brake(dt); RecordSample(); },
                exit: () => PlayWindupFx(false));
            release = new ActionState(
                enter: () =>
                {
                    PlayAnimHash(ReleaseHash);
                    if (echo && path.Count > 0) echo.Play(path);
                    cooldown = cooldownTime;
                },
                fixedTick: dt => Brake(dt));
            dead = new ActionState(enter: () => { PlayWindupFx(false); PlayAnimHash(DieHash); if (echo) echo.Stop(); });

            machine.AddAnyTransition(hurt, () => ShouldStagger);
            machine.AddTransition(hurt, drift, () => HurtFinished);
            machine.AddTransition(drift, record, () => cooldown <= 0f && (!echo || !echo.Playing) && CanSeePlayer);
            machine.AddTransition(record, release, () => record.Elapsed >= recordTime);
            machine.AddTransition(release, drift, () => release.Elapsed >= releaseTime);
        }

        void RecordSample()
        {
            if (!Player) return;
            var pose = EchoGhost.Pose.Stand;
            if (playerAttack && playerAttack.IsAttacking) pose = EchoGhost.Pose.Slash;
            else if (playerController && !playerController.IsGrounded) pose = EchoGhost.Pose.Jump;
            else if (playerBody && Mathf.Abs(playerBody.linearVelocity.x) > 0.5f) pose = EchoGhost.Pose.Run;
            path.Add(new EchoGhost.Sample
            {
                position = Player.position,
                pose = pose,
                facingRight = playerController ? playerController.FacingRight : true
            });
        }

        void Drift(float dt)
        {
            Vector2 target;
            if (CanSeePlayer || (Player && PlayerDistanceSqr <= detectRange * detectRange))
            {
                // Giu cach nguoi choi 4-6 o, lo lung tren cao ngang tam dau.
                float side = Mathf.Sign(transform.position.x - Player.position.x);
                if (side == 0f) side = 1f;
                float dist = Mathf.Clamp(Mathf.Abs(PlayerDx), keepDistanceMin, keepDistanceMax);
                target = new Vector2(Player.position.x + side * dist, Player.position.y + hoverHeight);
                Face(-side);
            }
            else
            {
                if (Vector2.Distance(body.position, idleTarget) < 0.3f)
                    idleTarget = (Vector2)StartPosition + Random.insideUnitCircle * idleRadius;
                target = idleTarget;
            }
            Vector2 desired = Vector2.ClampMagnitude((target - body.position) * 2f, config.moveSpeed * SpeedScale);
            body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, desired, acceleration * dt);
        }

        void Brake(float dt) => body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, Vector2.zero, acceleration * dt);

        // Bay: bi day lui theo ca hai truc.
        protected override void OnHurt(HitInfo hit) => body.linearVelocity = hit.direction * hit.knockback;

        protected override void PlayHurtAnim() => PlayAnimHash(HurtHash);

        protected override bool OnDeath(HitInfo hit)
        {
            Machine.SetState(dead);
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            body.linearVelocity = Vector2.zero;
            CancelInvoke();
            Invoke(nameof(Vanish), dieTime);
            return true;
        }

        void Vanish() => gameObject.SetActive(false);

        protected override void OnResetState()
        {
            CancelInvoke();
            if (echo) echo.Stop();
            foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.enabled = true;
            ReapplyIgnorePlayer();
            cooldown = 0f;
            base.OnResetState();
        }
    }
}
