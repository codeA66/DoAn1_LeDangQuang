using LuHanh.Enemies;
using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>Các mesh pixel đặt sẵn, đổi vị trí/scale/màu; không tạo hoặc hủy object lúc chơi.</summary>
    public sealed class FoundryWardenFx : MonoBehaviour
    {
        [Tooltip("Ba vạch báo đỏ đặt sẵn cho xả áp.")][SerializeField] MeshRenderer[] warnings;
        [Tooltip("Hai sóng đất đặt sẵn.")][SerializeField] MeshRenderer[] waves;
        [Tooltip("Các cột hơi nóng đặt sẵn, một mesh mỗi điểm phun.")][SerializeField] MeshRenderer[] vents;
        [Tooltip("Mesh báo/đánh vùng đập hoặc quét.")][SerializeField] MeshRenderer melee;
        [Tooltip("Tia lửa đặt sẵn, tái sử dụng mỗi đòn.")][SerializeField] ParticleSystem sparks;
        [Tooltip("12 cạnh viền đặt sẵn, báo chính xác tối đa ba vùng nguy hiểm.")]
        [SerializeField] MeshRenderer[] boundaries;
        [Tooltip("Ba lớp tô mờ phía trong vùng cảnh báo, đặt sẵn trong prefab.")]
        [SerializeField] MeshRenderer[] fills;
        [Tooltip("Ba lớp bổ sung mỗi điểm phun/vùng đánh: quầng, lõi và khói/dư ảnh.")]
        [SerializeField] MeshRenderer[] layers;
        [Tooltip("Hai hệ hạt bụi/tàn lửa chạy theo sóng, tái sử dụng.")]
        [SerializeField] ParticleSystem[] waveTrails;
        [Tooltip("Một hệ tàn lửa đặt sẵn mỗi cột xả áp.")]
        [SerializeField] ParticleSystem[] ventBursts;
        [Tooltip("Vật liệu hiệu ứng chung, đặt sẵn trong prefab.")]
        [SerializeField] Material effectMaterial;
        [Tooltip("Shader lửa xả áp cuộn và phồng; không tạo vật liệu lúc chơi.")]
        [SerializeField] Material pressureFlameMaterial;
        [Tooltip("Shader xung kích riêng cho đập và sóng đất.")]
        [SerializeField] Material shockwaveMaterial;
        [Tooltip("Hệ hạt mảnh nóng của đòn đập, đặt sẵn.")]
        [SerializeField] ParticleSystem impactDebris;
        [Tooltip("Hệ hạt chạy theo đầu quét, đặt sẵn.")]
        [SerializeField] ParticleSystem sweepTrail;
        [Tooltip("Bụi chân húc; World space, đặt sẵn trong prefab.")]
        [SerializeField] ParticleSystem ramDust;
        [Tooltip("Tia lửa phóng ngược chiều húc, đặt sẵn trong prefab.")]
        [SerializeField] ParticleSystem ramSparks;
        [Tooltip("Báo đòn bằng tư thế, ánh nóng và hạt; bỏ khung hitbox. Chỉ bật sau khi duyệt preview.")]
        [SerializeField] bool naturalTelegraphs;
        [Tooltip("Hình boss dùng để đặt ánh tụ lực theo kích thước tư thế hiện tại.")]
        [SerializeField] SpriteRenderer chargeVisual;
        [Tooltip("Khiên đặt sẵn, chỉ hiện khi boss bất tử; không collider hoặc sát thương.")]
        [SerializeField] MeshRenderer shield;
        [Tooltip("Khoảng nới hình khiên so với collider thân; chỉ là hình ảnh.")]
        [SerializeField] Vector2 shieldPadding = new(0.6f, 0.6f);
        MaterialPropertyBlock colors;
        static readonly int ColorId = Shader.PropertyToID("_BaseColor");
        static readonly int KindId = Shader.PropertyToID("_FxKind");
        static readonly int ProgressId = Shader.PropertyToID("_Progress");
        static readonly int DirectionId = Shader.PropertyToID("_Direction");
        bool burstPlayed;
        int chargeBeat;
        bool chargingParticles;
        Collider2D chargeBody;
        static readonly Color Red = new(0.918f, 0.396f, 0.278f, 1f);
        static readonly Color HotRed = new(1f, 0.53f, 0.39f, 1f);
        float stageProgress;
        int[] ventPhase;

        void Set(MeshRenderer renderer, Vector2 centre, Vector2 size, Color color, float kind = 0f, float direction = 1f)
        {
            if (!renderer) return;
            var material = kind == 2f && pressureFlameMaterial ? pressureFlameMaterial :
                (kind == 1f || kind == 3f) && shockwaveMaterial ? shockwaveMaterial : effectMaterial;
            if (material && renderer.sharedMaterial != material) renderer.sharedMaterial = material;
            renderer.enabled = true;
            colors ??= new MaterialPropertyBlock();
            renderer.transform.position = new Vector3(centre.x, centre.y, -0.1f);
            renderer.transform.rotation = Quaternion.identity;
            renderer.transform.localScale = new Vector3(size.x, size.y, 1f);
            colors.SetColor(ColorId, color);
            colors.SetFloat(KindId, kind);
            colors.SetFloat(ProgressId, stageProgress);
            colors.SetFloat(DirectionId, direction * Mathf.Sign(transform.lossyScale.x));
            renderer.SetPropertyBlock(colors);
        }
        static void Hide(MeshRenderer[] renderers)
        {
            if (renderers == null) return;
            foreach (var renderer in renderers) if (renderer) renderer.enabled = false;
        }
        void HideShapes()
        {
            Hide(warnings); Hide(waves); Hide(vents); Hide(boundaries); Hide(fills); Hide(layers);
            if (melee) melee.enabled = false;
        }
        public void Clear()
        {
            if (vents != null && (ventPhase == null || ventPhase.Length != vents.Length))
                ventPhase = new int[vents.Length];
            if (ventPhase != null) System.Array.Clear(ventPhase, 0, ventPhase.Length);
            HideShapes();
            if (shield) shield.enabled = false;
            if (chargingParticles) EndChargeParticles();
            burstPlayed = false;
            chargeBeat = 0;
            chargingParticles = false;
            if (sparks) sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            StopParticles(waveTrails, true);
            StopParticles(ventBursts, true);
            if (impactDebris) impactDebris.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (sweepTrail) sweepTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (ramDust) ramDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (ramSparks) ramSparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        public void Finish()
        {
            HideShapes();
            if (sparks && sparks.isEmitting) sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            StopParticles(waveTrails, false);
            StopParticles(ventBursts, false);
            if (impactDebris && impactDebris.isEmitting) impactDebris.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (sweepTrail && sweepTrail.isEmitting) sweepTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (ramDust && ramDust.isEmitting) ramDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (ramSparks && ramSparks.isEmitting) ramSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        public void DrawShield(bool active, Bounds body, float phaseProgress, float blockPulse)
        {
            if (!shield) return;
            if (!active) { shield.enabled = false; return; }
            float savedProgress = stageProgress;
            stageProgress = phaseProgress;
            Color color = Color.Lerp(Red, HotRed, blockPulse);
            color.a = 0.5f + blockPulse * 0.4f;
            Set(shield, body.center, (Vector2)body.size + shieldPadding, color, 10f, 1f + blockPulse);
            stageProgress = savedProgress;
        }
        static void StopParticles(ParticleSystem[] systems, bool clear)
        {
            if (systems == null) return;
            foreach (var ps in systems)
                if (ps && (clear || ps.isEmitting)) ps.Stop(true, clear ?
                    ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
        }
        void AddLayers(int slot, Vector2 centre, Vector2 size, float kind, float direction)
        {
            int first = slot * 3;
            if (layers == null || layers.Length < first + 3) return;
            float progress = stageProgress;
            Color glow = Red; glow.a = 0.2f * (1f - progress * 0.35f);
            Set(layers[first], centre, size, glow, 7f, direction);
            Color core = new(1f, 0.74f, 0.58f, 0.55f + 0.25f * (1f - progress));
            Vector2 coreSize = new(size.x * (kind == 2f ? 0.45f : 0.8f), size.y * 0.85f);
            Set(layers[first + 1], centre, coreSize, core, kind, direction);
            Color residual = new(0.56f, 0.22f, 0.24f, 0.45f);
            if (kind == 2f)
            {
                residual = new Color(0.60f, 0.48f, 0.66f, 0.45f);
                Set(layers[first + 2], centre, size, residual, 9f, direction);
            }
            else if (kind == 3f)
            {
                residual = HotRed; residual.a = 0.75f * (1f - progress);
                Set(layers[first + 2], centre, size, residual, 8f, direction);
            }
            else
            {
                stageProgress = Mathf.Max(0f, progress - 0.14f);
                Vector2 shift = kind == 1f ? new Vector2(-direction * size.x * 0.18f, 0f) : Vector2.zero;
                Set(layers[first + 2], centre + shift, size * 0.9f, residual, kind, direction);
                stageProgress = progress;
            }
        }
        void Outline(Rect rect, int slot, Color color, bool fill)
        {
            int first = slot * 4;
            if (boundaries != null && boundaries.Length >= first + 4)
            {
                var size = new Vector2(Mathf.Min(0.65f, rect.width * 0.3f), Mathf.Min(0.5f, rect.height * 0.4f));
                for (int i = 0; i < 4; i++)
                {
                    bool right = (i & 1) != 0, top = i >= 2;
                    var centre = new Vector2(right ? rect.xMax - size.x * 0.5f : rect.xMin + size.x * 0.5f,
                        top ? rect.yMax - size.y * 0.5f : rect.yMin + size.y * 0.5f);
                    Set(boundaries[first + i], centre, size, color, 4f, right ? -1f : 1f);
                    if (top) boundaries[first + i].transform.localScale =
                        new Vector3(size.x, -size.y, 1f);
                }
            }
            if (fill && fills != null && slot < fills.Length)
            {
                color.a = 0.1f + stageProgress * 0.08f;
                Set(fills[slot], rect.center, rect.size, color, 6f);
            }
        }
        public void Charge(WardenAttack kind, Vector2 foot, Rect area, FoundryWardenConfig config, float progress)
        {
            HideShapes();
            stageProgress = Mathf.Clamp01(progress);
            if (naturalTelegraphs)
            {
                NaturalCharge(kind, foot, area, config);
                return;
            }
            float pulse = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 4f));
            Color color = Color.Lerp(Red, HotRed, pulse);
            if (kind == WardenAttack.Vent)
            {
                for (int i = 0; i < warnings.Length && i < config.VentOffsets.Length; i++)
                    Set(warnings[i], foot + new Vector2(config.VentOffsets[i], 0.06f),
                        new Vector2(config.VentSize.x, 0.12f), color);
                for (int i = 0; i < config.VentOffsets.Length; i++)
                    Outline(new Rect(foot + new Vector2(config.VentOffsets[i] - config.VentSize.x * 0.5f, 0f),
                        config.VentSize), i, color, true);
            }
            else if (kind == WardenAttack.DoubleSlam)
            {
                if (warnings != null)
                    for (int i = 0; i < warnings.Length && i < 2; i++)
                        Set(warnings[i], foot + new Vector2((i == 0 ? -1f : 1f) * config.WaveDistance * 0.5f, 0.07f),
                            new Vector2(config.WaveDistance, 0.14f), color, 0f, i == 0 ? -1f : 1f);
            }
            else
            {
                Outline(area, 0, color, true);
                if (warnings != null && warnings.Length > 0)
                    Set(warnings[0], new Vector2(area.center.x, area.yMin + 0.06f), new Vector2(area.width, 0.12f), color);
            }
        }
        void ChargeParticle(ParticleSystem system, Vector2 position, Color color, int count, bool dust)
        {
            if (!system) return;
            if (!system.isPlaying)
            {
                var module = system.emission;
                module.enabled = false;
                system.Play();
            }
            var emission = new ParticleSystem.EmitParams
            {
                position = new Vector3(position.x, position.y, 0f),
                applyShapeToPosition = false,
                startColor = color,
                startSize = dust ? 0.12f : 0.045f,
                startLifetime = dust ? 0.32f : 0.22f
            };
            for (int i = 0; i < count; i++)
            {
                emission.velocity = new Vector3(Random.Range(-0.45f, 0.45f), dust ? 0.6f : Random.Range(0.8f, 1.8f), 0f);
                system.Emit(emission, 1);
            }
        }
        void NaturalCharge(WardenAttack kind, Vector2 foot, Rect area, FoundryWardenConfig config)
        {
            chargingParticles = true;
            if (kind == WardenAttack.RockThrow) return; // Tín hiệu ném nằm ở đá và tư thế giơ tay.
            int beat = Mathf.FloorToInt(stageProgress * 8f);
            bool emit = beat > chargeBeat;
            chargeBeat = beat;
            Color heat = Red; heat.a = 0.15f + stageProgress * 0.3f;
            if (kind == WardenAttack.Vent)
            {
                for (int i = 0; i < config.VentOffsets.Length; i++)
                {
                    float until = config.ChargeDuration * (1f - stageProgress) + config.VentDelay(i);
                    if (until <= config.VentWaveInterval)
                        VentCue(i, foot, config, 1f - until / config.VentWaveInterval);
                }
            }
            else if (kind == WardenAttack.DoubleSlam)
            {
                if (layers != null && layers.Length > 0)
                    Set(layers[0], foot + Vector2.up * 0.025f, new Vector2(config.WaveSize.x, 0.05f), heat, 8f);
                if (emit) ChargeParticle(impactDebris, foot, new Color(0.42f, 0.35f, 0.48f, 0.35f), 2, true);
            }
            else
            {
                float direction = kind == WardenAttack.Ram ?
                    (config.artFacesRight ? 1f : -1f) * Mathf.Sign(transform.lossyScale.x) :
                    area.center.x >= foot.x ? 1f : -1f;
                Vector2 hand = area.center;
                if (chargeVisual)
                {
                    Bounds bounds = chargeVisual.bounds;
                    if (!chargeBody) chargeBody = chargeVisual.GetComponentInParent<Collider2D>();
                    float height = chargeBody ? chargeBody.bounds.size.y : bounds.size.y;
                    hand = new Vector2(bounds.center.x + direction * bounds.size.x *
                        0.28f, foot.y + height * 0.5f);
                }
                if (layers != null && layers.Length > 0)
                    Set(layers[0], hand, Vector2.one * config.WaveSize.x, heat, 7f);
                if (emit)
                {
                    ChargeParticle(sparks, hand, Red, 2, false);
                }
            }
        }
        static void RestoreEmission(ParticleSystem system)
        {
            if (!system) return;
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var module = system.emission;
            module.enabled = true;
        }
        void EndChargeParticles()
        {
            chargingParticles = false;
            chargeBeat = 0;
            RestoreEmission(sparks);
            RestoreEmission(impactDebris);
            if (ventBursts != null) foreach (var system in ventBursts) RestoreEmission(system);
        }
        public void Active(WardenAttack kind, Vector2 foot, Rect area, FoundryWardenConfig config,
            float progress, float[] travel, bool[] stopped, Vector2? strikeSource = null, float ramDirection = 0f)
        {
            HideShapes();
            if (chargingParticles)
            {
                EndChargeParticles();
            }
            stageProgress = Mathf.Clamp01(progress);
            bool firstFrame = !burstPlayed;
            if (firstFrame && impactDebris && kind == WardenAttack.DoubleSlam)
            {
                impactDebris.transform.position = new Vector3(foot.x,
                    foot.y + 0.12f, 0f);
                impactDebris.Play();
            }
            if (!burstPlayed)
            {
                burstPlayed = true;
                if (sparks)
                {
                    sparks.transform.position = kind == WardenAttack.RockThrow && strikeSource.HasValue ?
                        (Vector3)strikeSource.Value : kind == WardenAttack.Ram ?
                        new Vector3(area.center.x, foot.y + 0.15f, 0f) : new Vector3(foot.x, foot.y + 0.15f, 0f);
                    sparks.Play();
                }
            }
            if (kind == WardenAttack.RockThrow)
            {
                if (firstFrame && sparks && strikeSource.HasValue)
                    sparks.transform.position = strikeSource.Value;
                // Đá và hạt va chạm do FoundryWardenRock quản lý; không phát hiệu ứng đập cũ.
            }
            else if (kind == WardenAttack.Ram)
            {
                float direction = ramDirection == 0f ? Mathf.Sign(transform.lossyScale.x) : ramDirection;
                float rear = area.center.x - direction * area.width * 0.5f;
                Vector2 wake = new(rear - direction * area.width * 0.3f, foot.y + area.height * 0.4f);
                Color color = Red; color.a = 0.7f;
                Set(melee, wake, new Vector2(area.width * 0.7f, area.height * 0.8f), color, 11f, direction);
                if (layers != null && layers.Length >= 3)
                {
                    color.a = 0.18f;
                    Set(layers[0], wake, new Vector2(area.width * 0.85f, area.height), color, 11f, direction);
                    color = HotRed; color.a = 0.75f;
                    Set(layers[1], new Vector2(rear, foot.y + 0.1f), new Vector2(1.5f, 0.35f), color, 11f, direction);
                }
                RamParticles(ramDust, new Vector2(rear, foot.y + 0.12f), direction, false);
                RamParticles(ramSparks, new Vector2(rear, foot.y + 0.3f), direction, true);
            }
            else if (kind == WardenAttack.DoubleSlam)
            {
                for (int i = 0; i < waves.Length && i < 2; i++)
                {
                    float sign = i == 0 ? -1f : 1f;
                    if (stopped[i])
                    {
                        if (waveTrails != null && i < waveTrails.Length && waveTrails[i] && waveTrails[i].isEmitting)
                            waveTrails[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        continue;
                    }
                    Vector2 centre = foot + new Vector2(sign * travel[i], config.WaveSize.y * 0.5f);
                    Set(waves[i], centre, config.WaveSize, Red, 1f, sign);
                    AddLayers(i, centre, config.WaveSize, 1f, sign);
                    if (waveTrails != null && i < waveTrails.Length && waveTrails[i])
                    {
                        waveTrails[i].transform.position = new Vector3(centre.x, foot.y + 0.1f, 0f);
                        if (!waveTrails[i].isEmitting) waveTrails[i].Play();
                    }
                }
            }
            else if (kind == WardenAttack.Vent)
            {
                for (int i = 0; i < vents.Length && i < config.VentOffsets.Length; i++)
                {
                    float localTime = progress * config.ActiveDuration(kind) - config.VentDelay(i);
                    if (localTime < 0f)
                    {
                        if (localTime >= -config.VentWaveInterval)
                            VentCue(i, foot, config, 1f + localTime / config.VentWaveInterval);
                        continue;
                    }
                    if (localTime >= config.VentDuration)
                    {
                        if (ventPhase != null && i < ventPhase.Length && ventPhase[i] != 3)
                        {
                            ventPhase[i] = 3;
                            if (ventBursts != null && i < ventBursts.Length && ventBursts[i])
                                ventBursts[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        }
                        continue;
                    }
                    stageProgress = Mathf.Clamp01(localTime / config.VentDuration);
                    Vector2 centre = foot + new Vector2(config.VentOffsets[i], config.VentSize.y * 0.5f);
                    Set(vents[i], centre, config.VentSize, Red, 2f);
                    AddLayers(i, centre, config.VentSize, 2f, 1f);
                    Color border = Red; border.a = 0.55f;
                    if (!naturalTelegraphs)
                        Outline(new Rect(centre - config.VentSize * 0.5f, config.VentSize), i, border, false);
                    if (ventPhase != null && i < ventPhase.Length && ventPhase[i] != 2)
                    {
                        ventPhase[i] = 2;
                        if (ventBursts != null && i < ventBursts.Length && ventBursts[i])
                        {
                            RestoreEmission(ventBursts[i]);
                            ventBursts[i].transform.position = new Vector3(centre.x, foot.y + 0.15f, 0f);
                            ventBursts[i].Play();
                        }
                    }
                }
            }
            else
            {
                Color c = HotRed; c.a = 1f - progress * 0.35f;
                float direction = area.center.x >= foot.x ? 1f : -1f;
                float mode = 3f;
                Set(melee, area.center, area.size, c, mode, direction);
                AddLayers(0, area.center, area.size, mode, direction);
                if (strikeSource.HasValue && layers != null && layers.Length >= 9)
                {
                    // Nối hiệu ứng từ găng đã đo tới vùng đánh được duyệt; không dịch hitbox.
                    Vector2 start = strikeSource.Value;
                    Vector2 delta = area.center - start;
                    float savedProgress = stageProgress;
                    stageProgress = 1f;
                    Color connection = Red; connection.a = 0.5f * (1f - progress);
                    Set(layers[8], (start + area.center) * 0.5f,
                        new Vector2(delta.magnitude, Mathf.Min(area.height, config.WaveSize.y)), connection, 7f);
                    layers[8].transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                    connection = HotRed; connection.a = 0.4f * (1f - progress);
                    Set(layers[6], start, Vector2.one * config.WaveSize.x, connection, 7f);
                    stageProgress = savedProgress;
                }
                c = Red; c.a = 0.7f;
                if (!naturalTelegraphs) Outline(area, 0, c, false);
            }
        }

        void VentCue(int index, Vector2 foot, FoundryWardenConfig config, float progress)
        {
            Vector2 point = foot + new Vector2(config.VentOffsets[index], 0.08f);
            int slot = index * 3;
            if (layers != null && slot < layers.Length)
                Set(layers[slot], point + Vector2.up * 0.15f, new Vector2(0.7f, 0.45f),
                    new Color(0.918f, 0.396f, 0.278f, 0.15f + progress * 0.3f), 7f);
            if (ventPhase != null && index < ventPhase.Length && ventPhase[index] == 0)
            {
                ventPhase[index] = 1;
                if (ventBursts != null && index < ventBursts.Length)
                    ChargeParticle(ventBursts[index], point, Red, 3, false);
            }
        }

        static void RamParticles(ParticleSystem system, Vector2 position, float direction, bool sparks)
        {
            if (!system) return;
            system.transform.position = new Vector3(position.x, position.y, 0f);
            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            float speed = sparks ? 7f : 3f;
            velocity.x = new ParticleSystem.MinMaxCurve(-direction * speed);
            velocity.y = new ParticleSystem.MinMaxCurve(sparks ? 1.5f : 0.7f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f);
            if (!system.isEmitting) system.Play();
        }
    }
}
