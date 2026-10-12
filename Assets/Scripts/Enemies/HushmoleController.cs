using LuHanh.Combat;
using LuHanh.Core;
using LuHanh.Core.FSM;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Chuot chui mu (Hushmole): khong nhin, chi nghe. Nghe tieng dong (nguoi choi chay, nhay, roi, dash)
    /// -> dung nghe (khe do sang, bao truoc) -> chui xuong dat -> dao toi CHO PHAT RA TIENG CUOI CUNG
    /// (dang dao ma nghe tieng moi thi doi huong) -> dat nut + anh do (bao truoc) -> troi len gay sat thuong.
    /// Troi len ma nguoi choi o gan va van gay tieng thi luot toi vo; khong thi dung nghe (khoang ho de danh).
    /// Duoi dat: tat collider, khong danh duoc.
    /// </summary>
    public class HushmoleController : BuriedEnemyBase
    {
        [Header("Nghe")]
        [SerializeField] float hearRadius = 7f;
        [Tooltip("Toc do nguoi choi tren muc nay moi tinh la gay tieng dong.")]
        [SerializeField] float noiseSpeed = 0.5f;
        [Tooltip("Dung nghe (bao truoc) truoc khi chui xuong.")]
        [SerializeField] float listenTime = 0.6f;
        [Tooltip("Sau khi troi len / bi danh: dung nghe bao lau, khong nghe gi thi quay ve di do.")]
        [SerializeField] float alertTime = 1.2f;

        [Header("Don tho")]
        [SerializeField] float burrowTime = 0.67f;
        [SerializeField] float tunnelSpeed = 12f;
        [Tooltip("Dao toi da bao lau roi troi len du chua toi.")]
        [SerializeField] float maxTunnelTime = 4f;
        [Tooltip("Dat nut + anh do truoc khi troi len.")]
        [SerializeField] float emergeTelegraph = 0.5f;
        [Tooltip("Khoang thoi gian gay sat thuong luc troi len.")]
        [SerializeField] float emergeHitTime = 0.3f;
        [SerializeField] float emergeRecover = 0.4f;

        [Header("Luot vo")]
        [Tooltip("Troi len ma nguoi choi trong khoang nay va dang gay tieng thi luot toi vo.")]
        [SerializeField] float lungeRange = 5f;
        [SerializeField] float lungeWindup = 0.3f;
        [Tooltip("Thoi gian luot. Quang luot tu tinh de dung ngay truoc mat nguoi choi.")]
        [SerializeField] float lungeDashTime = 0.3f;
        [SerializeField] float lungeMaxDistance = 5.5f;
        [SerializeField] float lungeHop = 3f;
        [SerializeField] float lungeRecover = 0.4f;

        [Header("Chet")]
        [SerializeField] float dieTime = 0.67f;

        ActionState walk, listen, burrow, tunnel, telegraph, emerge, alert, lunge, dead;
        Rigidbody2D playerBody;
        Collider2D[] colliders;
        Light2D[] lights;
        float gravity, groundY, noiseX, lungeSpeed;
        bool hidden, lunged;

        static readonly int ListenHash = Animator.StringToHash("listen");
        static readonly int TunnelHash = Animator.StringToHash("tunnel");
        static readonly int HurtHash = Animator.StringToHash("hurt");
        static readonly int DieHash = Animator.StringToHash("die");

        protected override IState InitialState => walk;

        protected override void Awake()
        {
            colliders = GetComponentsInChildren<Collider2D>(true);
            lights = GetComponentsInChildren<Light2D>(true);
            gravity = GetComponent<Rigidbody2D>().gravityScale;
            base.Awake();
        }

        /// <summary>Nguoi choi dang gay tieng trong tam nghe. Nghe thay thi ghi lai cho phat ra tieng.</summary>
        bool HearPlayer()
        {
            if (!Player || !Player.gameObject.activeInHierarchy) return false;
            if ((Player.position - transform.position).sqrMagnitude > hearRadius * hearRadius) return false;
            if (!playerBody) playerBody = Player.GetComponent<Rigidbody2D>();
            if (!playerBody || playerBody.linearVelocity.sqrMagnitude <= noiseSpeed * noiseSpeed) return false;
            noiseX = Player.position.x;
            return true;
        }

        bool PlayerInLungeRange() => Mathf.Abs(PlayerDx) <= lungeRange && Mathf.Abs(PlayerDy) < 1.5f;

        protected override bool CanStagger() => !hidden;

        protected override void BuildStates(StateMachine machine)
        {
            walk = new ActionState(enter: PlayWalk, fixedTick: _ => PatrolStep());
            listen = new ActionState(
                enter: () => { Stop(); FacePlayer(); PlayAnimHash(ListenHash); PlayWindupFx(true, listenTime); },
                tick: _ => HearPlayer(),
                fixedTick: _ => Stop());
            burrow = new ActionState(enter: () => { Stop(); PlayBurrow(); groundY = body.position.y; }, fixedTick: _ => Stop());
            tunnel = new ActionState(
                enter: () => { SetHidden(true); PlayAnimHash(TunnelHash); },
                tick: _ => HearPlayer(),
                fixedTick: TunnelStep);
            telegraph = new ActionState(
                enter: () => { HoldUnderground(); PlayEmerge(); CameraShake.Kick(0.1f, emergeTelegraph); },
                fixedTick: _ => HoldUnderground());
            emerge = new ActionState(
                enter: () => { SetHidden(false); FacePlayer(); SetAttackHitbox(true); },
                tick: _ => { if (emerge.Elapsed >= emergeHitTime) SetAttackHitbox(false); },
                fixedTick: _ => Stop(),
                exit: () => SetAttackHitbox(false));
            alert = new ActionState(enter: () => { Stop(); PlayIdle(); }, fixedTick: _ => Stop());
            lunge = new ActionState(
                enter: () =>
                {
                    lunged = false;
                    Stop();
                    FacePlayer();
                    PlayAttack();
                    lungeSpeed = Mathf.Clamp(Mathf.Abs(PlayerDx), 1f, lungeMaxDistance) / lungeDashTime * SpeedScale;
                },
                tick: _ =>
                {
                    if (lunged || lunge.Elapsed < lungeWindup) return;
                    lunged = true;
                    SetAttackHitbox(true);
                    SetVelocity(new Vector2(FacingSign * lungeSpeed, lungeHop));
                },
                // Het quang luot thi ham lai, khong truot tiep.
                fixedTick: _ => { if (lunged && lunge.Elapsed >= lungeWindup + lungeDashTime) SetHorizontalSpeed(0f); },
                exit: () => SetAttackHitbox(false));
            dead = new ActionState(enter: () => { Stop(); SetAttackHitbox(false); PlayWindupFx(false); PlayAnimHash(DieHash); });

            machine.AddAnyTransition(hurt, () => ShouldStagger);
            machine.AddTransition(hurt, alert, () => HurtFinished);
            machine.AddTransition(walk, listen, HearPlayer);
            machine.AddTransition(listen, burrow, () => listen.Elapsed >= listenTime);
            machine.AddTransition(burrow, tunnel, () => burrow.Elapsed >= burrowTime);
            machine.AddTransition(tunnel, telegraph, () => Mathf.Abs(noiseX - body.position.x) <= 0.3f || tunnel.Elapsed >= maxTunnelTime);
            machine.AddTransition(telegraph, emerge, () => telegraph.Elapsed >= emergeTelegraph);
            machine.AddTransition(emerge, lunge, () => emerge.Elapsed >= emergeRecover && HearPlayer() && PlayerInLungeRange());
            machine.AddTransition(emerge, alert, () => emerge.Elapsed >= emergeRecover);
            machine.AddTransition(lunge, alert, () => lunge.Elapsed >= lungeWindup + lungeDashTime + lungeRecover && IsGrounded());
            machine.AddTransition(alert, lunge, () => alert.Elapsed >= 0.3f && HearPlayer() && PlayerInLungeRange());
            machine.AddTransition(alert, listen, () => alert.Elapsed >= 0.3f && HearPlayer());
            machine.AddTransition(alert, walk, () => alert.Elapsed >= alertTime);
        }

        /// <summary>Duoi dat: tat collider, den va trong luc. Hinh van hien (anim Tunnel la u dat).</summary>
        void SetHidden(bool on)
        {
            hidden = on;
            if (flash && on) flash.enabled = false;
            foreach (var l in lights) if (l) l.enabled = !on;
            // Tat ca collider (than + Hitbox) de don chem khong trung khi dang duoi dat.
            foreach (var c in colliders) if (c) c.enabled = !on;
            body.gravityScale = on ? 0f : gravity;
            body.linearVelocity = Vector2.zero;
            if (on) body.position = new Vector2(body.position.x, groundY);
            else ReapplyIgnorePlayer();
        }

        void HoldUnderground()
        {
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(body.position.x, groundY);
        }

        void TunnelStep(float dt)
        {
            float dx = noiseX - body.position.x;
            float dir = Mathf.Sign(dx);
            float nextX = body.position.x + dir * Mathf.Min(tunnelSpeed * dt, Mathf.Abs(dx));
            // Chi dao duoi dat lien: phia truoc la ho / mep thi dung lai troi len.
            bool groundAhead = Physics2D.Raycast(new Vector2(nextX + dir * 0.5f, groundY + 0.3f), Vector2.down, 0.8f, config.groundLayers);
            Face(dir);
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(groundAhead ? nextX : body.position.x, groundY);
            if (!groundAhead) noiseX = body.position.x;
        }

        protected override void PlayHurtAnim() => PlayAnimHash(HurtHash);

        // Choi animation chet roi moi tat object.
        protected override bool OnDeath(HitInfo hit)
        {
            Machine.SetState(dead);
            foreach (var c in colliders) if (c) c.enabled = false;
            body.simulated = false;
            CancelInvoke();
            Invoke(nameof(Vanish), dieTime);
            return true;
        }

        void Vanish() => gameObject.SetActive(false);

        protected override void OnResetState()
        {
            CancelInvoke();
            body.simulated = true;
            SetHidden(false);
            base.OnResetState();
        }
    }
}
