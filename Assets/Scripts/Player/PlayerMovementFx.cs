using LuHanh.Core.FSM;
using LuHanh.Player.States;
using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Hieu ung nhay kieu Hollow Knight: bui luc bat nhay, bui luc tiep dat (roi cang manh
    /// bui cang nhieu), va co gian sprite. Chi nghe su kien doi trang thai, khong dung vao FSM.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerMovementFx : MonoBehaviour
    {
        [Header("Particle")]
        public ParticleSystem jumpDust;
        public ParticleSystem landDust;
        [Tooltip("Toc do roi (unit/giay) ung voi luong bui va do bep toi da.")]
        public float hardLandingSpeed = 20f;
        public int minLandParticles = 4;
        public int maxLandParticles = 16;

        [Header("Bui khi chay - moi buoc chan hat len mot cum")]
        public ParticleSystem runDust;
        [Tooltip("Duoi ti le toc do nay (so voi maxRun) thi khong ra bui.")]
        [Range(0f, 1f)] public float runDustMinSpeedRatio = 0.3f;
        [Tooltip("Giay giua hai buoc khi vua du nguong toc do.")]
        public float runStepIntervalSlow = 0.35f;
        [Tooltip("Giay giua hai buoc khi chay het toc. Nho hon = buoc day hon, nhieu bui hon.")]
        public float runStepIntervalFast = 0.22f;
        [Tooltip("So dam trong mot cum (min, max - ca hai deu tinh).")]
        public Vector2Int runPuffsPerStep = new Vector2Int(2, 3);
        [Tooltip("Cum bui sinh ra lui ve sau got bao xa.")]
        public float runDustBackOffset = 0.3f;
        [Tooltip("Van toc hat ra phia sau (min, max).")]
        public Vector2 runDustKickBack = new Vector2(0.4f, 1.4f);
        [Tooltip("Van toc boc len (min, max).")]
        public Vector2 runDustKickUp = new Vector2(0.15f, 0.6f);

        [Header("Co gian")]
        [Tooltip("Object con chua SpriteRenderer + Animator. Pivot o chan nen co gian tu chan len.")]
        public Transform visual;
        public Vector2 jumpStretch = new Vector2(0.8f, 1.2f);
        public Vector2 landSquash = new Vector2(1.25f, 0.75f);
        [Tooltip("Toc do tro ve scale 1. Lon = tro lai nhanh.")]
        public float recoverSpeed = 12f;

        PlayerController controller;
        Rigidbody2D body;
        float lastFallSpeed;
        bool running;
        float runStepTimer;
        Vector3 visualBasePos;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            body = GetComponent<Rigidbody2D>();
            if (visual) visualBasePos = visual.localPosition;
        }

        /// <summary>Co gian + nhich than mot thoang (vd luc vung kiem), tu tro ve nhu khi tiep dat.
        /// <paramref name="offset"/> tinh theo truc cuc bo cua nhan vat: x duong = phia truoc.</summary>
        public void Punch(Vector2 scale, Vector2 offset)
        {
            SetScale(scale);
            if (visual) visual.localPosition = visualBasePos + (Vector3)offset;
        }

        void OnEnable() => controller.StateChanged += OnStateChanged;
        void OnDisable() => controller.StateChanged -= OnStateChanged;

        // Nho toc do roi cua buoc vat ly truoc: luc bao tiep dat thi va cham da triet van toc ve 0.
        void FixedUpdate()
        {
            if (body.linearVelocity.y < 0f) lastFallSpeed = -body.linearVelocity.y;
        }

        void OnStateChanged(IState from, IState to)
        {
            bool wasAir = from is JumpState || from is FallState || from is DashState;
            bool nowGround = to is IdleState || to is RunState;
            running = to is RunState;

            if (to is JumpState)
            {
                if (jumpDust) jumpDust.Play();
                SetScale(jumpStretch);
            }
            else if (wasAir && nowGround)
            {
                float t = Mathf.Clamp01(lastFallSpeed / hardLandingSpeed);
                if (landDust) landDust.Emit(Mathf.RoundToInt(Mathf.Lerp(minLandParticles, maxLandParticles, t)));
                SetScale(Vector2.Lerp(Vector2.one, landSquash, t));
                lastFallSpeed = 0f;
            }
        }

        // Kieu Hollow Knight: khong ra bui deu lien tuc (thanh hang cham) ma moi buoc chan hat
        // len mot cum may ngay sau got. Chay cang nhanh buoc cang day -> cang nhieu bui.
        void Update()
        {
            if (!runDust) return;
            // Ve 0 de buoc dau tien sau khi bat dau chay co bui ngay.
            if (!running || !controller.IsGrounded) { runStepTimer = 0f; return; }

            float vx = body.linearVelocity.x;
            float ratio = Mathf.Abs(vx) / Mathf.Max(0.01f, controller.Config.maxRun);
            if (ratio < runDustMinSpeedRatio) return;

            runStepTimer -= Time.deltaTime;
            if (runStepTimer > 0f) return;

            float t = Mathf.InverseLerp(runDustMinSpeedRatio, 1f, ratio);
            runStepTimer = Mathf.Lerp(runStepIntervalSlow, runStepIntervalFast, t);
            EmitRunStep(vx);
        }

        void EmitRunStep(float vx)
        {
            float back = -Mathf.Sign(vx);
            int count = Random.Range(runPuffsPerStep.x, runPuffsPerStep.y + 1);
            for (int i = 0; i < count; i++)
            {
                var p = new ParticleSystem.EmitParams
                {
                    position = runDust.transform.position + new Vector3(back * runDustBackOffset, 0f, 0f),
                    applyShapeToPosition = true,
                    velocity = new Vector3(back * Random.Range(runDustKickBack.x, runDustKickBack.y),
                                           Random.Range(runDustKickUp.x, runDustKickUp.y), 0f)
                };
                runDust.Emit(p, 1);
            }
        }

        void SetScale(Vector2 s)
        {
            if (visual) visual.localScale = new Vector3(s.x, s.y, 1f);
        }

        void LateUpdate()
        {
            if (!visual) return;
            // Tro ve (1,1) muot, khong phu thuoc FPS.
            float k = 1f - Mathf.Exp(-recoverSpeed * Time.deltaTime);
            visual.localScale = Vector3.Lerp(visual.localScale, Vector3.one, k);
            visual.localPosition = Vector3.Lerp(visual.localPosition, visualBasePos, k);
        }
    }
}
