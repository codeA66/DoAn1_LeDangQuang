using LuHanh.Core.FSM;
using LuHanh.Player.States;
using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Hieu ung dash kieu Nine Sols: luc bat dau dash de lai 1 bong tai cho, bong tan dan thanh
    /// khoi (shader SpriteDissolve + particle khoi phun tu hinh bong), kem chum vet toc do bat
    /// nguoc huong dash va bui duoi chan neu dang dung dat. Pool san, khong Instantiate khi choi.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerDashFx : MonoBehaviour
    {
        [Header("Bong tai cho")]
        [Tooltip("SpriteRenderer cua nhan vat de chep khung hinh.")]
        public SpriteRenderer source;
        [Tooltip("Material dung shader LuHanh/SpriteDissolve.")]
        public Material ghostMaterial;
        public Color color = new Color(0.37f, 0.97f, 1f, 0.6f);
        [Tooltip("Giay de bong tan het.")]
        public float dissolveTime = 0.4f;
        [Tooltip("Do tan luc bong vua hien (0 = nguyen hinh dac, de bi loe sang 1 frame).")]
        [Range(0f, 1f)] public float startDissolve = 0.15f;
        [Tooltip("Order cua bong so voi nhan vat (am = nam sau).")]
        public int orderOffset = -1;
        [Min(1)] public int poolSize = 3;

        [Header("Particle (World space)")]
        [Tooltip("Khoi phun quanh than bong (Shape = Box om than, dat tai vi tri bong).")]
        public ParticleSystem smoke;
        public int smokeCount = 10;
        [Tooltip("Van toc khoi nguoc huong dash.")]
        public float smokeBackSpeed = 1.5f;
        [Tooltip("Vet toc do (Stretched Billboard), ban nguoc huong dash.")]
        public ParticleSystem burst;
        public int burstCount = 8;
        [Tooltip("Bui duoi chan, chi phat khi dash luc dang dung dat.")]
        public ParticleSystem groundDust;
        public int groundDustCount = 8;

        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
        static readonly int WindId = Shader.PropertyToID("_Wind");
        static readonly int SpriteUVRectId = Shader.PropertyToID("_SpriteUVRect");

        PlayerController controller;
        Rigidbody2D body;
        MaterialPropertyBlock block;
        Transform holder;
        SpriteRenderer[] ghosts;
        float[] life;
        Vector4[] winds;
        Vector4[] spriteUVRects;
        int next;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            body = GetComponent<Rigidbody2D>();
            block = new MaterialPropertyBlock();

            // Bong nam ngoai nhan vat de dung yen trong the gioi khi nhan vat lao di.
            holder = new GameObject(name + "_DashGhosts").transform;
            ghosts = new SpriteRenderer[poolSize];
            life = new float[poolSize];
            winds = new Vector4[poolSize];
            spriteUVRects = new Vector4[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var sr = new GameObject("Ghost").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(holder, false);
                sr.sharedMaterial = ghostMaterial;
                sr.enabled = false;
                ghosts[i] = sr;
            }
        }

        void OnDestroy()
        {
            if (holder) Destroy(holder.gameObject);
        }

        void OnEnable() => controller.StateChanged += OnStateChanged;
        void OnDisable() => controller.StateChanged -= OnStateChanged;

        void OnStateChanged(IState from, IState to)
        {
            if (!(to is DashState)) return;
            // Huong dash da duoc DashState.Enter dat vao van toc truoc khi bao doi trang thai.
            Vector2 dir = body.linearVelocity.sqrMagnitude > 0.01f
                ? body.linearVelocity.normalized
                : new Vector2(Mathf.Sign(source.transform.lossyScale.x), 0f);
            var ghost = SpawnGhost(dir);
            EmitSmoke(ghost, dir);
            EmitBurst(dir);
            if (groundDust && controller.IsGrounded) groundDust.Emit(groundDustCount);
        }

        SpriteRenderer SpawnGhost(Vector2 dir)
        {
            int i = next;
            next = (next + 1) % ghosts.Length;
            var g = ghosts[i];

            var t = source.transform;
            // Gio thoi nguoc huong dash, hoi boc len. Doi sang khong gian UV: nhan vat lat bang
            // scale am nen truc x cua UV cung lat theo.
            Vector2 wind = -dir + Vector2.up * 0.3f;
            wind.x *= Mathf.Sign(t.lossyScale.x);
            winds[i] = wind.normalized;
            g.transform.SetPositionAndRotation(t.position, t.rotation);
            g.transform.localScale = t.lossyScale;
            g.sprite = source.sprite;
            // Luu UV cua chinh khung nay: source co the doi animation khi bong van dang tan.
            var uv = UnityEngine.Sprites.DataUtility.GetOuterUV(g.sprite);
            spriteUVRects[i] = new Vector4(uv.x, uv.y, uv.z - uv.x, uv.w - uv.y);
            g.flipX = source.flipX;
            g.sortingLayerID = source.sortingLayerID;
            g.sortingOrder = source.sortingOrder + orderOffset;
            g.enabled = true;
            life[i] = dissolveTime;
            Apply(i, startDissolve);
            return g;
        }

        void EmitSmoke(SpriteRenderer ghost, Vector2 dir)
        {
            if (!smoke) return;
            smoke.transform.position = ghost.transform.position;
            var vel = smoke.velocityOverLifetime;
            vel.x = new ParticleSystem.MinMaxCurve(-dir.x * smokeBackSpeed);
            smoke.Emit(smokeCount);
        }

        void EmitBurst(Vector2 dir)
        {
            if (!burst) return;
            // Shape Edge ban theo truc +Y cuc bo: xoay +Y ve nguoc huong dash. Render Stretched
            // Billboard nen vet tu dai theo van toc.
            var back = -dir;
            burst.transform.SetPositionAndRotation(
                source.transform.position + Vector3.up * 0.9f,
                Quaternion.Euler(0f, 0f, Mathf.Atan2(back.y, back.x) * Mathf.Rad2Deg - 90f));
            burst.Emit(burstCount);
        }

        void LateUpdate()
        {
            for (int i = 0; i < ghosts.Length; i++)
            {
                if (life[i] <= 0f) continue;
                life[i] -= Time.deltaTime;
                if (life[i] <= 0f)
                {
                    ghosts[i].enabled = false;
                    continue;
                }
                Apply(i, Mathf.Lerp(startDissolve, 1f, 1f - life[i] / dissolveTime));
            }
        }

        void Apply(int i, float dissolve)
        {
            block.SetColor(ColorId, color);
            block.SetFloat(DissolveId, dissolve);
            block.SetVector(WindId, winds[i]);
            block.SetVector(SpriteUVRectId, spriteUVRects[i]);
            ghosts[i].SetPropertyBlock(block);
        }
    }
}
