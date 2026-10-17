using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Hieu ung luc ha guc quai: manh vang tung ra (particle tao bang code, chay thoi gian thuc), khung hinh
    /// khung lau hon don thuong, rung camera nhe va tieng quai chet. Dat trong LevelEssentials/GameSystems.
    /// </summary>
    public class KillFx : MonoBehaviour
    {
        static KillFx instance;

        [SerializeField] int burstCount = 22;
        [SerializeField] Color colorA = new Color(0.95f, 0.96f, 0.97f, 1f);
        [SerializeField] Color colorB = new Color(0.56f, 0.22f, 0.24f, 1f);   // #8F393D (do toi - mau vien quai)
        [SerializeField] float hitStop = 0.09f;
        [SerializeField] float shake = 0.18f;
        [SerializeField] float shakeTime = 0.18f;
        [SerializeField] int sortingOrder = 24;

        ParticleSystem burst;

        void Awake()
        {
            instance = this;
            var go = new GameObject("KillBurst");
            go.transform.SetParent(transform, false);
            burst = go.AddComponent<ParticleSystem>();
            burst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = burst.main;
            main.playOnAwake = false;
            main.loop = false;
            main.useUnscaledTime = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 11f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
            main.gravityModifier = 1.6f;
            main.maxParticles = 256;
            var em = burst.emission; em.rateOverTime = 0f;
            var sh = burst.shape; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = 0.3f;
            var col = burst.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sortingOrder = sortingOrder;
            r.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        void OnDestroy() { if (instance == this) instance = null; }

        /// <summary>Goi khi quai bi chem / ban chet tai <paramref name="position"/>.</summary>
        public static void Play(Vector2 position)
        {
            Sfx.Play(SfxId.EnemyDeath);
            if (!instance) return;
            instance.burst.transform.position = position;
            instance.burst.Emit(instance.burstCount);
            if (instance.hitStop > 0f) HitStop.Request(instance.hitStop);
            if (instance.shake > 0f) CameraShake.Kick(instance.shake, instance.shakeTime);
        }
    }
}
