using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Hiệu ứng của boss Rootwraith: particle (khói dịch chuyển, tia lửa, bụi, mảnh vỡ) dựng bằng code lúc chạy,
    /// cùng các sprite năng lượng dùng shader LuHanh/RootwraithEnergy (đạn, vệt cảnh báo, vòng sóng, vệt lao).
    /// Chỉ hiển thị - sát thương do RootwraithController tính.
    /// Ảnh nhiều frame được phát theo tuổi hiệu ứng; thiếu ảnh thì giữ hình tạm.
    /// </summary>
    public class RootwraithFx : MonoBehaviour
    {
        [Header("Material")]
        [Tooltip("Material dùng shader LuHanh/RootwraithEnergy.")]
        [SerializeField] Material energyMaterial;
        [Tooltip("Material cộng sáng (LuHanh/SpriteAdditive) cho tia lửa.")]
        [SerializeField] Material additiveMaterial;
        [Tooltip("Material pha trộn thường (Sprite-Unlit-Default) cho khói tối và gai.")]
        [SerializeField] Material alphaMaterial;

        [Header("Ảnh thay thế (tùy chọn)")]
        [Tooltip("Để trống: đạn vẽ bằng shader.")]
        [SerializeField] Sprite orbSprite;
        [Tooltip("Để trống: gai vẽ bằng code.")]
        [SerializeField] Sprite spikeSprite;

        [Header("Hoạt ảnh VFX (tùy chọn)")]
        [SerializeField] Sprite[] orbFrames;
        [SerializeField, Min(1f)] float orbFps = 10f;
        [SerializeField] Sprite[] popFrames;
        [SerializeField, Min(1f)] float popFps = 12f;
        [Tooltip("00–02 trồi; giữ 02; 03–05 rút theo thời gian đòn hiện có.")]
        [SerializeField] Sprite[] spikeFrames;
        [Tooltip("Chiều cao phần hình từng frame (pixel alpha > 8), tính từ pivot đáy; cập nhật cùng bộ ảnh.")]
        [SerializeField] float[] spikeAlphaHeights;
        [Tooltip("Chiều rộng phần hình frame 02 (pixel alpha > 8).")]
        [SerializeField] float spikeAlphaWidth;
        [SerializeField] Sprite[] warnFrames;
        [SerializeField, Min(1f)] float warnFps = 8f;
        [SerializeField] Sprite[] trailFrames;
        [SerializeField, Min(1f)] float trailFps = 12f;
        [SerializeField] Sprite[] impactFrames;
        [SerializeField, Min(1f)] float impactFps = 12f;
        [Tooltip("Phát đủ frame theo life của từng lệnh Ring; không co giãn chồng với hình nở trong ảnh.")]
        [SerializeField] Sprite[] ringFrames;
        [SerializeField] Sprite[] eyeFrames;
        [SerializeField, Min(1f)] float eyeFps = 12f;
        [SerializeField] Sprite[] smokeFrames;
        [SerializeField, Min(1f)] float smokeFps = 12f;
        [SerializeField] Sprite[] deathFrames;
        [SerializeField, Min(1f)] float deathFps = 10f;
        [Tooltip("Căn mặt của DeathCore 00/01 với silhouette Death trước khi crossfade.")]
        [SerializeField] Vector2 deathCoreOffset = new(0f, 0.75f);

        [Header("Màu")]
        [SerializeField] Color hot = new(1f, 0.88f, 0.78f);          // lõi kem
        [SerializeField] Color danger = new(0.918f, 0.396f, 0.278f);  // #EA6547
        [SerializeField] Color darkRed = new(0.561f, 0.224f, 0.239f); // #8F393D
        [SerializeField] Color smoke = new(0.07f, 0.05f, 0.09f);      // gần #0C0A14

        [SerializeField] string sortingLayer = "Default";
        [SerializeField] int sortingOrder = 20;

        ParticleSystem smokePs, sparkPs, dustPs, debrisPs, emberPs;
        Sprite square, soft, spikeGen;
        SpriteRenderer trail, eyeFlare;
        readonly SpriteRenderer[] rings = new SpriteRenderer[4];
        readonly float[] ringTime = new float[4];
        readonly float[] ringLife = new float[4];
        readonly float[] ringSize = new float[4];
        readonly float[] ringAlpha = new float[4];
        int nextRing;
        bool built;
        FramePool popPool, impactPool, smokePool;
        float trailAge, eyeAge;
        SpriteMask spikeFloorMask;
        SpriteRenderer deathArt, deathGlow, deathShaderRing, deathBody;
        Material deathGlowMaterial, deathRingMaterial;
        ParticleSystem deathEmbers, deathSparks, deathDebris, deathSmoke;
        Color deathBodyColor;
        Vector2 deathPosition;
        float deathAge, deathEmberCarry;
        bool deathActive, deathRingOne, deathRingTwo, deathSmokeStarted, deathParticlesStarted;

        public bool HasOrbFrames => orbFrames != null && orbFrames.Length > 0 && orbFrames[0];
        public Sprite OrbSprite => HasOrbFrames ? orbFrames[0] : orbSprite;
        public Sprite OrbFrame(float age)
        {
            if (!HasOrbFrames) return orbSprite;
            int i = Mathf.FloorToInt(Mathf.Max(0f, age) * Mathf.Max(1f, orbFps)) % orbFrames.Length;
            return orbFrames[i] ? orbFrames[i] : orbFrames[0];
        }
        public bool HasSpikeFrames => HasFrames(spikeFrames) && spikeFrames.Length >= 6;
        public bool HasSpikeReveal => HasSpikeFrames && spikeAlphaHeights != null && spikeAlphaHeights.Length == spikeFrames.Length && spikeAlphaHeights[2] > 0f && spikeAlphaWidth > 0f;
        public bool HasWarnFrames => HasFrames(warnFrames);
        public Sprite WarnFrame(float age) => FrameAt(warnFrames, age, warnFps, true);
        public Sprite SpikeFrame(float age, float rise, float hold, float retract)
        {
            if (!HasSpikeFrames) return SpikeSprite;
            int i;
            if (age < rise) i = Mathf.Min(2, Mathf.FloorToInt(Mathf.Clamp01(age / Mathf.Max(0.001f, rise)) * 3f));
            else if (age < rise + hold) i = 2;
            else i = 3 + Mathf.Min(2, Mathf.FloorToInt(Mathf.Clamp01((age - rise - hold) / Mathf.Max(0.001f, retract)) * 3f));
            return spikeFrames[i] ? spikeFrames[i] : spikeFrames[2];
        }
        public void ConfigureSpikeMask(Rect arena, float floorY, float height)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            if (!HasSpikeReveal) return;
            Build();
            if (!spikeFloorMask)
            {
                var go = new GameObject("SpikeFloorMask");
                go.transform.SetParent(worldRoot.transform, false);
                spikeFloorMask = go.AddComponent<SpriteMask>();
                spikeFloorMask.sprite = square;
                spikeFloorMask.alphaCutoff = 0.5f;
                spikeFloorMask.isCustomRangeActive = true;
                spikeFloorMask.backSortingLayerID = UnityEngine.SortingLayer.NameToID(sortingLayer);
                spikeFloorMask.frontSortingLayerID = UnityEngine.SortingLayer.NameToID(sortingLayer);
                spikeFloorMask.frontSortingOrder = sortingOrder + 1;
                spikeFloorMask.backSortingOrder = sortingOrder - 1;
            }
            float maskHeight = height + 2f;
            spikeFloorMask.transform.position = new Vector3(arena.center.x, floorY + maskHeight * 0.5f, 0f);
            spikeFloorMask.transform.localScale = new Vector3(arena.width + 4f, maskHeight, 1f);
        }

        public void ApplySpikeReveal(SpriteRenderer renderer, float x, float floorY, float h, bool retracting, float width, float height)
        {
            if (!renderer) return;
            float desiredPixels = Mathf.Clamp01(h) * spikeAlphaHeights[2];
            int frame = 2;
            // Chọn hình đủ cao rồi kéo xuống dưới mặt sàn; mask cắt phần dư.
            // Không phóng Y theo h và không cho hình đi trước chiều cao hitbox.
            if (!retracting)
            {
                for (int i = 0; i <= 2; i++)
                    if (spikeAlphaHeights[i] >= desiredPixels) { frame = i; break; }
            }
            else
            {
                for (int i = spikeFrames.Length - 1; i >= 3; i--)
                    if (spikeAlphaHeights[i] >= desiredPixels) { frame = i; break; }
            }
            float ppu = spikeFrames[2].pixelsPerUnit;
            float scaleY = height / (spikeAlphaHeights[2] / ppu);
            float extraHeight = (spikeAlphaHeights[frame] - desiredPixels) / ppu * scaleY;
            renderer.sprite = spikeFrames[frame];
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            renderer.transform.localScale = new Vector3(width / (spikeAlphaWidth / ppu), scaleY, 1f);
            renderer.transform.position = new Vector3(x, floorY - extraHeight, 0f);
        }
        public Sprite SquareSprite { get { Build(); return square; } }
        public Sprite SpikeSprite { get { Build(); return HasSpikeFrames ? spikeFrames[0] : spikeSprite ? spikeSprite : spikeGen; } }
        public Material EnergyMaterial => energyMaterial;
        public Material AlphaMaterial => alphaMaterial;
        public string SortingLayer => sortingLayer;
        public int SortingOrder => sortingOrder;
        public Color Danger => danger;
        public Color Hot => hot;

        void Awake() => Build();

        void Build()
        {
            if (built) return;
            built = true;
            square = MakeSquare();
            soft = MakeSoft();
            spikeGen = MakeSpike();
            // Không làm con của boss: boss lật scale.x và bị tắt khi chết.
            var root = new GameObject("RootwraithFx_World").transform;
            worldRoot = root.gameObject;

            smokePs = MakePs(root, "Smoke", alphaMaterial, 120, 0.7f, 1.1f, 1.5f, 4f, 0.5f, 1.4f);
            sparkPs = MakePs(root, "Sparks", additiveMaterial, 200, 0.25f, 0.55f, 6f, 15f, 0.06f, 0.16f);
            dustPs = MakePs(root, "Dust", alphaMaterial, 80, 0.4f, 0.8f, 2f, 6f, 0.4f, 0.9f);
            debrisPs = MakePs(root, "Debris", alphaMaterial, 120, 0.5f, 0.9f, 4f, 10f, 0.08f, 0.18f);
            emberPs = MakePs(root, "Embers", additiveMaterial, 300, 0.3f, 0.7f, 0.2f, 1.2f, 0.08f, 0.2f);
            var g = sparkPs.main; g.gravityModifier = 1.6f;
            g = debrisPs.main; g.gravityModifier = 2.4f;
            g = dustPs.main; g.gravityModifier = -0.05f;
            g = smokePs.main; g.gravityModifier = -0.15f;

            trail = HasFrames(trailFrames)
                ? MakeFrameRenderer(root, "DiveTrail", trailFrames[0], sortingOrder + 1)
                : MakeEnergy(root, "DiveTrail", 3, 1f);
            trail.enabled = false;
            eyeFlare = HasFrames(eyeFrames)
                ? MakeFrameRenderer(root, "EyeFlare", eyeFrames[0], sortingOrder + 1)
                : MakeEnergy(root, "EyeFlare", 0, 3f);
            eyeFlare.enabled = false;
            for (int i = 0; i < rings.Length; i++)
            {
                rings[i] = HasFrames(ringFrames)
                    ? MakeFrameRenderer(root, "Ring" + i, ringFrames[0], sortingOrder + 1)
                    : MakeEnergy(root, "Ring" + i, 2, 2.5f);
                rings[i].enabled = false;
            }
            if (HasFrames(popFrames)) popPool = new FramePool(this, root, "OrbPop", popFrames, popFps, 32, sortingOrder + 3);
            if (HasFrames(impactFrames)) impactPool = new FramePool(this, root, "DiveImpact", impactFrames, impactFps, 4, sortingOrder);
            if (HasFrames(smokeFrames)) smokePool = new FramePool(this, root, "Teleport", smokeFrames, smokeFps, 4, sortingOrder - 1);
            if (HasFrames(deathFrames))
            {
                deathArt = MakeFrameRenderer(root, "DeathCore", deathFrames[0], sortingOrder + 7);
                deathGlow = MakeEnergy(root, "DeathCoreGlow", 0, 0.3f);
                deathGlow.sortingOrder = sortingOrder + 2;
                deathGlowMaterial = deathGlow.sharedMaterial;
                if (deathGlowMaterial) deathGlowMaterial.SetFloat("_Flicker", 0.08f);
                deathShaderRing = MakeEnergy(root, "DeathEnergyRing", 2, 1.2f);
                deathShaderRing.sortingOrder = sortingOrder - 2;
                deathRingMaterial = deathShaderRing.sharedMaterial;
                if (deathRingMaterial) deathRingMaterial.SetFloat("_Flicker", 0.05f);
                deathGlow.enabled = deathShaderRing.enabled = false;
                deathEmbers = MakePs(root, "DeathEmbers", additiveMaterial, 48, 0.4f, 0.7f, 0.2f, 0.6f, 0.025f, 0.055f);
                deathSparks = MakePs(root, "DeathSparks", additiveMaterial, 64, 0.25f, 0.55f, 2f, 5f, 0.03f, 0.08f);
                deathDebris = MakePs(root, "DeathRootDebris", alphaMaterial, 24, 0.5f, 1f, 1f, 3f, 0.06f, 0.12f);
                deathSmoke = MakePs(root, "DeathSmoke", alphaMaterial, 32, 0.45f, 0.8f, 0.2f, 0.6f, 0.3f, 0.65f);
                var deathSmokeSize = deathSmoke.sizeOverLifetime;
                deathSmokeSize.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 1.6f));
                deathSmoke.GetComponent<ParticleSystemRenderer>().sortingOrder = sortingOrder - 1;
                var debrisRenderer = deathDebris.GetComponent<ParticleSystemRenderer>();
                debrisRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                debrisRenderer.lengthScale = 2.5f;
                debrisRenderer.velocityScale = 0f;
            }
        }

        GameObject worldRoot;
        /// <summary>Da dung xong va object goc hieu ung con ton tai (Unity huy object theo thu tu bat ky khi thoat Play / doi scene).</summary>
        bool Alive => built && worldRoot;
        void OnDestroy()
        {
            if (worldRoot) Destroy(worldRoot);
            if (energyMaterial && deathGlowMaterial && deathGlowMaterial != energyMaterial) Destroy(deathGlowMaterial);
            if (energyMaterial && deathRingMaterial && deathRingMaterial != energyMaterial) Destroy(deathRingMaterial);
        }

        // ---------- API ----------

        /// <summary>Khói tối bung ra + tàn đỏ khi tan biến / hiện ra.</summary>
        public void Smoke(Vector2 pos, float size = 1f)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            smokePool?.Play(pos, size);
            Emit(smokePs, pos, (int)(28 * size), smoke, new Vector2(0.6f, 1.6f) * size, 1.2f * size);
            Emit(emberPs, pos, (int)(22 * size), danger, new Vector2(0.08f, 0.2f), 1.1f * size);
        }

        /// <summary>
        /// Tụ lực trước khi boss xuất hiện: tàn đỏ và khói từ vòng bán kính <paramref name="radius"/> hút về <paramref name="centre"/>.
        /// Gọi mỗi frame, <paramref name="k"/> 0..1 tăng dần thì càng dày.
        /// </summary>
        public void Gather(Vector2 centre, float radius, float k)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            Build();
            int n = Mathf.RoundToInt(Mathf.Lerp(1f, 6f, k));
            var p = new ParticleSystem.EmitParams();
            for (int i = 0; i < n; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                float r = radius * Random.Range(0.6f, 1f);
                float life = Random.Range(0.45f, 0.75f);
                p.position = centre + dir * r;
                p.velocity = -dir * (r / life) + Vector2.Perpendicular(dir) * Random.Range(-1.5f, 1.5f); // xoáy nhẹ
                p.startLifetime = life;
                p.startSize = Random.Range(0.08f, 0.22f);
                p.startColor = Random.value < 0.25f ? hot : danger;
                emberPs.Emit(p, 1);
            }
            if (Random.value < k * 0.6f)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                p.position = centre + dir * radius * 0.7f;
                p.velocity = -dir * radius * 0.9f;
                p.startLifetime = 0.8f;
                p.startSize = Random.Range(0.8f, 1.6f);
                p.startColor = smoke;
                smokePs.Emit(p, 1);
            }
        }

        /// <summary>Ánh mắt lóe lên báo đòn. <paramref name="k"/> 0..1 theo tiến độ báo đòn.</summary>
        public void EyeFlare(Vector2 pos, float k)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            if (!eyeFlare.enabled) eyeAge = 0f;
            eyeFlare.enabled = k > 0f && k < 1f;
            if (!eyeFlare.enabled) return;
            eyeFlare.transform.position = pos;
            bool animated = HasFrames(eyeFrames);
            float s = animated ? 1f : Mathf.Lerp(0.4f, 1.6f, k) * (1f + 0.15f * Mathf.Sin(Time.time * 40f));
            eyeFlare.transform.localScale = Vector3.one * s;
            if (animated) eyeFlare.sprite = FrameAt(eyeFrames, eyeAge, eyeFps, true);
            eyeFlare.color = new Color(1f, 1f, 1f, Mathf.Sin(k * Mathf.PI));
            if (Random.value < 0.5f) Emit(emberPs, pos + Random.insideUnitCircle * 1.2f, 1, danger, new Vector2(0.06f, 0.14f), 0.4f);
        }

        public void HideEyeFlare() { if (eyeFlare) eyeFlare.enabled = false; }

        /// <summary>Vệt lao kéo theo boss, xoay theo hướng bay.</summary>
        public void Trail(Vector2 pos, Vector2 dir, bool on)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            if (!trail.enabled) trailAge = 0f;
            trail.enabled = on;
            if (!on) return;
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bool animated = HasFrames(trailFrames);
            // Frame đã có đuôi dài và pivot mép phải: đặt đầu vệt ngay sau tâm boss.
            trail.transform.SetPositionAndRotation(animated ? pos : pos - dir.normalized * 1.6f, Quaternion.Euler(0, 0, ang));
            trail.transform.localScale = animated ? Vector3.one : new Vector3(4.5f, 1.4f, 1f);
            if (animated) trail.sprite = FrameAt(trailFrames, trailAge, trailFps, true);
            trail.color = Color.white;
            Emit(emberPs, pos + Random.insideUnitCircle * 0.6f, 3, danger, new Vector2(0.08f, 0.22f), 0.6f);
        }

        /// <summary>Chạm sàn sau cú lao: bụi, tia lửa, vòng sóng.</summary>
        public void Impact(Vector2 pos, float strength = 1f)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            impactPool?.Play(pos, strength);
            Emit(dustPs, pos, (int)(24 * strength), new Color(0.2f, 0.16f, 0.2f, 0.85f), new Vector2(0.5f, 1.1f), 1f);
            EmitCone(sparkPs, pos, (int)(30 * strength), hot, 25f, 155f);
            EmitCone(debrisPs, pos, (int)(14 * strength), new Color(0.12f, 0.09f, 0.12f), 40f, 140f);
            Ring(pos, 3.5f * strength, 0.35f);
        }

        /// <summary>Gai trồi lên: mảnh đất văng + tia lửa ở chân gai.</summary>
        public void SpikeBurst(Vector2 pos)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            EmitCone(debrisPs, pos, 5, new Color(0.14f, 0.1f, 0.12f), 60f, 120f);
            EmitCone(sparkPs, pos, 6, danger, 70f, 110f);
        }

        /// <summary>Nòng bắn: chớp sáng nhỏ.</summary>
        public void Muzzle(Vector2 pos, float dirSign)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            EmitCone(sparkPs, pos, 10, hot, dirSign > 0 ? -35f : 145f, dirSign > 0 ? 35f : 215f);
            Ring(pos, 1.2f, 0.18f);
        }

        /// <summary>Đạn vỡ.</summary>
        public void OrbPop(Vector2 pos)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            popPool?.Play(pos, 1f);
            Emit(sparkPs, pos, 8, danger, new Vector2(0.05f, 0.12f), 0.3f);
        }

        /// <summary>Vòng sóng xung kích.</summary>
        public void Ring(Vector2 pos, float size, float life, float alpha = 1f)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            int i = nextRing;
            nextRing = (nextRing + 1) % rings.Length;
            rings[i].enabled = true;
            rings[i].transform.position = pos;
            if (HasFrames(ringFrames))
            {
                rings[i].sprite = ringFrames[0];
                rings[i].transform.localScale = Vector3.one * size / Mathf.Max(0.001f, ringFrames[0].rect.width / ringFrames[0].pixelsPerUnit);
                rings[i].color = new Color(1f, 1f, 1f, alpha);
            }
            ringTime[i] = 0f; ringLife[i] = life; ringSize[i] = size;
            ringAlpha[i] = alpha;
        }

        /// <summary>Chết: co lõi, bung vương miện rễ, vòng/khói lệch nhịp rồi tàn tới 1,6 s.</summary>
        public void DeathBurst(Vector2 pos, SpriteRenderer body = null)
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            if (deathArt)
            {
                Clear();
                deathPosition = pos + deathCoreOffset;
                deathAge = deathEmberCarry = 0f;
                deathActive = true;
                deathBody = body;
                if (deathBody) deathBodyColor = deathBody.color;
                deathArt.sprite = deathFrames[0];
                deathArt.transform.position = deathPosition;
                deathArt.transform.localScale = Vector3.one;
                deathArt.color = new Color(1f, 1f, 1f, 0f);
                deathArt.enabled = true;
                deathGlow.transform.position = deathShaderRing.transform.position = deathPosition;
                EmitDeathPull();
                return;
            }
            Smoke(pos, 2.2f);
            Emit(sparkPs, pos, 60, hot, new Vector2(0.06f, 0.18f), 1.5f);
            Ring(pos, 6f, 0.6f);
            Ring(pos, 3.5f, 0.4f);
        }

        /// <summary>Dọn hết (người chơi hồi sinh / boss chết).</summary>
        public void Clear()
        {
            if (!Alive) return; // thoat Play / doi scene: object hieu ung co the da bi huy truoc
            if (!built) return;
            foreach (var ps in new[] { smokePs, sparkPs, dustPs, debrisPs, emberPs }) if (ps) ps.Clear();
            if (trail) trail.enabled = false;
            HideEyeFlare();
            foreach (var r in rings) if (r) r.enabled = false;
            popPool?.Clear();
            impactPool?.Clear();
            smokePool?.Clear();
            deathActive = deathRingOne = deathRingTwo = deathSmokeStarted = deathParticlesStarted = false;
            if (deathArt) deathArt.enabled = false;
            if (deathGlow) deathGlow.enabled = false;
            if (deathShaderRing) deathShaderRing.enabled = false;
            if (deathBody) deathBody.color = deathBodyColor;
            deathBody = null;
            foreach (var ps in new[] { deathEmbers, deathSparks, deathDebris, deathSmoke }) if (ps) ps.Clear();
        }

        void Update()
        {
            if (!Alive) return;
            TickDeath(Time.deltaTime);
            popPool?.Tick(Time.deltaTime);
            impactPool?.Tick(Time.deltaTime);
            smokePool?.Tick(Time.deltaTime);
            if (trail && trail.enabled && HasFrames(trailFrames))
            {
                trailAge += Time.deltaTime;
                trail.sprite = FrameAt(trailFrames, trailAge, trailFps, true);
            }
            if (eyeFlare && eyeFlare.enabled && HasFrames(eyeFrames))
            {
                eyeAge += Time.deltaTime;
                eyeFlare.sprite = FrameAt(eyeFrames, eyeAge, eyeFps, true);
            }
            for (int i = 0; i < rings.Length; i++)
            {
                if (!rings[i] || !rings[i].enabled) continue;
                ringTime[i] += Time.deltaTime;
                float k = ringTime[i] / ringLife[i];
                if (k >= 1f) { rings[i].enabled = false; continue; }
                if (HasFrames(ringFrames))
                {
                    int frame = Mathf.Min(ringFrames.Length - 1, Mathf.FloorToInt(k * ringFrames.Length));
                    rings[i].sprite = ringFrames[frame] ? ringFrames[frame] : ringFrames[0];
                    continue;
                }
                float e = 1f - (1f - k) * (1f - k);
                rings[i].transform.localScale = Vector3.one * Mathf.Lerp(0.2f, ringSize[i], e);
                rings[i].color = new Color(1f, 1f, 1f, (1f - k) * ringAlpha[i]);
            }
        }

        void TickDeath(float step)
        {
            if (!deathActive) return;
            deathAge += step;
            float blend = Mathf.Clamp01(deathAge / 0.16f);
            if (deathBody)
            {
                var color = deathBodyColor;
                color.a *= 1f - blend;
                deathBody.color = color;
            }
            deathArt.enabled = deathAge < deathFrames.Length / Mathf.Max(1f, deathFps);
            if (deathArt.enabled)
            {
                deathArt.sprite = FrameAt(deathFrames, deathAge, deathFps, false);
                float alpha = blend * (1f - Mathf.Clamp01((deathAge - 0.6f) / 0.2f));
                deathArt.color = new Color(1f, 1f, 1f, alpha);
            }

            deathGlow.enabled = deathAge < 0.38f;
            if (deathGlow.enabled)
            {
                float size, intensity, alpha;
                if (deathAge < 0.18f)
                {
                    float k = deathAge / 0.18f;
                    size = Mathf.Lerp(0.8f, 0.35f, k);
                    intensity = Mathf.Lerp(0.3f, 1.3f, k);
                    alpha = Mathf.Lerp(0.15f, 0.6f, k);
                }
                else if (deathAge < 0.2f)
                {
                    float k = (deathAge - 0.18f) / 0.02f;
                    size = Mathf.Lerp(0.35f, 1.2f, k);
                    intensity = Mathf.Lerp(1.3f, 1.8f, k);
                    alpha = Mathf.Lerp(0.6f, 0.55f, k);
                }
                else
                {
                    float k = Mathf.Clamp01((deathAge - 0.2f) / 0.18f);
                    size = 1.2f;
                    intensity = 1.8f * (1f - k);
                    alpha = 0.55f * (1f - k);
                }
                deathGlow.transform.localScale = Vector3.one * size;
                deathGlow.color = new Color(1f, 1f, 1f, alpha);
                if (deathGlowMaterial) deathGlowMaterial.SetFloat("_Intensity", intensity);
            }

            deathShaderRing.enabled = deathAge >= 0.2f && deathAge < 0.65f;
            if (deathShaderRing.enabled)
            {
                float k = Mathf.Clamp01((deathAge - 0.2f) / 0.45f);
                deathShaderRing.transform.localScale = Vector3.one * Mathf.Lerp(1.3f, 6f, k);
                deathShaderRing.color = new Color(1f, 1f, 1f, 0.45f * (1f - k));
                if (deathRingMaterial)
                {
                    deathRingMaterial.SetFloat("_Thickness", Mathf.Lerp(0.06f, 0.025f, k));
                    deathRingMaterial.SetFloat("_Intensity", 1.2f * (1f - k));
                }
            }
            if (!deathRingOne && deathAge >= 0.2f) { deathRingOne = true; Ring(deathPosition, 6f, 0.6f, 0.45f); }
            if (!deathRingTwo && deathAge >= 0.32f) { deathRingTwo = true; Ring(deathPosition, 4f, 0.45f, 0.3f); }
            if (!deathSmokeStarted && deathAge >= 0.28f) { deathSmokeStarted = true; smokePool?.Play(deathPosition, 1.1f, 0.4f); }
            if (!deathParticlesStarted && deathAge >= 0.2f)
            {
                deathParticlesStarted = true;
                EmitDeathOutward(deathSparks, 40, 2f, 5f, 0.03f, 0.08f, 0.25f, 0.55f, true);
                EmitDeathOutward(deathDebris, 12, 1f, 3f, 0.06f, 0.12f, 0.5f, 1f, false);
            }
            if (deathAge >= 0.35f && deathAge - step < 0.35f)
                Emit(deathSmoke, deathPosition, 20, smoke, new Vector2(0.3f, 0.65f), 0.6f);
            if (deathAge >= 0.65f && deathAge < 1.3f)
            {
                deathEmberCarry += step * 22f;
                while (deathEmberCarry >= 1f)
                {
                    deathEmberCarry -= 1f;
                    var p = new ParticleSystem.EmitParams
                    {
                        position = deathPosition + Random.insideUnitCircle.normalized * Random.Range(0.8f, 1.8f),
                        velocity = Random.insideUnitCircle.normalized * Random.Range(0.2f, 0.6f),
                        startColor = new Color(danger.r, danger.g, danger.b, 0.8f),
                        startSize = Random.Range(0.05f, 0.09f),
                        startLifetime = Mathf.Min(Random.Range(0.4f, 0.7f), 1.6f - deathAge)
                    };
                    deathEmbers.Emit(p, 1);
                }
            }
            if (deathAge >= 1.6f) deathActive = false;
        }

        void EmitDeathPull()
        {
            var p = new ParticleSystem.EmitParams();
            for (int i = 0; i < 16; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                float radius = Random.Range(0.65f, 1f), life = Random.Range(0.14f, 0.18f);
                p.position = deathPosition + dir * radius;
                p.velocity = -dir * radius / life;
                p.startLifetime = life;
                p.startSize = Random.Range(0.025f, 0.05f);
                p.startColor = danger;
                deathEmbers.Emit(p, 1);
            }
        }

        void EmitDeathOutward(ParticleSystem ps, int count, float speedMin, float speedMax,
            float sizeMin, float sizeMax, float lifeMin, float lifeMax, bool bright)
        {
            var p = new ParticleSystem.EmitParams();
            for (int i = 0; i < count; i++)
            {
                p.position = deathPosition + Random.insideUnitCircle * 0.1f;
                p.velocity = Random.insideUnitCircle.normalized * Random.Range(speedMin, speedMax);
                p.startSize = Random.Range(sizeMin, sizeMax);
                p.startLifetime = Random.Range(lifeMin, lifeMax);
                p.startColor = bright ? (i % 3 == 0 ? hot : danger) : (i % 3 == 0 ? darkRed : smoke);
                ps.Emit(p, 1);
            }
        }

        static bool HasFrames(Sprite[] frames) => frames != null && frames.Length > 0 && frames[0];

        static Sprite FrameAt(Sprite[] frames, float age, float fps, bool loop)
        {
            if (!HasFrames(frames)) return null;
            int i = Mathf.FloorToInt(Mathf.Max(0f, age) * Mathf.Max(1f, fps));
            i = loop ? i % frames.Length : Mathf.Min(i, frames.Length - 1);
            return frames[i] ? frames[i] : frames[0];
        }

        public SpriteRenderer MakeFrameRenderer(Transform parent, string name, Sprite first, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = first;
            if (alphaMaterial) sr.sharedMaterial = alphaMaterial;
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = order;
            sr.enabled = false;
            return sr;
        }

        // Các hiệu ứng chỉ hiển thị tái dùng slot cũ nhất nếu tất cả slot đang bận.
        // Không tạo hay hủy GameObject khi một đạn nổ.
        sealed class FramePool
        {
            readonly SpriteRenderer[] renderers;
            readonly float[] ages;
            readonly Sprite[] frames;
            readonly float fps;
            int next;

            public FramePool(RootwraithFx owner, Transform root, string name, Sprite[] frames, float fps, int count, int order)
            {
                this.frames = frames;
                this.fps = Mathf.Max(1f, fps);
                renderers = new SpriteRenderer[count];
                ages = new float[count];
                for (int i = 0; i < count; i++) renderers[i] = owner.MakeFrameRenderer(root, name + i, frames[0], order);
            }

            public void Play(Vector2 pos, float scale, float alpha = 1f)
            {
                int slot = next;
                for (int i = 0; i < renderers.Length; i++)
                {
                    int candidate = (next + i) % renderers.Length;
                    if (renderers[candidate] && !renderers[candidate].enabled) { slot = candidate; break; }
                }
                next = (slot + 1) % renderers.Length;
                ages[slot] = 0f;
                var sr = renderers[slot];
                if (!sr) return;
                sr.sprite = frames[0];
                sr.transform.position = pos;
                sr.transform.localScale = Vector3.one * scale;
                sr.color = new Color(1f, 1f, 1f, alpha);
                sr.enabled = true;
            }

            public void Tick(float step)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    var sr = renderers[i];
                    if (!sr || !sr.enabled) continue;
                    ages[i] += step;
                    if (ages[i] * fps >= frames.Length) { sr.enabled = false; continue; }
                    sr.sprite = FrameAt(frames, ages[i], fps, false);
                }
            }

            public void Clear() { foreach (var sr in renderers) if (sr) sr.enabled = false; }
        }

        // ---------- Sprite năng lượng cho đạn/gai (do controller quản lý) ----------

        /// <summary>Tạo một SpriteRenderer dùng shader năng lượng. shape: 0 cầu, 1 vệt cảnh báo, 2 vòng, 3 vệt lao.</summary>
        public SpriteRenderer MakeEnergy(Transform parent, string name, int shape, float intensity)
        {
            Build();
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            if (energyMaterial)
            {
                sr.material = energyMaterial; // instance riêng để đổi _Shape
                sr.material.SetFloat("_Shape", shape);
                sr.material.SetFloat("_Intensity", intensity);
                sr.material.SetColor("_CoreColor", hot);
                sr.material.SetColor("_RimColor", danger);
            }
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = sortingOrder + 1;
            return sr;
        }

        // ---------- Particle ----------

        ParticleSystem MakePs(Transform parent, string name, Material mat, int max,
            float lifeMin, float lifeMax, float speedMin, float speedMax, float sizeMin, float sizeMax)
        {
            var go = new GameObject("Ps_" + name);
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = max;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.useUnscaledTime = false;
            var em = ps.emission; em.enabled = false;
            var shape = ps.shape; shape.enabled = false;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, name == "Smoke" || name == "Dust" ? 1.8f : 0.2f));
            var vel = ps.limitVelocityOverLifetime;
            vel.enabled = true;
            vel.dampen = name == "Sparks" || name == "Debris" ? 0.05f : 0.15f;
            vel.limit = 30f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            if (mat)
            {
                r.material = mat;
                r.material.mainTexture = soft.texture;
            }
            r.sortingLayerName = sortingLayer;
            r.sortingOrder = name == "Smoke" || name == "Dust" ? sortingOrder - 1 : sortingOrder + 2;
            return ps;
        }

        static void Emit(ParticleSystem ps, Vector2 pos, int count, Color color, Vector2 size, float spread)
        {
            if (!ps || count <= 0) return;
            var p = new ParticleSystem.EmitParams { applyShapeToPosition = false };
            for (int i = 0; i < count; i++)
            {
                p.position = pos + Random.insideUnitCircle * spread * 0.5f;
                p.velocity = Random.insideUnitCircle.normalized * Random.Range(0.3f, 1f) * ps.main.startSpeed.constantMax;
                p.startColor = color;
                p.startSize = Random.Range(size.x, size.y);
                p.startLifetime = Random.Range(ps.main.startLifetime.constantMin, ps.main.startLifetime.constantMax);
                ps.Emit(p, 1);
            }
        }

        static void EmitCone(ParticleSystem ps, Vector2 pos, int count, Color color, float degMin, float degMax)
        {
            if (!ps) return;
            var p = new ParticleSystem.EmitParams();
            for (int i = 0; i < count; i++)
            {
                float a = Random.Range(degMin, degMax) * Mathf.Deg2Rad;
                p.position = pos;
                p.velocity = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Random.Range(ps.main.startSpeed.constantMin, ps.main.startSpeed.constantMax);
                p.startColor = color;
                p.startSize = Random.Range(ps.main.startSize.constantMin, ps.main.startSize.constantMax);
                p.startLifetime = Random.Range(ps.main.startLifetime.constantMin, ps.main.startLifetime.constantMax);
                ps.Emit(p, 1);
            }
        }

        // ---------- Texture dựng bằng code ----------

        static Sprite MakeSquare()
        {
            var t = new Texture2D(8, 8, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[64];
            for (int i = 0; i < 64; i++) px[i] = new Color32(255, 255, 255, 255);
            t.SetPixels32(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
        }

        static Sprite MakeSoft()
        {
            const int n = 32;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float d = new Vector2(x - n / 2f + 0.5f, y - n / 2f + 0.5f).magnitude / (n / 2f);
                float a = Mathf.Clamp01(1f - d);
                px[y * n + x] = new Color32(255, 255, 255, (byte)(a * a * 255));
            }
            t.SetPixels32(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        /// <summary>Gai rễ nhọn: thân đen tím, mép đỏ cam, đáy rộng. Pivot ở đáy giữa, cao 1 unit.</summary>
        Sprite MakeSpike()
        {
            const int w = 32, h = 80;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[w * h];
            Color32 body = new Color(0.1f, 0.07f, 0.12f), edge = danger, tip = hot;
            for (int y = 0; y < h; y++)
            {
                float k = y / (float)(h - 1);
                float half = (w / 2f) * (1f - k) * (1f - k * 0.15f);
                for (int x = 0; x < w; x++)
                {
                    float dx = Mathf.Abs(x - w / 2f + 0.5f);
                    if (dx > half) { px[y * w + x] = default; continue; }
                    bool rim = dx > half - 2.2f;
                    Color32 c = rim ? (k > 0.82f ? tip : edge) : body;
                    if (!rim && Mathf.Abs(x - w / 2f + 0.5f) < 1.2f && k > 0.2f) c = darkRed; // gân giữa
                    px[y * w + x] = c;
                }
            }
            t.SetPixels32(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), h);
        }
    }
}
