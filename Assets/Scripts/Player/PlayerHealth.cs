using LuHanh.Combat;
using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Mau nguoi choi: moi don mat 1 mask, khung hinh khung lai mot thoang va bi bat lui.
    /// Khong co bat tu sau khi trung don - moi don cua quai tu lo chi trung 1 lan
    /// (dan bien mat khi trung, cu lao cua Ghoul tat hitbox sau khi trung).
    /// Het mask thi hoi sinh o checkpoint.
    /// Roi vao ho / bay thi mat 1 mask va hien lai o cho dung an toan gan nhat.
    /// So mau nam o <see cref="Health"/>; lop nay lo phan phan ung.
    /// </summary>
    [RequireComponent(typeof(PlayerController), typeof(Health))]
    public class PlayerHealth : MonoBehaviour, IDamageable, IHazardReceiver
    {
        [Header("Bi danh trung")]
        [Tooltip("Khung hinh dung lai bao lau luc trung don (giay thuc).")]
        [SerializeField] float hitStopTime = 0.1f;
        [Tooltip("Van toc bat ra: X theo huong ra xa nguon don, Y huong len.")]
        [SerializeField] Vector2 knockbackVelocity = new Vector2(8f, 8f);
        [Tooltip("Mat dieu khien bao lau sau khi bi bat ra.")]
        [SerializeField] float hurtTime = 0.2f;

        [Header("Roi vao bay / ho")]
        [Tooltip("Dung vung tren dat lien tuc bao lau thi vi tri do moi duoc nho lam cho an toan.")]
        [SerializeField] float safeGroundTime = 0.2f;

        Health health;
        PlayerController controller;
        PlayerAttack attack;
        Rigidbody2D body;

        bool dead;
        bool hazardRespawning;
        Vector2 checkpoint;
        Vector2 safePosition;
        float groundedTimer;

        public Health Health => health;

        void Awake()
        {
            health = GetComponent<Health>();
            controller = GetComponent<PlayerController>();
            attack = GetComponent<PlayerAttack>();
            body = GetComponent<Rigidbody2D>();

            // Chua co ghe/checkpoint: hoi sinh tai cho bat dau man.
            checkpoint = transform.position;
            safePosition = checkpoint;
        }

        /// <summary>Doi diem hoi sinh (ghe ngoi, cua man...).</summary>
        public void SetCheckpoint(Vector2 position) => checkpoint = position;

        public void TakeHit(HitInfo hit)
        {
            if (dead || hazardRespawning) return;
            if (!health.Damage(hit.damage)) return;

            StartHitStop();

            if (health.IsDead)
            {
                Die();
                return;
            }

            float side = hit.direction.x != 0f ? Mathf.Sign(hit.direction.x) : (controller.FacingRight ? -1f : 1f);
            controller.Hurt(new Vector2(side * knockbackVelocity.x, knockbackVelocity.y), hurtTime);
        }

        /// <summary>
        /// Roi vao ho / bay: mat mau, man hinh toi nhanh, hien lai o cho dung an toan cuoi cung.
        /// Het mau thi chet nhu binh thuong.
        /// </summary>
        public void TakeHazardHit(int damage)
        {
            if (dead || hazardRespawning) return;

            if (health.Damage(damage)) StartHitStop();
            if (health.IsDead)
            {
                Die();
                return;
            }

            hazardRespawning = true;
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

        void Update() => TrackSafeGround();

        /// <summary>Nho vi tri moi khi da dung vung tren dat du lau (khong tinh luc bi danh bat ra).</summary>
        void TrackSafeGround()
        {
            if (dead || hazardRespawning || !controller.IsGrounded || controller.IsHurt)
            {
                groundedTimer = 0f;
                return;
            }
            groundedTimer += Time.deltaTime;
            if (groundedTimer >= safeGroundTime) safePosition = transform.position;
        }

        void Die()
        {
            dead = true;

            // Dung hinh tai cho trong luc man hinh toi dan.
            Freeze();

            if (RespawnManager.Instance) RespawnManager.Instance.Respawn(Revive);
            else Revive();
        }

        void Revive()
        {
            Unfreeze();
            controller.Respawn(checkpoint);
            safePosition = checkpoint;
            groundedTimer = 0f;
            health.Refill();
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
