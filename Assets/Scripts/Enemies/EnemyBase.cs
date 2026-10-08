using LuHanh.Combat;
using LuHanh.Core;
using LuHanh.Core.FSM;
using LuHanh.Level;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Phan chung cua moi quai: mau, nhan don + nhay trang, hoi sinh cung nguoi choi,
    /// ngu khi xa camera, quay mat, phat animation. Lop con (di bo, bay...) chi khai bao
    /// trang thai va luat chuyen rieng.
    ///
    /// Toi uu: chet thi SetActive(false) chu khong Destroy; nguoi choi hoi sinh thi
    /// bat lai tai cho cu (khong cap phat, khong rac). Xa camera thi ngu.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable, IResettable, IHazardReceiver
    {
        [Tooltip("Sprite chinh (object con Visual).")]
        [SerializeField] protected SpriteRenderer sprite;
        [Tooltip("Lop phu trang de nhay khi trung don (dung material LuHanh/SpriteFlash).")]
        [SerializeField] protected SpriteRenderer flash;
        [Tooltip("Collider than.")]
        [SerializeField] protected Collider2D bodyCollider;

        protected Rigidbody2D body;
        protected Health health;
        protected Animator animator;
        Transform cameraTransform;
        Collider2D[] ownColliders;
        Collider2D[] playerColliders;
        Room homeRoom;

        Vector3 startPosition;
        bool startFacingRight;
        bool asleep;
        float flashTimer;
        float squashTimer;
        Vector3 visualBaseScale = Vector3.one;
        int currentAnim;

        protected StateMachine Machine { get; private set; }
        /// <summary>Vua trung don trong frame nay; luat chuyen sang trang thai Hurt doc co nay.</summary>
        protected bool HurtRequested { get; private set; }

        public Rigidbody2D Body => body;
        public Transform Player { get; private set; }
        public bool FacingRight { get; private set; }
        public float FacingSign => FacingRight ? 1f : -1f;
        public Vector3 StartPosition => startPosition;

        public float PlayerDistanceSqr =>
            Player ? ((Vector2)(Player.position - transform.position)).sqrMagnitude : float.MaxValue;

        protected abstract EnemyConfigBase BaseConfig { get; }
        /// <summary>Trang thai luc bat dau va luc hoi sinh.</summary>
        protected abstract IState InitialState { get; }
        /// <summary>Tao cac trang thai va khai bao luat chuyen vao <paramref name="machine"/>.</summary>
        protected abstract void BuildStates(StateMachine machine);
        /// <summary>Chuan bi trang thai Hurt (vd luu huong day lui). Goi truoc khi bat co HurtRequested.</summary>
        protected abstract void OnHurt(HitInfo hit);
        /// <summary>
        /// Cho phep loai quai phat animation/va cham chet truoc khi bi tat. Tra ve true neu
        /// lop con tu xu ly viec tat object sau khi animation ket thuc.
        /// </summary>
        protected virtual bool OnDeath(HitInfo hit) => false;
        /// <summary>Viec rieng sau khi hoi sinh (vd phat lai clip).</summary>
        protected virtual void OnResetState() { }

        /// <summary>Lop con dam bao config khac null roi moi goi base.Awake().</summary>
        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            animator = GetComponentInChildren<Animator>();
            if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
            if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
            ownColliders = GetComponentsInChildren<Collider2D>(true);

            body.freezeRotation = true;
            health.SetMax(BaseConfig.maxHealth);
            if (flash) flash.enabled = false;
            if (sprite) visualBaseScale = sprite.transform.localScale;

            startPosition = transform.position;
            FacingRight = (transform.localScale.x >= 0f) == BaseConfig.artFacesRight;
            startFacingRight = FacingRight;

            Machine = new StateMachine();
            BuildStates(Machine);
            Machine.SetState(InitialState);
            RespawnManager.Register(this);
        }

        protected virtual void Start()
        {
            // Tim mot lan, khong tim lai moi frame.
            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject)
            {
                Player = playerObject.transform;
                playerColliders = playerObject.GetComponentsInChildren<Collider2D>();
                IgnorePlayer();
            }
            if (Camera.main) cameraTransform = Camera.main.transform;
            // Start chay sau moi OnEnable nen luc nay moi Room da dang ky.
            homeRoom = Room.At(startPosition);
        }

        // Physics2D.IgnoreCollision mat hieu luc khi collider bi tat/bat (quai chet roi hoi sinh,
        // quai cua RoomEncounter duoc bat) nen phai dat lai moi lan bat.
        void OnEnable()
        {
            if (playerColliders != null) IgnorePlayer();
        }

        /// <summary>Di xuyen qua nguoi choi nhu Hollow Knight; sat thuong do Hitbox (trigger) lo.</summary>
        void IgnorePlayer()
        {
            foreach (var mine in ownColliders)
            {
                if (mine.isTrigger) continue;
                foreach (var theirs in playerColliders)
                    if (theirs) Physics2D.IgnoreCollision(mine, theirs);
            }
        }

        protected virtual void OnDestroy() => RespawnManager.Unregister(this);

        void Update()
        {
            UpdateSleep();
            if (asleep) return;

            Machine.Tick(Time.deltaTime);
            HurtRequested = false;
        }

        void FixedUpdate()
        {
            if (asleep) return;
            Machine.FixedTick(Time.fixedDeltaTime);
            KeepInHomeRoom();
        }

        /// <summary>
        /// Quai chi o trong phong cua minh (phong chua vi tri bat dau), khong duoi nguoi choi sang
        /// phong khac. Cham mep trai/phai/tren thi dung lai o mep, bo van toc huong ra ngoai.
        /// Quai dat ngoai moi phong thi khong bi gioi han.
        /// </summary>
        void KeepInHomeRoom()
        {
            if (!homeRoom) return;
            var rect = homeRoom.Bounds;
            Vector2 half = bodyCollider ? (Vector2)bodyCollider.bounds.extents : Vector2.zero;
            Vector2 offset = bodyCollider ? (Vector2)bodyCollider.bounds.center - body.position : Vector2.zero;
            Vector2 centre = body.position + offset;
            Vector2 clamped = new Vector2(
                Mathf.Clamp(centre.x, rect.xMin + half.x, rect.xMax - half.x),
                // Khong chan mep duoi: quai roi xuong vuc van phai cham vung chet ben duoi.
                Mathf.Min(centre.y, rect.yMax - half.y));
            if (clamped == centre) return;

            var v = body.linearVelocity;
            if ((clamped.x > centre.x && v.x < 0f) || (clamped.x < centre.x && v.x > 0f)) v.x = 0f;
            if ((clamped.y > centre.y && v.y < 0f) || (clamped.y < centre.y && v.y > 0f)) v.y = 0f;
            body.linearVelocity = v;
            body.position = clamped - offset;
        }

        protected virtual void LateUpdate()
        {
            UpdateSquash();
            if (!flash || !flash.enabled) return;
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f) { flash.enabled = false; return; }

            // Chep sprite sau khi Animator da doi khung trong frame nay.
            flash.sprite = sprite.sprite;
            var c = flash.color;
            c.a = flashTimer / BaseConfig.flashTime;
            flash.color = c;
        }

        /// <summary>
        /// Bop hinh khi trung don roi bat ve: scale cua object Visual di tu hitSquash ve 1 trong
        /// squashTime. Dem bang Time.deltaTime nen dung yen trong luc hitstop -> thay ro hinh bi bop.
        /// </summary>
        void UpdateSquash()
        {
            if (squashTimer <= 0f || !sprite) return;
            squashTimer -= Time.deltaTime;
            float k = Mathf.Clamp01(squashTimer / Mathf.Max(0.001f, BaseConfig.squashTime));   // 1 -> 0
            var squash = Vector2.Lerp(Vector2.one, BaseConfig.hitSquash, k);
            sprite.transform.localScale = new Vector3(visualBaseScale.x * squash.x, visualBaseScale.y * squash.y, visualBaseScale.z);
        }

        // ---- Ngu khi xa camera ----

        void UpdateSleep()
        {
            if (!cameraTransform) return;
            float limit = BaseConfig.sleepDistance;
            bool far = ((Vector2)(transform.position - cameraTransform.position)).sqrMagnitude > limit * limit;
            if (far == asleep) return;

            asleep = far;
            body.simulated = !far;
            if (animator) animator.enabled = !far;
        }

        // ---- Nhan don ----

        public void TakeHit(HitInfo hit)
        {
            if (!health.Damage(hit.damage)) return;

            squashTimer = BaseConfig.squashTime;
            if (flash)
            {
                flashTimer = BaseConfig.flashTime;
                flash.enabled = true;
            }

            if (health.IsDead)
            {
                Vector2 centre = bodyCollider ? (Vector2)bodyCollider.bounds.center : (Vector2)transform.position;
                LuHanh.Player.SoulOrbPool.Spawn(centre, BaseConfig.soulOrbs);
                if (OnDeath(hit)) return;

                // Mac dinh tat ngay trong frame chet; quai can animation chet co the tre viec tat.
                gameObject.SetActive(false);
                return;
            }

            hit.knockback *= BaseConfig.knockbackScale;
            OnHurt(hit);
            HurtRequested = true;
        }

        /// <summary>Roi xuong ho / cham bay: chet ngay, khong tinh mau con lai. Hoi sinh lai khi nguoi choi hoi sinh.</summary>
        public void TakeHazardHit(int damage)
        {
            if (health.IsDead) return;
            health.Damage(health.Current);
            gameObject.SetActive(false);
        }

        /// <summary>Goi khi nguoi choi hoi sinh: ve cho cu, day mau, ve trang thai dau.</summary>
        public void ResetState()
        {
            transform.position = startPosition;
            body.position = startPosition;
            body.linearVelocity = Vector2.zero;
            Face(startFacingRight ? 1f : -1f);
            health.Refill();
            HurtRequested = false;
            flashTimer = 0f;
            if (flash) flash.enabled = false;
            squashTimer = 0f;
            if (sprite) sprite.transform.localScale = visualBaseScale;
            currentAnim = 0;

            gameObject.SetActive(true);
            Machine.SetState(InitialState);
            OnResetState();
        }

        // ---- Tien ich cho cac trang thai ----

        /// <summary>Quay mat theo dau cua <paramref name="direction"/>. Lat bang localScale.x giong player.</summary>
        public void Face(float direction)
        {
            if (Mathf.Abs(direction) < 0.01f) return;
            bool right = direction > 0f;
            FacingRight = right;
            var s = transform.localScale;
            float x = Mathf.Abs(s.x);
            s.x = right == BaseConfig.artFacesRight ? x : -x;
            transform.localScale = s;
        }

        /// <summary>Doi clip neu khac clip dang phat (khong goi lai Animator.Play moi frame).</summary>
        protected void PlayAnimHash(int hash)
        {
            if (!animator || hash == currentAnim) return;
            currentAnim = hash;
            animator.Play(hash, 0, 0f);
        }
    }
}
