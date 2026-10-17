using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Hiệu ứng của boss Rootwraith: particle (khói dịch chuyển, tia lửa, bụi, mảnh vỡ) dựng bằng code lúc chạy,
    /// cùng các sprite năng lượng dùng shader LuHanh/RootwraithEnergy (đạn, vệt cảnh báo, vòng sóng, vệt lao).
    /// Chỉ hiển thị - sát thương do RootwraithController tính.
    /// Ảnh vẽ tay về sau: gán vào orbSprite / spikeSprite là xong, không phải sửa code.
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
        int nextRing;
        bool built;

        public Sprite OrbSprite => orbSprite;
        public Sprite SquareSprite { get { Build(); return square; } }
        public Sprite SpikeSprite { get { Build(); return spikeSprite ? spikeSprite : spikeGen; } }
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

            trail = MakeEnergy(root, "DiveTrail", 3, 1f);
            trail.enabled = false;
            eyeFlare = MakeEnergy(root, "EyeFlare", 0, 3f);
            eyeFlare.enabled = false;
            for (int i = 0; i < rings.Length; i++)
            {
                rings[i] = MakeEnergy(root, "Ring" + i, 2, 2.5f);
                rings[i].enabled = false;
            }
        }

        GameObject worldRoot;
        void OnDestroy() { if (worldRoot) Destroy(worldRoot); }

        // ---------- API ----------

        /// <summary>Khói tối bung ra + tàn đỏ khi tan biến / hiện ra.</summary>
        public void Smoke(Vector2 pos, float size = 1f)
        {
            Emit(smokePs, pos, (int)(28 * size), smoke, new Vector2(0.6f, 1.6f) * size, 1.2f * size);
            Emit(emberPs, pos, (int)(22 * size), danger, new Vector2(0.08f, 0.2f), 1.1f * size);
        }

        /// <summary>
        /// Tụ lực trước khi boss xuất hiện: tàn đỏ và khói từ vòng bán kính <paramref name="radius"/> hút về <paramref name="centre"/>.
        /// Gọi mỗi frame, <paramref name="k"/> 0..1 tăng dần thì càng dày.
        /// </summary>
        public void Gather(Vector2 centre, float radius, float k)
        {
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
            eyeFlare.enabled = k > 0f && k < 1f;
            if (!eyeFlare.enabled) return;
            eyeFlare.transform.position = pos;
            float s = Mathf.Lerp(0.4f, 1.6f, k) * (1f + 0.15f * Mathf.Sin(Time.time * 40f));
            eyeFlare.transform.localScale = Vector3.one * s;
            eyeFlare.color = new Color(1f, 1f, 1f, Mathf.Sin(k * Mathf.PI));
            if (Random.value < 0.5f) Emit(emberPs, pos + Random.insideUnitCircle * 1.2f, 1, danger, new Vector2(0.06f, 0.14f), 0.4f);
        }

        public void HideEyeFlare() { if (eyeFlare) eyeFlare.enabled = false; }

        /// <summary>Vệt lao kéo theo boss, xoay theo hướng bay.</summary>
        public void Trail(Vector2 pos, Vector2 dir, bool on)
        {
            trail.enabled = on;
            if (!on) return;
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            trail.transform.SetPositionAndRotation(pos - dir.normalized * 1.6f, Quaternion.Euler(0, 0, ang));
            trail.transform.localScale = new Vector3(4.5f, 1.4f, 1f);
            trail.color = Color.white;
            Emit(emberPs, pos + Random.insideUnitCircle * 0.6f, 3, danger, new Vector2(0.08f, 0.22f), 0.6f);
        }

        /// <summary>Chạm sàn sau cú lao: bụi, tia lửa, vòng sóng.</summary>
        public void Impact(Vector2 pos, float strength = 1f)
        {
            Emit(dustPs, pos, (int)(24 * strength), new Color(0.2f, 0.16f, 0.2f, 0.85f), new Vector2(0.5f, 1.1f), 1f);
            EmitCone(sparkPs, pos, (int)(30 * strength), hot, 25f, 155f);
            EmitCone(debrisPs, pos, (int)(14 * strength), new Color(0.12f, 0.09f, 0.12f), 40f, 140f);
            Ring(pos, 3.5f * strength, 0.35f);
        }

        /// <summary>Gai trồi lên: mảnh đất văng + tia lửa ở chân gai.</summary>
        public void SpikeBurst(Vector2 pos)
        {
            EmitCone(debrisPs, pos, 5, new Color(0.14f, 0.1f, 0.12f), 60f, 120f);
            EmitCone(sparkPs, pos, 6, danger, 70f, 110f);
        }

        /// <summary>Nòng bắn: chớp sáng nhỏ.</summary>
        public void Muzzle(Vector2 pos, float dirSign)
        {
            EmitCone(sparkPs, pos, 10, hot, dirSign > 0 ? -35f : 145f, dirSign > 0 ? 35f : 215f);
            Ring(pos, 1.2f, 0.18f);
        }

        /// <summary>Đạn vỡ.</summary>
        public void OrbPop(Vector2 pos)
        {
            Emit(sparkPs, pos, 8, danger, new Vector2(0.05f, 0.12f), 0.3f);
        }

        /// <summary>Vòng sóng xung kích.</summary>
        public void Ring(Vector2 pos, float size, float life)
        {
            int i = nextRing;
            nextRing = (nextRing + 1) % rings.Length;
            rings[i].enabled = true;
            rings[i].transform.position = pos;
            ringTime[i] = 0f; ringLife[i] = life; ringSize[i] = size;
        }

        /// <summary>Chết: khói lớn, tia lửa, nhiều vòng sóng.</summary>
        public void DeathBurst(Vector2 pos)
        {
            Smoke(pos, 2.2f);
            Emit(sparkPs, pos, 60, hot, new Vector2(0.06f, 0.18f), 1.5f);
            Ring(pos, 6f, 0.6f);
            Ring(pos, 3.5f, 0.4f);
        }

        /// <summary>Dọn hết (người chơi hồi sinh / boss chết).</summary>
        public void Clear()
        {
            if (!built) return;
            foreach (var ps in new[] { smokePs, sparkPs, dustPs, debrisPs, emberPs }) if (ps) ps.Clear();
            if (trail) trail.enabled = false;
            HideEyeFlare();
            foreach (var r in rings) if (r) r.enabled = false;
        }

        void Update()
        {
            for (int i = 0; i < rings.Length; i++)
            {
                if (!rings[i] || !rings[i].enabled) continue;
                ringTime[i] += Time.deltaTime;
                float k = ringTime[i] / ringLife[i];
                if (k >= 1f) { rings[i].enabled = false; continue; }
                float e = 1f - (1f - k) * (1f - k);
                rings[i].transform.localScale = Vector3.one * Mathf.Lerp(0.2f, ringSize[i], e);
                rings[i].color = new Color(1f, 1f, 1f, 1f - k);
            }
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
