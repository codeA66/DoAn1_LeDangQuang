using System.Collections;
using System.Collections.Generic;
using LuHanh.Combat;
using LuHanh.Core;
using LuHanh.Core.FSM;
using LuHanh.Effects;
using LuHanh.Level;
using LuHanh.UI;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Boss Rootwraith (màn 2), đánh theo kiểu Grimm trong Hollow Knight:
    /// - Mỗi đòn: tan thành khói -> hiện ở vị trí mới -> báo đòn (mắt lóe) -> đánh -> khoảng hở -> đòn sau.
    /// - Lao chéo từ trên cao xuống chỗ người chơi rồi trượt ngang trên sàn.
    /// - Bắn đạn ngang 2 làn cao/thấp từ mép phòng, đạn lượn sóng.
    /// - Gai trồi khắp sàn chừa khe trống; giai đoạn 2 thêm đợt lệch nửa ô.
    /// - Còn 50% máu: bay lên giữa phòng, bất tử, bắn đạn tỏa tròn nhiều đợt, rồi đánh nhanh hơn.
    /// Boss lơ lửng (gravity 0), vị trí transform = chân. Sàn đấu = độ cao lúc đặt trong scene,
    /// biên phòng = Room chứa điểm đặt. Logic chạy bằng chuỗi lệnh (IEnumerator) bước theo deltaTime của FSM
    /// nên ngủ / buff / hồi sinh đều đúng như quái khác.
    /// </summary>
    public sealed class RootwraithController : EnemyBase, IDamageable
    {
        [SerializeField] RootwraithConfig config;
        [SerializeField] RootwraithFx fx;
        [Tooltip("Tên boss hiện lúc xuất hiện (tùy chọn).")]
        [SerializeField] BossTitleCard title;
        [Tooltip("Mép trái / phải / trần của đấu trường (tọa độ thế giới). Để 0 thì lấy theo Room chứa boss.")]
        [SerializeField] float arenaLeft, arenaRight, arenaTop;

        protected override EnemyConfigBase BaseConfig => config;
        protected override IState InitialState => run;

        ActionState run;
        Collider2D playerBody;
        IDamageable target;
        Rect arena;
        float floorY;
        bool dead, hidden, invulnerable, diving, phase2, phase2Pending;
        float deathTimer;
        int lastAttack = -1;

        // Bộ chạy chuỗi lệnh: yield return float = chờ N giây, null = sang frame sau, IEnumerator = chạy chuỗi con.
        readonly Stack<IEnumerator> stack = new();
        float wait;
        float dt;

        Transform world;
        readonly List<Orb> orbs = new();
        readonly List<Spike> spikes = new();

        class Orb { public SpriteRenderer sr; public Vector2 pos, vel; public float baseY, age; public bool wave, active; }
        class Spike { public SpriteRenderer body, warn; public float x, age; public bool hit, burst, active, harmless; }

        Vector2 Feet => body ? body.position : (Vector2)transform.position;
        Vector2 Centre => bodyCollider && bodyCollider.enabled ? (Vector2)bodyCollider.bounds.center : Feet + Vector2.up * 1.3f;
        Vector2 Head => Feet + new Vector2(0.15f * FacingSign, 2.25f);
        float Rest => phase2 ? config.restPhase2 : config.restPhase1;

        // ---------- Vòng đời ----------

        protected override void Awake()
        {
            if (!config || !bodyCollider || !sprite)
            {
                Debug.LogError("Rootwraith thiếu Config, Body Collider hoặc Visual.", this);
                enabled = false;
                return;
            }
            world = new GameObject("Rootwraith_Attacks").transform;
            base.Awake();
            body.gravityScale = 0f;
        }

        protected override void Start()
        {
            base.Start();
            if (Player)
            {
                playerBody = Player.GetComponent<Collider2D>();
                target = Player.GetComponent<IDamageable>();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (world) Destroy(world.gameObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (config) StartBrain();
        }

        void OnDisable()
        {
            CameraShake.Hold(0f); // tắt rung liên tục nếu đang dở màn xuất hiện
            ClearAttacks();
            if (fx) fx.Clear();
        }

        protected override void BuildStates(StateMachine machine)
        {
            run = new ActionState(tick: TickRun, fixedTick: FixedTickRun);
        }

        void StartBrain()
        {
            var room = Room.At(StartPosition);
            arena = room ? room.Bounds : new Rect(StartPosition.x - 16f, StartPosition.y - 1f, 32f, 18f);
            floorY = StartPosition.y;
            if (arenaRight > arenaLeft)
                arena = Rect.MinMaxRect(arenaLeft, floorY - 1f, arenaRight, arenaTop > floorY ? arenaTop : arena.yMax);
            stack.Clear();
            wait = 0f;
            stack.Push(Brain());
        }

        protected override void OnResetState()
        {
            dead = diving = phase2 = phase2Pending = invulnerable = false;
            deathTimer = 0f;
            lastAttack = -1;
            CameraShake.Hold(0f);
            ClearAttacks();
            if (fx) fx.Clear();
            body.gravityScale = 0f;
            body.simulated = true;
            bodyCollider.enabled = true;
            ReapplyIgnorePlayer();
            SetHidden(false);
            if (gameObject.activeInHierarchy) StartBrain();
        }

        protected override void OnHurt(HitInfo hit) { } // boss không khựng/lùi khi bị chém (giống Hollow Knight)

        public new void TakeHit(HitInfo hit)
        {
            if (dead || hidden || invulnerable) return;
            base.TakeHit(hit);
            if (!dead && !phase2 && !phase2Pending && health.Current > 0 &&
                health.Current <= Mathf.CeilToInt(health.Max * config.phase2At))
                phase2Pending = true;
        }
        void IDamageable.TakeHit(HitInfo hit) => TakeHit(hit);

        protected override bool OnDeath(HitInfo hit)
        {
            if (dead) return true;
            dead = true;
            stack.Clear();
            diving = false;
            body.linearVelocity = Vector2.zero;
            bodyCollider.enabled = false;
            ClearAttacks();
            if (fx) { fx.Trail(Vector2.zero, Vector2.right, false); fx.HideEyeFlare(); fx.DeathBurst(Centre); }
            Play("Death");
            CameraShake.Kick(config.deathShake, 0.8f);
            HitStop.Request(0.18f);
            deathTimer = 1.6f;
            return true;
        }

        // ---------- FSM ----------

        void TickRun(float deltaTime)
        {
            dt = deltaTime;
            if (dead)
            {
                deathTimer -= deltaTime;
                if (deathTimer <= 0f) gameObject.SetActive(false);
                return;
            }
            Step();
            TickSpikes(deltaTime);
        }

        void FixedTickRun(float deltaTime)
        {
            if (dead) return;
            TickOrbs(deltaTime);
            if (!hidden && bodyCollider.enabled && Overlaps(bodyCollider.bounds))
                Damage(diving ? config.diveDamage : config.contactDamage, Centre);
        }

        void Step()
        {
            if (wait > 0f) { wait -= dt; return; }
            while (stack.Count > 0)
            {
                var top = stack.Peek();
                if (!top.MoveNext()) { stack.Pop(); continue; }
                switch (top.Current)
                {
                    case IEnumerator child: stack.Push(child); continue;
                    case float seconds: wait = seconds; return;
                    default: return; // null: sang frame sau
                }
            }
        }

        // ---------- Não boss ----------

        IEnumerator Brain()
        {
            yield return Intro();
            while (true)
            {
                if (phase2Pending) yield return PhaseShift();
                int pick;
                do pick = Random.Range(0, 3); while (pick == lastAttack);
                lastAttack = pick;
                switch (pick)
                {
                    case 0: yield return Dive(); break;
                    case 1: yield return Volley(); break;
                    default: yield return Pillars(); break;
                }
                if (phase2Pending) continue;
                yield return Idle(Rest);
            }
        }

        /// <summary>
        /// Màn xuất hiện 3 nhịp: tụ lực (tàn đỏ + khói hút về giữa, rung tăng dần, mắt lóe trong khói)
        /// -> bùng nổ (hiện ra, khựng hình, sóng lớn, vòng gai trang trí) -> gào thét (rung liên tục, vòng sóng) -> tên boss.
        /// </summary>
        IEnumerator Intro()
        {
            SetHidden(true);
            invulnerable = true;
            Teleport(StartPosition);
            Vector2 home = StartPosition;
            Vector2 core = home + Vector2.up * 1.4f;

            // 1. Tụ lực
            float gather = config.introGatherTime;
            for (float t = 0f; t < gather; t += dt)
            {
                float k = t / gather;
                if (fx)
                {
                    fx.Gather(core, Mathf.Lerp(9f, 5f, k), k);
                    if (k > 0.55f) fx.EyeFlare(home + new Vector2(0.15f * FacingSign, 2.25f), (k - 0.55f) / 0.45f);
                }
                CameraShake.Hold(Mathf.Lerp(0.02f, config.introShake * 0.5f, k * k));
                yield return null;
            }
            CameraShake.Hold(0f);
            if (fx) fx.HideEyeFlare();

            // 2. Bùng nổ
            SetHidden(false);
            FacePlayer();
            Play("Teleport_In");
            HitStop.Request(0.12f);
            CameraShake.Kick(config.introShake, 0.6f);
            if (fx) { fx.Smoke(core, 2.4f); fx.Impact(home, 1.6f); fx.Ring(core, 9f, 0.55f); fx.Ring(core, 5f, 0.35f); }
            SpawnSpikeRing();
            yield return config.teleportInTime;

            // 3. Gào thét
            Play("Phase_Shift");
            float roar = config.introRoarTime, nextRing = 0f;
            for (float t = 0f; t < roar; t += dt)
            {
                CameraShake.Hold(config.introShake * 0.35f * (1f - t / roar));
                if (t >= nextRing) { nextRing += 0.28f; if (fx) fx.Ring(core, 6.5f, 0.4f); }
                yield return null;
            }
            CameraShake.Hold(0f);
            invulnerable = false;

            Play("Idle");
            if (title)
            {
                title.Show();
                yield return Idle(title.Duration);
            }
            else yield return Idle(0.6f);
        }

        /// <summary>Vòng gai rễ trồi quanh boss lúc xuất hiện - chỉ để nhìn, không gây sát thương.</summary>
        void SpawnSpikeRing()
        {
            for (int i = 1; i <= 4; i++)
            {
                float d = 0.9f + i * 0.85f;
                SpawnSpike(StartPosition.x - d, true, i * 0.04f);
                SpawnSpike(StartPosition.x + d, true, i * 0.04f);
            }
        }

        IEnumerator Idle(float seconds)
        {
            Play("Idle");
            for (float t = 0f; t < seconds; t += dt)
            {
                FacePlayer();
                yield return null;
            }
        }

        IEnumerator TeleportTo(Vector2 feet)
        {
            Play("Teleport_Out");
            if (fx) fx.Smoke(Centre);
            yield return config.teleportOutTime;
            SetHidden(true);
            yield return config.hiddenTime;
            Teleport(feet);
            SetHidden(false);
            FacePlayer();
            Play("Teleport_In");
            if (fx) fx.Smoke(Centre);
            yield return config.teleportInTime;
        }

        IEnumerator Windup(string clip, float seconds)
        {
            Play(clip);
            for (float t = 0f; t < seconds; t += dt)
            {
                FacePlayer();
                if (fx) fx.EyeFlare(Head, t / seconds);
                yield return null;
            }
            if (fx) fx.HideEyeFlare();
        }

        // ---------- Đòn 1: Lao chéo ----------

        IEnumerator Dive()
        {
            int count = phase2 ? Mathf.Max(1, config.phase2DiveCount) : 1;
            for (int i = 0; i < count; i++)
            {
                float px = PlayerFeet.x;
                float side = Random.value < 0.5f ? -1f : 1f;
                float x = px - side * config.diveStartSide;
                if (x < arena.xMin + 2f || x > arena.xMax - 2f) x = px + side * config.diveStartSide;
                x = Mathf.Clamp(x, arena.xMin + 2f, arena.xMax - 2f);
                float y = Mathf.Min(floorY + config.diveStartHeight, arena.yMax - 3f);
                yield return TeleportTo(new Vector2(x, y));
                yield return Windup("Dive_Windup", config.diveWindup);

                Vector2 dir = PlayerFeet - Feet;
                if (dir.sqrMagnitude < 0.01f) dir = Vector2.down;
                dir.Normalize();
                if (dir.y > -0.35f) { dir.y = -0.35f; dir.Normalize(); }
                Face(dir.x);
                Play("Dive");
                diving = true;
                if (fx) fx.Ring(Centre, 2f, 0.2f);
                for (float t = 0f; t < 1.6f; t += dt)
                {
                    body.linearVelocity = dir * config.diveSpeed;
                    if (fx) fx.Trail(Centre, dir, true);
                    if (Feet.y <= floorY + 0.05f) break;
                    yield return null;
                }

                // Chạm sàn
                body.linearVelocity = Vector2.zero;
                Teleport(new Vector2(Feet.x, floorY));
                if (fx) { fx.Trail(Vector2.zero, dir, false); fx.Impact(Feet, 1.2f); }
                CameraShake.Kick(config.diveLandShake, 0.35f);
                HitStop.Request(config.hitStopOnLand);
                Play("Dive_Land");

                float sign = Mathf.Sign(dir.x == 0f ? FacingSign : dir.x);
                float travelled = 0f;
                while (travelled < config.slideDistance)
                {
                    float k = travelled / config.slideDistance;
                    float speed = config.slideSpeed * (1f - k * 0.8f);
                    float before = Feet.x;
                    body.linearVelocity = new Vector2(sign * speed, 0f);
                    if (fx && Random.value < 0.6f) fx.SpikeBurst(Feet);
                    yield return null;
                    float moved = Mathf.Abs(Feet.x - before);
                    if (moved < 0.001f && travelled > 0.2f) break; // va tường
                    travelled += Mathf.Max(moved, speed * dt * 0.5f);
                }
                body.linearVelocity = Vector2.zero;
                diving = false;
                yield return Idle(i < count - 1 ? 0.15f : config.recoverTime);
            }
        }

        // ---------- Đòn 2: Bắn đạn ngang ----------

        IEnumerator Volley()
        {
            float cx = arena.center.x;
            float x = PlayerFeet.x > cx ? arena.xMin + 2.5f : arena.xMax - 2.5f;
            yield return TeleportTo(new Vector2(x, floorY));
            yield return Windup("Volley_Windup", config.volleyWindup);
            Play("Volley_Fire");
            int n = phase2 ? config.volleyCountPhase2 : config.volleyCountPhase1;
            for (int i = 0; i < n; i++)
            {
                float lane = i % 2 == 0 ? config.orbLowLane : config.orbHighLane;
                Vector2 from = new(Feet.x + FacingSign * 1.1f, floorY + lane);
                SpawnOrb(from, new Vector2(FacingSign * config.orbSpeed, 0f), true);
                if (fx) fx.Muzzle(from, FacingSign);
                CameraShake.Kick(0.12f, 0.1f);
                yield return config.volleyInterval;
            }
            yield return Idle(config.recoverTime);
        }

        // ---------- Đòn 3: Gai trồi ----------

        IEnumerator Pillars()
        {
            yield return TeleportTo(new Vector2(arena.center.x, Mathf.Min(floorY + 5.5f, arena.yMax - 3f)));
            yield return Windup("Pillar_Windup", config.pillarWindup);
            Play("Pillar_Slam");
            CameraShake.Kick(config.spikeShake * 0.6f, 0.25f);
            if (fx) fx.Ring(Centre, 3f, 0.3f);
            SpawnSpikeWave(0f, 0);
            float total = config.spikeWarn + config.spikeRise + config.spikeHold + config.spikeRetract;
            if (phase2)
            {
                yield return config.secondWaveDelay;
                SpawnSpikeWave(config.spikeSpacing * 0.5f, 1);
                yield return total;
            }
            else yield return total;
            yield return Idle(config.recoverTime * 0.6f);
        }

        // ---------- Chuyển giai đoạn ----------

        IEnumerator PhaseShift()
        {
            phase2Pending = false;
            phase2 = true;
            invulnerable = true;
            yield return TeleportTo(new Vector2(arena.center.x, Mathf.Min(floorY + config.phaseShiftHeight, arena.yMax - 3f)));
            Play("Phase_Shift");
            CameraShake.Kick(config.phaseShake * 2f, 0.6f);
            HitStop.Request(0.1f);
            if (fx) { fx.Ring(Centre, 7f, 0.6f); fx.Smoke(Centre, 1.5f); }
            float next = 0.4f, offset = 0f;
            for (float t = 0f; t < config.phaseShiftDuration; t += dt)
            {
                if (t >= next)
                {
                    next += config.burstInterval;
                    offset += 360f / config.burstCount * 0.5f;
                    for (int i = 0; i < config.burstCount; i++)
                    {
                        float a = (offset + i * 360f / config.burstCount) * Mathf.Deg2Rad;
                        var d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                        SpawnOrb(Centre + d * 0.8f, d * config.burstSpeed, false);
                    }
                    if (fx) fx.Ring(Centre, 2.5f, 0.25f);
                    CameraShake.Kick(config.phaseShake, 0.2f);
                }
                yield return null;
            }
            invulnerable = false;
            yield return Idle(0.3f);
        }

        // ---------- Đạn ----------

        void SpawnOrb(Vector2 pos, Vector2 vel, bool wave)
        {
            Orb o = null;
            foreach (var x in orbs) if (!x.active) { o = x; break; }
            if (o == null)
            {
                o = new Orb();
                if (fx && fx.OrbSprite)
                {
                    var go = new GameObject("Orb");
                    go.transform.SetParent(world, false);
                    o.sr = go.AddComponent<SpriteRenderer>();
                    o.sr.sprite = fx.OrbSprite;
                    o.sr.sortingLayerName = fx.SortingLayer;
                    o.sr.sortingOrder = fx.SortingOrder + 3;
                }
                else if (fx) o.sr = fx.MakeEnergy(world, "Orb", 0, 2.6f);
                orbs.Add(o);
            }
            o.active = true;
            o.pos = pos; o.vel = vel; o.baseY = pos.y; o.age = 0f; o.wave = wave;
            if (o.sr)
            {
                o.sr.enabled = true;
                o.sr.transform.position = pos;
                if (!(fx && fx.OrbSprite)) o.sr.transform.localScale = Vector3.one * config.orbRadius * 3.2f;
            }
        }

        void TickOrbs(float step)
        {
            foreach (var o in orbs)
            {
                if (!o.active) continue;
                o.age += step;
                o.pos += o.vel * step;
                if (o.wave) o.pos.y = o.baseY + config.orbWaveAmp * Mathf.Sin(o.age * config.orbWaveFreq * Mathf.PI * 2f);
                if (o.sr) o.sr.transform.position = o.pos;
                bool gone = o.age > 6f || !arena.Contains(o.pos) ||
                            Physics2D.OverlapCircle(o.pos, config.orbRadius * 0.5f, config.groundLayers);
                if (!gone && playerBody && DistanceToPlayer(o.pos) < config.orbRadius)
                {
                    Damage(config.orbDamage, o.pos);
                    gone = true;
                }
                if (!gone) continue;
                o.active = false;
                if (o.sr) o.sr.enabled = false;
                if (fx) fx.OrbPop(o.pos);
            }
        }

        // ---------- Gai ----------

        void SpawnSpikeWave(float offset, int waveIndex)
        {
            int idx = 0;
            for (float x = arena.xMin + 0.8f + offset; x <= arena.xMax - 0.8f; x += config.spikeSpacing, idx++)
            {
                int gapEvery = Mathf.Max(2, config.spikeGapEvery);
                if ((idx + waveIndex * (gapEvery / 2 + 1)) % gapEvery == gapEvery - 1) continue; // khe trống
                SpawnSpike(x);
            }
        }

        /// <param name="harmless">Gai trang trí: bỏ qua cảnh báo, trồi ngay, không gây sát thương.</param>
        /// <param name="delay">Trồi trễ (giây) để tạo nhịp lan ra.</param>
        void SpawnSpike(float x, bool harmless = false, float delay = 0f)
        {
            Spike s = null;
            foreach (var v in spikes) if (!v.active) { s = v; break; }
            if (s == null)
            {
                s = new Spike();
                if (fx)
                {
                    var go = new GameObject("Spike");
                    go.transform.SetParent(world, false);
                    s.body = go.AddComponent<SpriteRenderer>();
                    s.body.sprite = fx.SpikeSprite;
                    if (fx.AlphaMaterial) s.body.sharedMaterial = fx.AlphaMaterial;
                    s.body.sortingLayerName = fx.SortingLayer;
                    s.body.sortingOrder = fx.SortingOrder;
                    s.warn = fx.MakeEnergy(world, "SpikeWarn", 1, 2.2f);
                }
                spikes.Add(s);
            }
            s.active = true; s.x = x; s.hit = harmless; s.burst = false; s.harmless = harmless;
            s.age = harmless ? config.spikeWarn - delay : 0f; // gai trang trí bỏ qua phần cảnh báo
            if (s.body) { s.body.enabled = false; s.body.transform.position = new Vector3(x, floorY, 0f); }
            if (s.warn)
            {
                s.warn.enabled = !harmless;
                s.warn.transform.position = new Vector3(x, floorY + 0.35f, 0f);
                s.warn.transform.localScale = new Vector3(config.spikeWidth * 1.6f, 0.7f, 1f);
            }
        }

        void TickSpikes(float step)
        {
            float warn = config.spikeWarn, rise = config.spikeRise, hold = config.spikeHold, retract = config.spikeRetract;
            bool anyBurstThisFrame = false;
            foreach (var s in spikes)
            {
                if (!s.active) continue;
                s.age += step;
                float t = s.age;
                float h;
                if (t < warn)
                {
                    h = 0f;
                    if (s.warn) s.warn.color = new Color(1f, 1f, 1f, 0.35f + 0.65f * Mathf.PingPong(t * 6f, 1f));
                }
                else if (t < warn + rise) h = (t - warn) / rise;
                else if (t < warn + rise + hold) h = 1f;
                else if (t < warn + rise + hold + retract) h = 1f - (t - warn - rise - hold) / retract;
                else
                {
                    s.active = false;
                    if (s.body) s.body.enabled = false;
                    if (s.warn) s.warn.enabled = false;
                    continue;
                }

                if (t >= warn && !s.burst)
                {
                    s.burst = true;
                    anyBurstThisFrame = true;
                    if (s.warn) s.warn.enabled = false;
                    if (fx) fx.SpikeBurst(new Vector2(s.x, floorY));
                }
                if (s.body)
                {
                    s.body.enabled = h > 0f;
                    s.body.transform.localScale = new Vector3(config.spikeWidth, config.spikeHeight * h, 1f);
                }
                if (h > 0.3f && !s.hit && t < warn + rise + hold)
                {
                    var area = new Bounds(new Vector3(s.x, floorY + config.spikeHeight * h * 0.5f, 0f),
                        new Vector3(config.spikeWidth * 0.7f, config.spikeHeight * h, 4f));
                    if (Overlaps(area)) { s.hit = true; Damage(config.spikeDamage, new Vector2(s.x, floorY)); }
                }
            }
            if (anyBurstThisFrame) CameraShake.Kick(config.spikeShake, 0.3f);
        }

        void ClearAttacks()
        {
            foreach (var o in orbs) { o.active = false; if (o.sr) o.sr.enabled = false; }
            foreach (var s in spikes)
            {
                s.active = false;
                if (s.body) s.body.enabled = false;
                if (s.warn) s.warn.enabled = false;
            }
        }

        // ---------- Tiện ích ----------

        Vector2 PlayerFeet => playerBody ? new Vector2(playerBody.bounds.center.x, playerBody.bounds.min.y)
            : Player ? (Vector2)Player.position : Feet;

        void FacePlayer() { if (Player) Face(Player.position.x - Feet.x); }

        void Teleport(Vector2 feet)
        {
            body.position = feet;
            transform.position = feet;
            body.linearVelocity = Vector2.zero;
        }

        void SetHidden(bool value)
        {
            hidden = value;
            if (sprite) sprite.enabled = !value;
            if (bodyCollider) bodyCollider.enabled = !value && !dead;
            if (!value) ReapplyIgnorePlayer();
        }

        void Play(string clip)
        {
            if (animator) animator.Play(clip, 0, 0f);
        }

        bool PlayerHittable => target != null && playerBody && playerBody.enabled &&
            (!playerBody.attachedRigidbody || playerBody.attachedRigidbody.simulated);

        bool Overlaps(Bounds area)
        {
            if (!PlayerHittable) return false;
            area.center = new Vector3(area.center.x, area.center.y, playerBody.bounds.center.z);
            area.size = new Vector3(area.size.x, area.size.y, 4f);
            return area.Intersects(playerBody.bounds);
        }

        float DistanceToPlayer(Vector2 p)
        {
            Vector2 closest = playerBody.bounds.ClosestPoint(p);
            return Vector2.Distance(closest, p);
        }

        void Damage(int amount, Vector2 from)
        {
            if (!PlayerHittable || dead) return;
            target.TakeHit(new HitInfo
            {
                damage = amount,
                source = gameObject,
                direction = new Vector2(playerBody.bounds.center.x >= from.x ? 1f : -1f, 0f),
                reaction = config.hitReaction,
                knockback = 0f
            });
        }
    }
}
