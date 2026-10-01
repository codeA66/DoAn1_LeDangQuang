using System.Collections;
using LuHanh.Combat;
using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Mau nguoi choi kieu Hollow Knight: moi don mat 1 mask, khung hinh khung lai mot
    /// thoang, bi bat lui, bat tu + nhap nhay mot luc. Het mask thi hoi sinh o checkpoint.
    /// So mau nam o <see cref="Health"/>; lop nay lo phan phan ung.
    /// </summary>
    [RequireComponent(typeof(PlayerController), typeof(Health))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Bi danh trung")]
        [Tooltip("Thoi gian bat tu sau khi trung don (Hollow Knight ~1.3s).")]
        [SerializeField] float invulnerableTime = 1.3f;
        [Tooltip("Khung hinh dung lai bao lau luc trung don (giay thuc).")]
        [SerializeField] float hitStopTime = 0.1f;
        [Tooltip("Van toc bat ra: X theo huong ra xa nguon don, Y huong len.")]
        [SerializeField] Vector2 knockbackVelocity = new Vector2(8f, 8f);
        [Tooltip("Mat dieu khien bao lau sau khi bi bat ra.")]
        [SerializeField] float hurtTime = 0.2f;

        [Header("Nhap nhay khi bat tu")]
        [Tooltip("Sprite cua nhan vat (object con Visual).")]
        [SerializeField] SpriteRenderer sprite;
        [SerializeField] float blinkInterval = 0.1f;

        Health health;
        PlayerController controller;
        PlayerAttack attack;
        Rigidbody2D body;

        float invulnerableTimer;
        bool dead;
        Vector2 checkpoint;
        Coroutine hitStopRoutine;

        public Health Health => health;
        public bool IsInvulnerable => invulnerableTimer > 0f;

        void Awake()
        {
            health = GetComponent<Health>();
            controller = GetComponent<PlayerController>();
            attack = GetComponent<PlayerAttack>();
            body = GetComponent<Rigidbody2D>();
            if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();

            // Chua co ghe/checkpoint: hoi sinh tai cho bat dau man.
            checkpoint = transform.position;
        }

        /// <summary>Doi diem hoi sinh (ghe ngoi, cua man...).</summary>
        public void SetCheckpoint(Vector2 position) => checkpoint = position;

        public void TakeHit(HitInfo hit)
        {
            if (dead || IsInvulnerable) return;
            if (!health.Damage(hit.damage)) return;

            StartHitStop();

            if (health.IsDead)
            {
                Die();
                return;
            }

            invulnerableTimer = invulnerableTime;
            float side = hit.direction.x != 0f ? Mathf.Sign(hit.direction.x) : (controller.FacingRight ? -1f : 1f);
            controller.Hurt(new Vector2(side * knockbackVelocity.x, knockbackVelocity.y), hurtTime);
        }

        void Update()
        {
            if (invulnerableTimer > 0f)
            {
                invulnerableTimer -= Time.deltaTime;
                // Tat/bat xen ke theo nhip blinkInterval, het bat tu thi luon hien.
                if (sprite) sprite.enabled = invulnerableTimer <= 0f || Mathf.FloorToInt(invulnerableTimer / blinkInterval) % 2 == 0;
            }
        }

        void Die()
        {
            dead = true;
            invulnerableTimer = 0f;
            if (sprite) sprite.enabled = true;

            // Dung hinh tai cho trong luc man hinh toi dan.
            controller.enabled = false;
            if (attack) attack.enabled = false;
            body.linearVelocity = Vector2.zero;
            body.simulated = false;

            if (RespawnManager.Instance) RespawnManager.Instance.Respawn(Revive);
            else Revive();
        }

        void Revive()
        {
            body.simulated = true;
            controller.enabled = true;
            if (attack) attack.enabled = true;
            controller.Respawn(checkpoint);
            health.Refill();
            dead = false;
        }

        void StartHitStop()
        {
            if (hitStopTime <= 0f) return;
            if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
            hitStopRoutine = StartCoroutine(HitStop());
        }

        IEnumerator HitStop()
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(hitStopTime);
            Time.timeScale = 1f;
            hitStopRoutine = null;
        }

        void OnDisable()
        {
            // Bi tat giua luc dung hinh thi khong de game dung mai.
            if (hitStopRoutine != null) Time.timeScale = 1f;
        }
    }
}
