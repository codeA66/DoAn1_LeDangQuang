using LuHanh.Combat;
using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Mau nguoi choi: moi don mat 1 mask, khung hinh khung lai mot thoang va bi bat lui.
    /// Sau khi trung don co bat tu <c>invulnerableTime</c> giay (khong nhap nhay), kem rung man hinh va tua cham nhu Hollow Knight:
    /// trong luc bat tu moi don cua quai deu bi bo qua (khong tinh roi ho / bay).
    /// Het mask thi hoi sinh o checkpoint.
    /// Roi vao ho / bay thi mat 1 mask va hien lai o cho dung an toan gan nhat.
    /// So mau nam o <see cref="Health"/>; lop nay lo phan phan ung.
    /// </summary>
    [RequireComponent(typeof(PlayerController), typeof(Health))]
    public class PlayerHealth : MonoBehaviour, IDamageable, IHazardReceiver
    {
        [Header("Bi danh trung")]
        [Tooltip("Phan ung khi don danh khong gui kem HitReaction rieng (day lui, chop sang...).")]
        [SerializeField] HitReaction defaultReaction;
        [Tooltip("Khung hinh dung lai bao lau khi roi bay / chet (giay thuc).")]
        [SerializeField] float hitStopTime = 0.1f;
        [Tooltip("Lop phu de chop sang (SpriteRenderer con, material LuHanh/SpriteFlash), chep hinh tu Visual.")]
        [SerializeField] SpriteRenderer hurtFlash;
        [SerializeField] SpriteRenderer visual;
        [Tooltip("Bat tu sau khi trung don (giay). 0 = tat.")]
        [SerializeField] float invulnerableTime = 1.3f;
        [Tooltip("Rung man hinh khi trung don: bien do (unit) / thoi gian tat dan (giay).")]
        [SerializeField] float hurtShake = 0.45f;
        [SerializeField] float hurtShakeTime = 0.35f;
        [Tooltip("Tua cham sau khi trung don kieu Hollow Knight: toc do game / bao lau (giay thuc).")]
        [Range(0.05f, 1f)][SerializeField] float hurtSlowScale = 0.3f;
        [SerializeField] float hurtSlowTime = 0.3f;

        [Header("Roi vao bay / ho")]
        [Tooltip("Dung vung tren dat lien tuc bao lau thi vi tri do moi duoc nho lam cho an toan.")]
        [SerializeField] float safeGroundTime = 0.2f;

        Health health;
        PlayerController controller;
        PlayerAttack attack;
        PlayerMovementFx movementFx;
        PlayerActionAnimation actionAnimation;
        Rigidbody2D body;

        static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        MaterialPropertyBlock flashBlock;
        int flashesLeft;
        float flashOn, flashOff, flashTimer;
        bool flashLit;

        bool dead;
        bool hazardRespawning;
        Vector2 checkpoint;
        Vector2 safePosition;
        float groundedTimer;
        float invulnerableTimer;
        int baseMaxMasks;   // so mask goc tren Health; mask toi da that = PlayerStats.Apply(MaxMasks, goc)

        public Health Health => health;
        /// <summary>Dang trong khoang bat tu sau khi trung don.</summary>
        public bool IsInvulnerable => invulnerableTimer > 0f;

        void Awake()
        {
            health = GetComponent<Health>();
            controller = GetComponent<PlayerController>();
            attack = GetComponent<PlayerAttack>();
            movementFx = GetComponent<PlayerMovementFx>();
            actionAnimation = GetComponent<PlayerActionAnimation>();
            body = GetComponent<Rigidbody2D>();
            flashBlock = new MaterialPropertyBlock();
            if (hurtFlash) hurtFlash.enabled = false;

            // Chua co ghe/checkpoint: hoi sinh tai cho bat dau man.
            checkpoint = transform.position;
            safePosition = checkpoint;
            baseMaxMasks = health.Max;
        }

        void Start() => health.SetMax(PlayerStats.ApplyInt(PlayerStat.MaxMasks, baseMaxMasks), true);
        void OnEnable() => PlayerStats.Changed += OnStatsChanged;
        void OnDisable() => PlayerStats.Changed -= OnStatsChanged;

        /// <summary>Nang cap / bo nang cap mask giua luc choi: doi toi da, giu mau hien tai.</summary>
        void OnStatsChanged()
        {
            int max = PlayerStats.ApplyInt(PlayerStat.MaxMasks, baseMaxMasks);
            if (max != health.Max) health.SetMax(max, false);
        }

        /// <summary>Doi diem hoi sinh (ghe ngoi, cua man...).</summary>
        public void SetCheckpoint(Vector2 position) => checkpoint = position;

        public void TakeHit(HitInfo hit)
        {
            if (dead || hazardRespawning || IsInvulnerable) return;
            if (!health.Damage(hit.damage)) return;
            Sfx.Play(SfxId.PlayerHurt);

            if (health.IsDead)
            {
                StartHitStop();
                Die();
                return;
            }

            invulnerableTimer = invulnerableTime;
            CameraShake.Kick(hurtShake, hurtShakeTime);
            HitStop.SlowMotion(hurtSlowScale, hurtSlowTime);
            var reaction = hit.reaction ? hit.reaction : defaultReaction;
            if (reaction) reaction.Apply(this, hit);
            else StartHitStop();
        }

        // ---- Cac phan ung co ban, HitReaction goi tuy y ----

        /// <summary>Khung hinh mot thoang (giay thuc).</summary>
        public void HitStopFor(float seconds)
        {
            if (seconds > 0f) HitStop.Request(seconds);
        }

        /// <summary>Bat ra xa nguon don: X theo huong <paramref name="direction"/>, Y huong len; mat dieu khien <paramref name="hurtSeconds"/>.</summary>
        public void Knockback(Vector2 direction, Vector2 velocity, float hurtSeconds)
        {
            float side = direction.x != 0f ? Mathf.Sign(direction.x) : (controller.FacingRight ? -1f : 1f);
            controller.Hurt(new Vector2(side * velocity.x, velocity.y), hurtSeconds);
        }

        /// <summary>
        /// Dang bi trung: than ngua ra sau <paramref name="leanAngle"/> do (bi danh tu sau lung thi chui ve truoc)
        /// va bep theo <paramref name="squash"/>, roi tu tro ve.
        /// </summary>
        public void HurtPose(Vector2 direction, Vector2 squash, float leanAngle)
        {
            if (!movementFx) return;
            // direction = huong bi day. Bi day cung chieu dang nhin nghia la don den tu sau lung.
            bool fromBehind = direction.x != 0f && (direction.x > 0f) == controller.FacingRight;
            movementFx.Punch(squash, Vector2.zero, fromBehind ? -leanAngle : leanAngle);
        }

        /// <summary>Chop sang <paramref name="count"/> lan. Dem bang thoi gian thuc nen van chop trong luc khung hinh.</summary>
        public void Flash(Color color, int count, float onTime, float offTime)
        {
            if (!hurtFlash || !visual || count <= 0) return;
            flashBlock.SetColor(FlashColorId, color);
            hurtFlash.SetPropertyBlock(flashBlock);
            flashesLeft = count;
            flashOn = onTime;
            flashOff = offTime;
            SetFlashLit(true);
        }

        void SetFlashLit(bool lit)
        {
            flashLit = lit;
            flashTimer = lit ? flashOn : flashOff;
            hurtFlash.enabled = lit;
            if (lit) flashesLeft--;
        }

        void UpdateFlash()
        {
            if (!hurtFlash || (!flashLit && flashesLeft <= 0)) return;
            // Chep khung hinh dang chay de lop sang luon khop dang nhan vat.
            hurtFlash.sprite = visual.sprite;
            hurtFlash.flipX = visual.flipX;
            flashTimer -= Time.unscaledDeltaTime;
            if (flashTimer > 0f) return;
            if (flashLit) SetFlashLit(false);
            else if (flashesLeft > 0) SetFlashLit(true);
        }

        void StopFlash()
        {
            flashesLeft = 0;
            flashLit = false;
            if (hurtFlash) hurtFlash.enabled = false;
        }

        /// <summary>
        /// Roi vao ho / bay: mat mau, man hinh toi nhanh, hien lai o cho dung an toan cuoi cung.
        /// Het mau thi chet nhu binh thuong.
        /// </summary>
        public void TakeHazardHit(int damage)
        {
            if (dead || hazardRespawning) return;

            if (health.Damage(damage)) { StartHitStop(); Sfx.Play(SfxId.PlayerHurt); }
            if (health.IsDead)
            {
                Die();
                return;
            }

            hazardRespawning = true;
            if (actionAnimation) actionAnimation.Cancel();
            Freeze();
            if (RespawnManager.Instance)
                RespawnManager.Instance.HazardRespawn(() => controller.Respawn(safePosition), FinishHazard);
            else
            {
                controller.Respawn(safePosition);
                FinishHazard();
            }
        }

        void FinishHazard()
        {
            Unfreeze();
            hazardRespawning = false;
            groundedTimer = 0f;
        }

        void Update()
        {
            TrackSafeGround();
            UpdateFlash();
            UpdateInvulnerable();
        }

        /// <summary>Dem bat tu bang thoi gian game (dung trong luc hitstop). Nhu Hollow Knight: khong nhap nhay.</summary>
        void UpdateInvulnerable()
        {
            if (invulnerableTimer > 0f) invulnerableTimer -= Time.deltaTime;
        }

        void ClearInvulnerable() => invulnerableTimer = 0f;

        /// <summary>Nho vi tri moi khi da dung vung tren dat du lau (khong tinh luc bi danh bat ra).</summary>
        void TrackSafeGround()
        {
            if (dead || hazardRespawning || !controller.IsGrounded || controller.IsHurt)
            {
                groundedTimer = 0f;
                return;
            }
            // Dung sat mep (mot ben chan lo lung) hoac tren vat di chuyen thi khong tinh la cho an toan:
            // hoi sinh o day se truot xuong vuc.
            if (!OnSolidGround())
            {
                groundedTimer = 0f;
                return;
            }
            groundedTimer += Time.deltaTime;
            if (groundedTimer >= safeGroundTime) safePosition = transform.position;
        }

        [Tooltip("Cho an toan phai co dat ca hai ben chan, du them bao nhieu (unit) tinh tu mep than.")]
        [SerializeField] float safeEdgeMargin = 0.25f;

        readonly RaycastHit2D[] safeHits = new RaycastHit2D[4];

        /// <summary>Ca hai ben chan (mo rong safeEdgeMargin) deu cham dat dung yen (khong phai thang may, Dripfang...).</summary>
        bool OnSolidGround()
        {
            var col = GetComponent<Collider2D>();
            if (!col) return true;
            var b = col.bounds;
            int mask = LayerMask.GetMask("Ground");
            return SolidBelow(new Vector2(b.min.x - safeEdgeMargin, b.min.y + 0.1f), mask)
                && SolidBelow(new Vector2(b.max.x + safeEdgeMargin, b.min.y + 0.1f), mask);
        }

        bool SolidBelow(Vector2 origin, int mask)
        {
            var hit = Physics2D.Raycast(origin, Vector2.down, 0.45f, mask);
            if (!hit.collider) return false;
            var rb = hit.collider.attachedRigidbody;
            return !rb || rb.bodyType == RigidbodyType2D.Static;
        }

        void Die()
        {
            dead = true;
            StopFlash();
            ClearInvulnerable();
            // Roi het Shards thanh tui o cho an toan gan nhat (khong roi giua ho gai).
            ShardPool.DropOnDeath(safePosition);

            // Dung hinh tai cho trong luc man hinh toi dan.
            Freeze();

            if (RespawnManager.Instance && !RespawnManager.Instance.IsRespawning)
            {
                if (actionAnimation) actionAnimation.PlayDeath(RespawnManager.Instance.DeathFadeDuration);
                RespawnManager.Instance.Respawn(Revive, RevealRespawn, FinishRespawn);
            }
            else
            {
                Revive();
                FinishRespawn();
            }
        }

        void Revive()
        {
            StopFlash();
            controller.Respawn(checkpoint);
            safePosition = checkpoint;
            groundedTimer = 0f;
            health.Refill();
        }

        void RevealRespawn()
        {
            if (actionAnimation) actionAnimation.PlayRespawn(
                RespawnManager.Instance ? RespawnManager.Instance.RespawnFadeDuration : 0.5f);
        }

        void FinishRespawn()
        {
            if (actionAnimation) actionAnimation.Cancel();
            Unfreeze();
            dead = false;
        }

        /// <summary>Dung hinh tai cho: tat dieu khien, chem va vat ly.</summary>
        void Freeze()
        {
            controller.enabled = false;
            if (attack) attack.enabled = false;
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }

        void Unfreeze()
        {
            body.simulated = true;
            controller.enabled = true;
            if (attack) attack.enabled = true;
        }

        // Dung chung HitStop voi don chem de hai ben khong gianh nhau Time.timeScale.
        void StartHitStop() => HitStop.Request(hitStopTime);
    }
}
