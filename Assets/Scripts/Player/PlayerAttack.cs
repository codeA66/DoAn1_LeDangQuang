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
            [Tooltip("Vung co mau that cua vet chem (hop chung moi khung), unit, tinh tu pivot sprite, truoc lat/xoay. " +
                     "Bam chuot phai PlayerAttack > 'Do vung chem tu sprite vet chem' de tu tinh. Rong = dung hop trong config.")]
            public Rect hitArea;
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
        PlayerActionAnimation actionAnimation;
        PlayerShadow shadow;
        float rangeScale = 1f;      // chot luc bam chem: dang Shadow thi lon hon
        readonly Collider2D[] homingHits = new Collider2D[16];
        readonly RaycastHit2D[] homingCast = new RaycastHit2D[8];
        int groundMask;
        Collider2D homingTarget;   // muc tieu cua nhat chem tu dinh vi hien tai
        const float HomingReachSlack = 0.8f;
        Sprite[] activeFrames;
        SlashSet activeSet;
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
            actionAnimation = GetComponent<PlayerActionAnimation>();
            shadow = GetComponent<PlayerShadow>();
            groundMask = LayerMask.GetMask("Ground");

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

            homingTarget = null;
            rangeScale = shadow ? shadow.RangeMultiplier : 1f;
            // Dang Shadow: lao toi quai gan nhat va chem theo huong cua no; khong co quai thi chem thuong.
            bool homed = shadow && shadow.HomingEnabled && TryHoming();
            // Danh thuong: ngam ho tro - chi quay mat + chon huong, khong lao toi.
            // Nguoi choi dang giu W/S thi ton trong huong do, khong ngam ho tro.
            if (!homed && controller.MoveY == 0 && config.aimAssistRadius > 0f && FindTarget(config.aimAssistRadius, out var near))
                AimAt(near, body.position);
            else if (!homed)
            {
                if (controller.MoveY == 1) currentDirection = Direction.Up;
                else if (controller.MoveY == -1 && !controller.IsGrounded) currentDirection = Direction.Down;
                else currentDirection = Direction.Side;
            }

            // Chem lien tiep thi doi chieu quet de chuoi chem trong nhu combo.
            comboIndex = Time.time - lastSwingTime <= config.slashAlternateWindow ? comboIndex + 1 : 0;
            lastSwingTime = Time.time;

            PlaySwingPose();
            ShowSlash();
            DetectHits();   // trung ngay trong frame bam, khong doi buoc vat ly
        }

        /// <summary>
        /// Chem tu dinh vi (dang Shadow): tim muc tieu gan nhat trong homingRadius, lao thang toi
        /// cach no homingStopDistance (dung lai truoc tuong), quay mat va chon huong chem theo vi tri muc tieu.
        /// </summary>
        bool TryHoming()
        {
            var cfg = shadow.Config;
            Vector2 chest = body.position + Vector2.up * config.slashAnchor.y;
            if (!FindTarget(cfg.homingRadius, out var best)) return false;

            Vector2 to = (Vector2)best.bounds.center - chest;
            float dist = to.magnitude;
            Vector2 dir = dist > 0.001f ? to / dist : Vector2.right;
            float travel = Mathf.Max(0f, dist - cfg.homingStopDistance);

            // Dung truoc tuong; bo qua platform 1 chieu (di xuyen duoc).
            int hits = Physics2D.CircleCast(chest, 0.3f, dir,
                new ContactFilter2D { useLayerMask = true, layerMask = groundMask }, homingCast, travel);
            for (int i = 0; i < hits; i++)
            {
                if (homingCast[i].collider.GetComponent<PlatformEffector2D>()) continue;
                travel = Mathf.Min(travel, Mathf.Max(0f, homingCast[i].distance - 0.05f));
            }

            // Tuong chan giua duong, toi noi van ngoai tam: khong lao, chem thuong tai cho.
            if (dist - travel > cfg.homingStopDistance + HomingReachSlack) return false;

            Vector2 from = body.position;
            Vector2 to2 = from + dir * travel;
            body.position = to2;
            transform.position = to2;
            body.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
            shadow.Trail(from, to2);

            homingTarget = best;
            AimAt(best, to2, allowDownOnGround: true);
            return true;
        }

        /// <summary>Muc tieu (IDamageable) gan nguc nhat trong ban kinh.</summary>
        bool FindTarget(float radius, out Collider2D best)
        {
            Vector2 chest = body.position + Vector2.up * config.slashAnchor.y;
            int count = Physics2D.OverlapCircle(chest, radius, filter, homingHits);
            best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var c = homingHits[i];
                // Chi nham quai (bo qua can gat, vat pha duoc...).
                if (!c.GetComponentInParent<LuHanh.Enemies.EnemyBase>()) continue;
                float d = ((Vector2)c.bounds.center - chest).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = c; }
            }
            return best;
        }

        /// <summary>
        /// Quay mat va chon huong chem ve phia muc tieu, tinh tu chan dang dung o <paramref name="feet"/>.
        /// Chem xuong chi khi dang tren khong (giong dieu khien thuong), tru khi vua lao toi muc tieu ben duoi.
        /// </summary>
        void AimAt(Collider2D target, Vector2 feet, bool allowDownOnGround = false)
        {
            Vector2 rel = (Vector2)target.bounds.center - (feet + Vector2.up * config.slashAnchor.y);
            if (Mathf.Abs(rel.x) > 0.05f) controller.Face(rel.x);
            facingRightAtStart = controller.FacingRight;
            if (Mathf.Abs(rel.y) > Mathf.Abs(rel.x) * 1.2f && (rel.y > 0f || allowDownOnGround || !controller.IsGrounded))
                currentDirection = rel.y > 0f ? Direction.Up : Direction.Down;
            else
                currentDirection = Direction.Side;
        }

        void GetArea(out Vector2 centre, out Vector2 size)
        {
            if (TryGetSlashArea(out centre, out size)) return;

            float side = facingRightAtStart ? 1f : -1f;
            Vector2 offset;
            switch (currentDirection)
            {
                case Direction.Up: offset = config.upOffset; size = config.upSize; break;
                case Direction.Down: offset = config.downOffset; size = config.downSize; break;
                default: offset = config.sideOffset; size = config.sideSize; break;
            }
            // Dang Shadow: vung chem to ra va day xa them theo huong chem.
            size *= rangeScale;
            if (currentDirection == Direction.Side) offset.x *= rangeScale;
            else offset.y *= rangeScale;
            centre = (Vector2)transform.position + new Vector2(offset.x * side, offset.y);
        }

        /// <summary>
        /// Vung chem khop vet chem dang hien: lay hitArea cua bo vet chem, dat qua dung vi tri/xoay/lat/phong to
        /// cua SpriteRenderer vet chem, roi lay hop bao truc. Thu nho theo slashHitShrink de chi trung phan dam.
        /// </summary>
        bool TryGetSlashArea(out Vector2 centre, out Vector2 size)
        {
            centre = size = Vector2.zero;
            if (!config.hitAreaFromSlash || !slashVisual || activeSet == null) return false;
            var r = activeSet.hitArea;
            if (r.width <= 0f || r.height <= 0f) return false;

            float fx = slashVisual.flipX ? -1f : 1f, fy = slashVisual.flipY ? -1f : 1f;
            var m = slashVisual.transform.localToWorldMatrix;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = -min;
            for (int i = 0; i < 4; i++)
            {
                var local = new Vector3((i & 1) == 0 ? r.xMin : r.xMax, (i & 2) == 0 ? r.yMin : r.yMax, 0f);
                local.x *= fx;
                local.y *= fy;
                Vector2 w = m.MultiplyPoint3x4(local);
                min = Vector2.Min(min, w);
                max = Vector2.Max(max, w);
            }
            centre = (min + max) * 0.5f;
            size = (max - min) * config.slashHitShrink;
            return true;
        }

        void DetectHits()
        {
            GetArea(out var centre, out var size);
            int count = Physics2D.OverlapBox(centre, size, 0f, filter, hits);

            for (int i = 0; i < count; i++) Hit(hits[i], centre);

            // Chem tu dinh vi: da lao toi thi muc tieu do chac chan trung (neu van trong tam voi),
            // ke ca khi no lech khoi hop chem vi dang di chuyen hoac o cheo.
            if (homingTarget && homingTarget.gameObject.activeInHierarchy)
            {
                Vector2 chest = body.position + Vector2.up * config.slashAnchor.y;
                float reach = shadow.Config.homingStopDistance + HomingReachSlack;
                if (((Vector2)homingTarget.ClosestPoint(chest) - chest).sqrMagnitude <= reach * reach)
                    Hit(homingTarget, centre);
            }
        }

        void Hit(Collider2D col, Vector2 centre)
        {
            var target = col.GetComponentInParent<IDamageable>();
            if (target == null || !hitThisSwing.Add(target)) return;   // moi muc tieu trung 1 lan / nhat

            var direction = HitDirection();
            target.TakeHit(new HitInfo
            {
                damage = config.damage,
                direction = direction,
                knockback = config.knockback,
                source = gameObject
            });
            // Diem tren collider muc tieu gan tam vung chem nhat = cho loe sang.
            if (hitEffect) hitEffect.Play(col.ClosestPoint(centre), direction);
            ReactToHit();
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
            if (actionAnimation && actionAnimation.PlayAttack(currentDirection)) return;
            if (!movementFx) return;
            switch (currentDirection)
            {
                // Goc am = chui ve phia truoc (nhan vat lat bang scale nen am luon la "ve truoc").
                case Direction.Up: movementFx.Punch(config.upSwingSquash, new Vector2(0f, config.swingLunge), config.swingLean * 0.5f); break;
                case Direction.Down: movementFx.Punch(config.downSwingSquash, new Vector2(0f, -config.swingLunge), -config.swingLean); break;
                default: movementFx.Punch(config.sideSwingSquash, new Vector2(config.swingLunge, 0f), -config.swingLean); break;
            }
        }

        // ---- Vet chem ----

        SlashSet PickSet()
        {
            var sets = currentDirection == Direction.Up ? upSlashes
                : currentDirection == Direction.Down ? (downSlashes != null && downSlashes.Length > 0 ? downSlashes : upSlashes)
                : sideSlashes;
            if (sets == null || sets.Length == 0) return null;
            return sets[comboIndex % sets.Length];
        }

#if UNITY_EDITOR
        /// <summary>
        /// Do vung co mau that (alpha > nguong) cua moi bo vet chem tu file anh goc, luu vao hitArea.
        /// Chay lai sau khi doi sprite vet chem.
        /// </summary>
        [ContextMenu("Do vung chem tu sprite vet chem")]
        public void MeasureSlashAreas()
        {
            UnityEditor.Undo.RecordObject(this, "Do vung chem");
            int n = 0;
            foreach (var sets in new[] { sideSlashes, upSlashes, downSlashes })
            {
                if (sets == null) continue;
                foreach (var set in sets) { set.hitArea = MeasureSet(set.frames); n++; }
            }
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[PlayerAttack] Da do vung chem cho {n} bo vet chem.", this);
        }

        static Rect MeasureSet(Sprite[] frames)
        {
            const float alphaThreshold = 0.25f;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = -min;
            var cache = new System.Collections.Generic.Dictionary<string, Texture2D>();
            if (frames == null) return default;
            foreach (var s in frames)
            {
                if (!s) continue;
                string path = UnityEditor.AssetDatabase.GetAssetPath(s.texture);
                if (!cache.TryGetValue(path, out var tex))
                {
                    tex = new Texture2D(2, 2);
                    tex.LoadImage(System.IO.File.ReadAllBytes(path));
                    cache[path] = tex;
                }
                // Anh goc co the lon hon texture import (Max Size): quy ve toa do anh goc.
                float k = tex.width / (float)s.texture.width;
                var rect = s.rect;
                for (int y = 0; y < rect.height; y++)
                for (int x = 0; x < rect.width; x++)
                {
                    if (tex.GetPixel(Mathf.FloorToInt((rect.x + x) * k), Mathf.FloorToInt((rect.y + y) * k)).a < alphaThreshold) continue;
                    var p = (new Vector2(x + 0.5f, y + 0.5f) - s.pivot) / s.pixelsPerUnit;
                    min = Vector2.Min(min, p);
                    max = Vector2.Max(max, p);
                }
            }
            foreach (var t in cache.Values) DestroyImmediate(t);
            return min.x > max.x ? default : Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
#endif

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
                Vector2 pos = config.slashAnchor + push * rangeScale;
                t.localPosition = new Vector3(pos.x, pos.y, t.localPosition.z);
                t.localRotation = Quaternion.Euler(0f, 0f, config.slashBaseAngle + dirAngle);
                slashVisual.flipX = config.slashFlipX;
                // Xoay +90 dua nua tren cua anh ra sau lung; lat doc de phan cong van huong ve phia truoc.
                slashVisual.flipY = currentDirection == Direction.Up;
            }
            t.localScale = Vector3.one * rangeScale;
            activeSet = PickSet();
            activeFrames = activeSet?.frames;
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
            slashVisual.transform.localScale = Vector3.one * (rangeScale * (1f + config.slashGrow * k));
        }

        void OnDrawGizmosSelected()
        {
            if (config == null) return;
            // Dang chem: ve vung chem that su (khop vet chem) mau xanh ngoc.
            if (Application.isPlaying && IsAttacking)
            {
                GetArea(out var c, out var s);
                Gizmos.color = new Color(0.27f, 0.86f, 0.77f, 0.9f);
                Gizmos.DrawWireCube(c, s);
            }
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
