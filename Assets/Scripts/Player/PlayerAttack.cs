using System.Collections.Generic;
using LuHanh.Combat;
using LuHanh.Core;
using LuHanh.Effects;
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
        [System.Serializable]
        public class SlashSet
        {
            [Tooltip("Cac khung cua mot nhat chem, ve huong sang phai.")]
            public Sprite[] frames;
        }

        [Tooltip("Cac bo vet chem ngang. Chem lien tiep (trong slashAlternateWindow) thi lay bo ke tiep, het thi quay lai bo dau.")]
        [SerializeField] SlashSet[] sideSlashes;
        [Tooltip("Cac bo vet chem len (xoay 90 do), xoay vong nhu tren.")]
        [SerializeField] SlashSet[] upSlashes;
        [Tooltip("Cac bo vet chem xuong (xoay -90 do). De trong thi dung bo chem len.")]
        [SerializeField] SlashSet[] downSlashes;

        PlayerController controller;
        Rigidbody2D body;
        PlayerMovementFx movementFx;
        Sprite[] activeFrames;
        float lastSwingTime = float.NegativeInfinity;
        int comboIndex;

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
        HitEffect hitEffect;

        public bool IsAttacking => activeTimer > 0f;
        void Awake()
        {
            controller = GetComponent<PlayerController>();
            body = GetComponent<Rigidbody2D>();
            movementFx = GetComponent<PlayerMovementFx>();

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<PlayerAttackConfig>();
                Debug.LogWarning("PlayerAttack chua duoc gan config, dang dung bo so mac dinh.", this);
            }

            // Muc tieu co the la trigger (nguoi choi di xuyen qua) nen phai tinh ca trigger.
            filter = new ContactFilter2D { useLayerMask = true, layerMask = config.hitLayers, useTriggers = true };

            if (slashVisual) slashVisual.enabled = false;

            // Tao san mot ban hieu ung trung don dung chung (khong gan vao Player de khong bi lat theo).
            if (config.hitEffectPrefab) hitEffect = Instantiate(config.hitEffectPrefab);
        }

        void Update()
        {
            if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

            bool buffered = Time.time - lastPressedTime <= config.inputBuffer;
            if (buffered && cooldownTimer <= 0f && !controller.IsDashing)
            {
                lastPressedTime = float.NegativeInfinity;
                StartSwing();
            }

            UpdateSlashVisual();
        }

        /// <summary>Goi tu PlayerInput (action Attack). Chi ghi lai thoi diem bam, Update quyet dinh chem.</summary>
        public void onAttack(InputAction.CallbackContext callback)
        {
            // UnityEvent van goi khi component bi tat (vd luc chet) - bo qua de khong chem ngay khi hoi sinh.
            if (callback.started && enabled) lastPressedTime = Time.time;
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

            // Chem lien tiep thi doi chieu quet de chuoi chem trong nhu combo.
            comboIndex = Time.time - lastSwingTime <= config.slashAlternateWindow ? comboIndex + 1 : 0;
            lastSwingTime = Time.time;

            PlaySwingPose();
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

                var direction = HitDirection();
                target.TakeHit(new HitInfo
                {
                    damage = config.damage,
                    direction = direction,
                    knockback = config.knockback,
                    source = gameObject
                });
                // Diem tren collider muc tieu gan tam vung chem nhat = cho loe sang.
                if (hitEffect) hitEffect.Play(hits[i].ClosestPoint(centre), direction);
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

            // Khung hinh khung lai mot thoang: luoi kiem nhu "cam vao" muc tieu.
            HitStop.Request(config.hitStopTime);

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

        // Chua co animation chem nen gia dong tac bang co gian + nhich than ve huong chem.
        void PlaySwingPose()
        {
            if (!movementFx) return;
            switch (currentDirection)
            {
                case Direction.Up: movementFx.Punch(config.upSwingSquash, new Vector2(0f, config.swingLunge)); break;
                case Direction.Down: movementFx.Punch(config.downSwingSquash, new Vector2(0f, -config.swingLunge)); break;
                default: movementFx.Punch(config.sideSwingSquash, new Vector2(config.swingLunge, 0f)); break;
            }
        }

        // ---- Vet chem ----

        Sprite[] PickFrames()
        {
            var sets = currentDirection == Direction.Up ? upSlashes
                : currentDirection == Direction.Down ? (downSlashes != null && downSlashes.Length > 0 ? downSlashes : upSlashes)
                : sideSlashes;
            if (sets == null || sets.Length == 0) return null;
            return sets[comboIndex % sets.Length].frames;
        }

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
                slashVisual.flipY = false;
            }
            else
            {
                // Day ra xa theo huong chem; X cuc bo da la phia truoc vi object cha tu lat.
                Vector2 push = currentDirection == Direction.Up ? new Vector2(0f, config.slashUpPush)
                    : currentDirection == Direction.Down ? new Vector2(0f, -config.slashDownPush)
                    : new Vector2(config.slashSidePush, 0f);
                Vector2 pos = config.slashAnchor + push;
                t.localPosition = new Vector3(pos.x, pos.y, t.localPosition.z);
                t.localRotation = Quaternion.Euler(0f, 0f, config.slashBaseAngle + dirAngle);
                slashVisual.flipX = config.slashFlipX;
                // Xoay +90 dua nua tren cua anh ra sau lung; lat doc de phan cong van huong ve phia truoc.
                slashVisual.flipY = currentDirection == Direction.Up;
            }
            t.localScale = Vector3.one;
            activeFrames = PickFrames();
            if (activeFrames != null && activeFrames.Length > 0) slashVisual.sprite = activeFrames[0];
            slashVisual.enabled = true;
            slashVisual.color = Color.white;
        }

        void UpdateSlashVisual()
        {
            if (!slashVisual || !slashVisual.enabled) return;
            slashTimer -= Time.deltaTime;
            if (slashTimer <= 0f) { slashVisual.enabled = false; return; }

            float k = 1f - slashTimer / Mathf.Max(0.001f, config.slashVisualTime);   // 0 -> 1
            if (activeFrames != null && activeFrames.Length > 0)
            {
                // Do mo da ve san trong tung khung nen chi can doi khung.
                int i = Mathf.Min(activeFrames.Length - 1, Mathf.FloorToInt(k * activeFrames.Length));
                slashVisual.sprite = activeFrames[i];
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
