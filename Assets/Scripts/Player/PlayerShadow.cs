using System;
using LuHanh.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LuHanh.Player
{
    /// <summary>
    /// Dang Shadow: diet quai -> hat nang luong -> day thanh -> giu phim Shadow (Y) -> bien hinh
    /// trong <see cref="ShadowConfig.duration"/> giay: chay nhanh hon, chem xa hon, chem tu lao toi quai.
    /// Thanh can dan khi dang Shadow; can thi tro ve binh thuong.
    ///
    /// Kha nang phai duoc mo khoa (nhat vat pham o Level2), luu bang PlayerPrefs.
    /// Nguoi choi chet: thanh ve 0, het dang Shadow (IResettable).
    /// Code khac chi doc <see cref="IsActive"/> va cac he so, khong tu bat/tat.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerShadow : MonoBehaviour, IResettable
    {
        const string UnlockKey = "LuHanh.ShadowUnlocked";

        [SerializeField] ShadowConfig config;
        [Tooltip("SpriteRenderer than nhan vat (Visual).")]
        [SerializeField] SpriteRenderer visual;
        [Tooltip("Ten action trong PlayerInput.")]
        [SerializeField] string actionName = "Shadow";
        [Tooltip("So bong mo dung chung (pool).")]
        [Min(1)] [SerializeField] int afterimagePool = 8;

        PlayerController controller;
        PlayerMovementFx movementFx;
        Rigidbody2D body;
        InputAction action;
        Color baseColor = Color.white;

        float energy;
        float hold;
        float afterimageTimer;
        SpriteRenderer[] ghosts;
        float[] ghostLife;
        int nextGhost;
        Transform ghostHolder;

        /// <summary>Nang luong 0..1 thay doi.</summary>
        public event Action<float> EnergyChanged;
        /// <summary>Tien do giu phim 0..1 (0 = da tha).</summary>
        public event Action<float> HoldChanged;
        public event Action Started;
        public event Action Ended;
        public event Action Unlocked;

        public ShadowConfig Config => config;
        public bool IsUnlocked { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsFull => energy >= config.maxEnergy - 0.001f;
        public float Energy01 => energy / config.maxEnergy;

        public float SpeedMultiplier => IsActive ? config.speedMultiplier : 1f;
        public float RangeMultiplier => IsActive ? config.attackRangeMultiplier : 1f;
        public int DamageMultiplier => IsActive ? config.damageMultiplier : 1;
        public bool HomingEnabled => IsActive && config.homingRadius > 0f;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            movementFx = GetComponent<PlayerMovementFx>();
            body = GetComponent<Rigidbody2D>();
            if (!config)
            {
                config = ScriptableObject.CreateInstance<ShadowConfig>();
                Debug.LogWarning("PlayerShadow chua duoc gan config, dang dung bo so mac dinh.", this);
            }
            if (visual) baseColor = visual.color;
            IsUnlocked = PlayerPrefs.GetInt(UnlockKey, 0) == 1;

            var playerInput = GetComponent<PlayerInput>();
            if (playerInput) action = playerInput.actions.FindAction(actionName);
            if (action == null) Debug.LogWarning($"PlayerShadow: khong tim thay action '{actionName}'.", this);

            BuildGhostPool();
            RespawnManager.Register(this);
        }

        void OnDestroy()
        {
            RespawnManager.Unregister(this);
            if (ghostHolder) Destroy(ghostHolder.gameObject);
        }

        // ---- API cho he thong khac ----

        /// <summary>Mo khoa kha nang (vat pham huong dan). Luu lai cho cac lan choi sau.</summary>
        public void Unlock()
        {
            if (IsUnlocked) return;
            IsUnlocked = true;
            PlayerPrefs.SetInt(UnlockKey, 1);
            PlayerPrefs.Save();
            Unlocked?.Invoke();
        }

        /// <summary>Cong nang luong (hat nang luong). Dang Shadow thi khong cong.</summary>
        public void AddEnergy(float amount)
        {
            if (!IsUnlocked || IsActive || amount <= 0f) return;
            SetEnergy(energy + amount);
        }

        public void FillEnergy() => AddEnergy(config.maxEnergy);

        /// <summary>Nguoi choi chet: thanh ve 0, het dang Shadow.</summary>
        public void ResetState()
        {
            if (IsActive) End();
            SetHold(0f);
            SetEnergy(0f);
        }

        [ContextMenu("Xoa mo khoa Shadow (de thu lai vat pham)")]
        void ClearUnlock()
        {
            PlayerPrefs.DeleteKey(UnlockKey);
            IsUnlocked = false;
        }

        // ---- Vong lap ----

        void Update()
        {
            UpdateGhosts();
            if (!IsUnlocked || !controller.enabled) { SetHold(0f); return; }

            if (IsActive)
            {
                SetEnergy(energy - config.maxEnergy / config.duration * Time.deltaTime);
                if (energy <= 0f) End();
                else EmitAfterimage();
                return;
            }

            bool pressed = action != null && action.IsPressed();
            if (!pressed || !IsFull) { SetHold(0f); return; }

            // So bang gia tri vua cong, khong doc lai hold: SetHold bo qua thay doi rat nho (Approximately)
            // nen hold co the ket o 0.99999994 va khong bao gio dat 1.
            float next = hold + Time.deltaTime / config.holdTime;
            SetHold(next);
            if (next >= 1f) Begin();
        }

        void Begin()
        {
            IsActive = true;
            SetHold(0f);
            controller.SetRunMultiplier(config.speedMultiplier);
            if (visual) visual.color = config.bodyTint;
            if (config.transformHitStop > 0f) HitStop.Request(config.transformHitStop);
            if (movementFx) movementFx.Punch(new Vector2(0.8f, 1.25f), Vector2.zero);
            afterimageTimer = 0f;
            Started?.Invoke();
        }

        void End()
        {
            IsActive = false;
            controller.SetRunMultiplier(1f);
            if (visual) visual.color = baseColor;
            Ended?.Invoke();
        }

        void SetEnergy(float value)
        {
            value = Mathf.Clamp(value, 0f, config.maxEnergy);
            if (Mathf.Approximately(value, energy)) return;
            energy = value;
            EnergyChanged?.Invoke(Energy01);
        }

        void SetHold(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(value, hold)) return;
            hold = value;
            HoldChanged?.Invoke(hold);
        }

        // ---- Bong mo khi di chuyen (pool) ----

        void BuildGhostPool()
        {
            if (!visual) return;
            ghostHolder = new GameObject(name + "_ShadowGhosts").transform;
            ghosts = new SpriteRenderer[afterimagePool];
            ghostLife = new float[afterimagePool];
            for (int i = 0; i < ghosts.Length; i++)
            {
                var sr = new GameObject("Ghost").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(ghostHolder, false);
                sr.sharedMaterial = visual.sharedMaterial;
                sr.enabled = false;
                ghosts[i] = sr;
            }
        }

        /// <summary>Vet bong mo doc duong lao toi (chem tu dinh vi). Vi tri la goc nhan vat.</summary>
        public void Trail(Vector2 from, Vector2 to, int count = 4)
        {
            if (ghosts == null) return;
            Vector3 offset = visual.transform.position - transform.position;
            for (int i = 0; i < count; i++)
            {
                int g = SpawnGhost();
                ghosts[g].transform.position = (Vector3)Vector2.Lerp(from, to, i / (float)count) + offset;
                ghostLife[g] = config.afterimageLife * (0.5f + 0.5f * i / count);
            }
        }

        void EmitAfterimage()
        {
            if (ghosts == null || body.linearVelocity.sqrMagnitude < 4f) return;
            afterimageTimer -= Time.deltaTime;
            if (afterimageTimer > 0f) return;
            afterimageTimer = config.afterimageInterval;
            SpawnGhost();
        }

        /// <summary>Lay bong ke tiep trong pool, chep khung hinh hien tai. Tra ve chi so.</summary>
        int SpawnGhost()
        {
            int i = nextGhost;
            var g = ghosts[i];
            ghostLife[i] = config.afterimageLife;
            nextGhost = (nextGhost + 1) % ghosts.Length;
            var t = visual.transform;
            g.transform.SetPositionAndRotation(t.position, t.rotation);
            g.transform.localScale = t.lossyScale;
            g.sprite = visual.sprite;
            g.flipX = visual.flipX;
            g.sortingLayerID = visual.sortingLayerID;
            g.sortingOrder = visual.sortingOrder - 2;
            g.color = config.afterimageColor;
            g.enabled = true;
            return i;
        }

        void UpdateGhosts()
        {
            if (ghosts == null) return;
            for (int i = 0; i < ghosts.Length; i++)
            {
                if (ghostLife[i] <= 0f) continue;
                ghostLife[i] -= Time.deltaTime;
                if (ghostLife[i] <= 0f) { ghosts[i].enabled = false; continue; }
                var c = config.afterimageColor;
                c.a *= ghostLife[i] / config.afterimageLife;
                ghosts[i].color = c;
            }
        }
    }
}
