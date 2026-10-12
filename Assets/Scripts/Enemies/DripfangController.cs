using LuHanh.Combat;
using LuHanh.Core;
using LuHanh.Core.FSM;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Nhu da song (Dripfang): treo tren tran gia lam nhu da (khong danh duoc). Nguoi choi di vao ngay ben duoi
    /// -> bui roi, khe do he mo (bao truoc) -> roi thang xuong, trung nguoi choi thi gay sat thuong
    /// -> cam xuong san thanh be dung duoc (danh duoc) -> het gio thi nut roi vo. Khong moc lai, hoi sinh cung quai khac.
    /// Rigidbody Kinematic: tu di chuyen, khong theo trong luc.
    /// Pivot moi frame o mat dat / mui nhon (chan ve cach canh duoi ~8 px), nen luc cam san transform = mat dat.
    /// </summary>
    public class DripfangController : BuriedEnemyBase
    {
        [Header("Phat hien")]
        [Tooltip("Nguoi choi cach theo chieu ngang trong khoang nay va o ben duoi thi tinh giac.")]
        [SerializeField] float triggerHalfWidth = 1.5f;
        [SerializeField] float triggerDepth = 12f;

        [Header("Roi")]
        [SerializeField] float wakeTime = 0.6f;
        [SerializeField] float fallGravity = 60f;
        [SerializeField] float maxFallSpeed = 25f;

        [Header("Cam san")]
        [SerializeField] float embeddedTime = 6f;
        [Tooltip("Giay cuoi luc cam san: hien vet nut (sap vo).")]
        [SerializeField] float crackTime = 1f;
        [Tooltip("Be dung duoc luc cam san (layer Ground), tinh tu mat dat.")]
        [SerializeField] Vector2 platformSize = new Vector2(1f, 1f);
        [SerializeField] Vector2 platformOffset = new Vector2(0f, 0.5f);

        [Header("Chet / vo")]
        [SerializeField] float dieTime = 0.67f;

        ActionState hang, wake, fall, embedded, dead;
        Collider2D[] colliders;
        Light2D[] lights;
        LuHanh.Effects.SpriteOutline[] outlines;
        BoxCollider2D platform;
        float fallSpeed, embedStart, hangTopOffset, hangTipOffset;
        bool cracked;

        static readonly int HangHash = Animator.StringToHash("hang");
        static readonly int WakeHash = Animator.StringToHash("wake");
        static readonly int FallHash = Animator.StringToHash("fall");
        static readonly int EmbeddedHash = Animator.StringToHash("embedded");
        static readonly int CrackHash = Animator.StringToHash("crack");
        static readonly int HurtHash = Animator.StringToHash("hurt");
        static readonly int DieHash = Animator.StringToHash("die");

        protected override IState InitialState => hang;

        protected override void Awake()
        {
            colliders = GetComponentsInChildren<Collider2D>(true);
            lights = GetComponentsInChildren<Light2D>(true);
            outlines = GetComponentsInChildren<LuHanh.Effects.SpriteOutline>(true);

            // Be dung la object rieng (khong phai con): EnemyBase bo va cham giua moi collider con voi nguoi choi.
            var go = new GameObject(name + "_Platform") { layer = LayerMask.NameToLayer("Ground") };
            platform = go.AddComponent<BoxCollider2D>();
            platform.size = platformSize;
            go.SetActive(false);
            base.Awake();
        }

        protected override void OnDestroy()
        {
            if (platform) Destroy(platform.gameObject);
            base.OnDestroy();
        }

        void OnDisable() { if (platform) platform.gameObject.SetActive(false); }

        bool PlayerBelow =>
            Player && Player.gameObject.activeInHierarchy &&
            Mathf.Abs(PlayerDx) <= triggerHalfWidth && PlayerDy < 0f && PlayerDy > -triggerDepth;

        // Chi danh duoc luc cam san.
        protected override bool CanStagger() => Machine.Current == embedded;

        // Khong bi day lui (Kinematic: van toc se lam no troi di).
        protected override void OnHurt(HitInfo hit) { }

        protected override void BuildStates(StateMachine machine)
        {
            hang = new ActionState(enter: () => { SetColliders(false); SetLights(false); PlayAnimHash(HangHash); });
            wake = new ActionState(enter: () => { PlayAnimHash(WakeHash); PlayWindupFx(true, wakeTime); CameraShake.Kick(0.05f, wakeTime); });
            fall = new ActionState(
                enter: () =>
                {
                    fallSpeed = 0f;
                    // Giu mui nhon dung cho: hinh Hang co mui cao hon pivot, hinh Fall mui nam o pivot.
                    body.position += Vector2.up * hangTipOffset;
                    SetLights(true);
                    SetAttackHitbox(true);
                    if (attackHitbox) attackHitbox.GetComponent<Collider2D>().enabled = true;
                    PlayAnimHash(FallHash);
                },
                fixedTick: FallStep,
                exit: () => SetAttackHitbox(false));
            embedded = new ActionState(
                enter: () =>
                {
                    SetColliders(true);
                    SetAttackHitbox(false);
                    platform.transform.position = body.position + platformOffset;
                    platform.gameObject.SetActive(true);
                    PlayAnimHash(cracked ? CrackHash : EmbeddedHash);
                },
                tick: TickEmbedded);
            dead = new ActionState(enter: () =>
            {
                SetColliders(false);
                platform.gameObject.SetActive(false);
                PlayWindupFx(false);
                PlayAnimHash(DieHash);
                CancelInvoke();
                Invoke(nameof(Vanish), dieTime);
            });

            machine.AddAnyTransition(hurt, () => ShouldStagger);
            // Bi danh van dem gio cam san, het gio trong luc khung thi vo luon.
            machine.AddTransition(hurt, dead, () => EmbedTime >= embeddedTime);
            machine.AddTransition(hurt, embedded, () => HurtFinished);
            machine.AddTransition(hang, wake, () => PlayerBelow);
            machine.AddTransition(wake, fall, () => wake.Elapsed >= wakeTime);
            machine.AddTransition(embedded, dead, () => EmbedTime >= embeddedTime);
        }

        protected override void Start()
        {
            base.Start();
            // Do mot lan tren hinh Hang (prefab de san Hang_00): pivot -> dinh hinh, pivot -> mui nhon.
            var s = sprite ? sprite.sprite : null;
            hangTopOffset = s ? s.bounds.max.y : 2f;
            hangTipOffset = s ? Mathf.Max(0f, MinY(s)) : 0f;
            AttachToCeiling();
        }

        /// <summary>Dat dinh nhu da cham tran ngay tren cho dat.</summary>
        void AttachToCeiling()
        {
            var hit = Physics2D.Raycast(body.position, Vector2.up, 20f, config.groundLayers);
            if (hit.collider) body.position = new Vector2(body.position.x, hit.point.y - hangTopOffset);
            transform.position = body.position;
        }

        void FallStep(float dt)
        {
            fallSpeed = Mathf.Min(fallSpeed + fallGravity * dt, maxFallSpeed);
            float step = fallSpeed * dt;
            var hit = Physics2D.Raycast(body.position + Vector2.up * 0.1f, Vector2.down, step + 0.1f, config.groundLayers);
            body.linearVelocity = Vector2.zero;
            if (hit.collider)
            {
                body.position = new Vector2(body.position.x, hit.point.y);
                embedStart = Time.time;
                cracked = false;
                CameraShake.Kick(0.15f, 0.2f);
                Machine.SetState(embedded);
                return;
            }
            body.position += Vector2.down * step;
        }

        void TickEmbedded(float dt)
        {
            if (cracked || EmbedTime < embeddedTime - crackTime) return;
            cracked = true;
            PlayAnimHash(CrackHash);
        }

        /// <summary>Thoi gian da cam san (dem ca luc dang khung).</summary>
        float EmbedTime => Time.time - embedStart;

        static float MinY(Sprite s)
        {
            float m = float.MaxValue;
            foreach (var v in s.vertices) m = Mathf.Min(m, v.y);
            return m;
        }

        void SetColliders(bool on)
        {
            foreach (var c in colliders) if (c) c.enabled = on;
            if (on) ReapplyIgnorePlayer();
        }

        // Luc treo gia lam nhu da: tat ca den rieng lan vien quai.
        void SetLights(bool on)
        {
            foreach (var l in lights) if (l) l.enabled = on;
            foreach (var o in outlines) if (o) o.gameObject.SetActive(on);
        }

        protected override void PlayHurtAnim() => PlayAnimHash(HurtHash);

        protected override bool OnDeath(HitInfo hit)
        {
            Machine.SetState(dead);
            return true;
        }

        void Vanish() => gameObject.SetActive(false);

        protected override void OnResetState()
        {
            CancelInvoke();
            if (platform) platform.gameObject.SetActive(false);
            base.OnResetState();
            Machine.SetState(hang);
            AttachToCeiling();
        }
    }
}
