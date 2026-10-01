using System.Collections.Generic;
using LuHanh.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LuHanh.Player
{
    /// <summary>
    /// Don chem thuong kieu Hollow Knight (phim J), chay song song voi FSM di chuyen:
    /// vua chay, vua nhay van chem duoc. Giu W chem len, tren khong giu S chem xuong;
    /// chem xuong trung thi nay len (pogo), chem ngang trung thi bi day lui nhe.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAttack : MonoBehaviour
    {
        public enum Direction { Side, Up, Down }

        [SerializeField] PlayerAttackConfig config;
        [Tooltip("SpriteRenderer vet chem (object con). De trong thi khong hien vet chem.")]
        [SerializeField] SpriteRenderer slashVisual;
        [Tooltip("Cac khung vet chem phat lan luot: quet toi -> day du -> thon lai -> tan. De trong thi dung sprite dang gan va chi lam mo.")]
        [SerializeField] Sprite[] slashFrames;

        PlayerController controller;
        Rigidbody2D body;

        float lastPressedTime = float.NegativeInfinity;
        float cooldownTimer;
        float activeTimer;
        float slashTimer;
        Direction currentDirection;
        bool facingRightAtStart;
        bool reactedThisSwing;

        readonly Collider2D[] hits = new Collider2D[8];
        readonly HashSet<IDamageable> hitThisSwing = new();
        ContactFilter2D filter;

        public bool IsAttacking => activeTimer > 0f;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            body = GetComponent<Rigidbody2D>();

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<PlayerAttackConfig>();
                Debug.LogWarning("PlayerAttack chua duoc gan config, dang dung bo so mac dinh.", this);
            }

            // Muc tieu co the la trigger (nguoi choi di xuyen qua) nen phai tinh ca trigger.
            filter = new ContactFilter2D { useLayerMask = true, layerMask = config.hitLayers, useTriggers = true };

            if (slashVisual) slashVisual.enabled = false;
        }

        void Update()
        {
            // Input Actions chua co action Attack nen doc J thang tu ban phim, giong Space va Shift.
            var kb = Keyboard.current;
            if (kb != null && kb.jKey.wasPressedThisFrame) lastPressedTime = Time.time;

            if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

            bool buffered = Time.time - lastPressedTime <= config.inputBuffer;
            if (buffered && cooldownTimer <= 0f && !controller.IsDashing)
            {
                lastPressedTime = float.NegativeInfinity;
                StartSwing();
            }

            UpdateSlashVisual();
        }

        void FixedUpdate()
        {
            if (activeTimer <= 0f) return;
            activeTimer -= Time.fixedDeltaTime;
            DetectHits();
        }

        void StartSwing()
        {
            cooldownTimer = config.cooldown;
            activeTimer = config.activeTime;
            slashTimer = config.slashVisualTime;
            facingRightAtStart = controller.FacingRight;
            reactedThisSwing = false;
            hitThisSwing.Clear();

            if (controller.MoveY == 1) currentDirection = Direction.Up;
            else if (controller.MoveY == -1 && !controller.IsGrounded) currentDirection = Direction.Down;
            else currentDirection = Direction.Side;

            ShowSlash();
            DetectHits();   // trung ngay trong frame bam, khong doi buoc vat ly
        }

        void GetArea(out Vector2 centre, out Vector2 size)
        {
            float side = facingRightAtStart ? 1f : -1f;
            Vector2 offset;
            switch (currentDirection)
            {
                case Direction.Up: offset = config.upOffset; size = config.upSize; break;
                case Direction.Down: offset = config.downOffset; size = config.downSize; break;
                default: offset = config.sideOffset; size = config.sideSize; break;
            }
            centre = (Vector2)transform.position + new Vector2(offset.x * side, offset.y);
        }

        void DetectHits()
        {
            GetArea(out var centre, out var size);
            int count = Physics2D.OverlapBox(centre, size, 0f, filter, hits);

            for (int i = 0; i < count; i++)
            {
                var target = hits[i].GetComponentInParent<IDamageable>();
                if (target == null || !hitThisSwing.Add(target)) continue;   // moi muc tieu trung 1 lan / nhat

                target.TakeHit(new HitInfo
                {
                    damage = config.damage,
                    direction = HitDirection(),
                    knockback = config.knockback,
                    source = gameObject
                });
                ReactToHit();
            }
        }

        Vector2 HitDirection()
        {
            switch (currentDirection)
            {
                case Direction.Up: return Vector2.up;
                case Direction.Down: return Vector2.down;
                default: return facingRightAtStart ? Vector2.right : Vector2.left;
            }
        }

        /// <summary>Phan luc len nguoi choi - chi mot lan moi nhat du trung nhieu muc tieu.</summary>
        void ReactToHit()
        {
            if (reactedThisSwing) return;
            reactedThisSwing = true;

            var v = body.linearVelocity;
            if (currentDirection == Direction.Down)
            {
                v.y = config.pogoSpeed > 0f ? config.pogoSpeed : controller.Config.jumpSpeed;
            }
            else if (currentDirection == Direction.Side)
            {
                v.x = (facingRightAtStart ? -1f : 1f) * config.recoilSpeed;
            }
            body.linearVelocity = v;
        }

        // ---- Vet chem ----

        void ShowSlash()
        {
            if (!slashVisual) return;
            var t = slashVisual.transform;
            // Object cha tu lat khi nhin trai nen chi can xoay len/xuong.
            float dirAngle = currentDirection == Direction.Up ? 90f : currentDirection == Direction.Down ? -90f : 0f;
            if (config.slashCentredOnHitArea)
            {
                GetArea(out var centre, out _);
                t.position = new Vector3(centre.x, centre.y, t.position.z);
                t.localRotation = Quaternion.Euler(0f, 0f, dirAngle);
                slashVisual.flipX = false;
            }
            else
            {
                t.localPosition = new Vector3(config.slashAnchor.x, config.slashAnchor.y, t.localPosition.z);
                t.localRotation = Quaternion.Euler(0f, 0f, config.slashBaseAngle + dirAngle);
                slashVisual.flipX = config.slashFlipX;
            }
            t.localScale = Vector3.one;
            if (slashFrames != null && slashFrames.Length > 0) slashVisual.sprite = slashFrames[0];
            slashVisual.enabled = true;
            slashVisual.color = Color.white;
        }

        void UpdateSlashVisual()
        {
            if (!slashVisual || !slashVisual.enabled) return;
            slashTimer -= Time.deltaTime;
            if (slashTimer <= 0f) { slashVisual.enabled = false; return; }

            float k = 1f - slashTimer / Mathf.Max(0.001f, config.slashVisualTime);   // 0 -> 1
            if (slashFrames != null && slashFrames.Length > 0)
            {
                // Do mo da ve san trong tung khung nen chi can doi khung.
                int i = Mathf.Min(slashFrames.Length - 1, Mathf.FloorToInt(k * slashFrames.Length));
                slashVisual.sprite = slashFrames[i];
            }
            else
            {
                var c = slashVisual.color;
                c.a = 1f - k * k;
                slashVisual.color = c;
            }
            slashVisual.transform.localScale = Vector3.one * (1f + config.slashGrow * k);
        }

        void OnDrawGizmosSelected()
        {
            if (config == null) return;
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
            float side = Application.isPlaying ? (controller && controller.FacingRight ? 1f : -1f) : 1f;
            void Box(Vector2 o, Vector2 s) =>
                Gizmos.DrawWireCube(transform.position + new Vector3(o.x * side, o.y, 0f), s);
            Box(config.sideOffset, config.sideSize);
            Box(config.upOffset, config.upSize);
            Box(config.downOffset, config.downSize);
        }
    }
}
