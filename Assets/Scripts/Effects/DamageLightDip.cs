using LuHanh.Combat;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Effects
{
    /// <summary>
    /// Nguoi choi mat mau thi anh sang chung toi sap xuong roi sang lai (kieu Hollow Knight):
    /// chi giam cac den Global duoc gan, den/quang sang dat trong man van giu nguyen nen noi bat len.
    /// Chay bang thoi gian thuc vi luc trung don co HitStop.
    /// Dong thoi ap do toi do quai gay ra (<see cref="AmbientDarkness"/>): man toi dan, vang sang quanh
    /// nguoi choi sang dan len de van thay cho dat chan.
    /// </summary>
    public class DamageLightDip : MonoBehaviour
    {
        [Tooltip("Den Global se bi giam (vd Global Light - World, Global Light - Background).")]
        [SerializeField] Light2D[] ambientLights;
        [Tooltip("Con lai bao nhieu phan cuong do luc toi nhat.")]
        [Range(0f, 1f)] [SerializeField] float dipFactor = 0.45f;
        [SerializeField] float fallTime = 0.06f;
        [SerializeField] float holdTime = 0.18f;
        [SerializeField] float recoverTime = 0.6f;

        [Header("Man bi quai lam toi")]
        [Tooltip("Thoi gian chuyen het muc toi / sang (giay).")]
        [SerializeField] float darknessFadeTime = 1f;
        [Tooltip("Vang sang quanh nguoi choi khi man toi nhat.")]
        [SerializeField] float playerLightRadius = 3f;
        [SerializeField] float playerLightIntensity = 0.9f;
        [SerializeField] Color playerLightColor = new Color(0.86f, 0.84f, 1f);

        float[] baseIntensity;
        Health health;
        Light2D playerLight;
        int lastHealth;
        float timer = -1f;
        float darkness = 1f;

        void Start()
        {
            baseIntensity = new float[ambientLights.Length];
            for (int i = 0; i < ambientLights.Length; i++) if (ambientLights[i]) baseIntensity[i] = ambientLights[i].intensity;
            var p = GameObject.FindWithTag("Player");
            if (!p) return;
            if (p.TryGetComponent(out health))
            {
                lastHealth = health.Current;
                health.Changed += OnHealthChanged;
            }

            var go = new GameObject("DarkVisionLight");
            go.transform.SetParent(p.transform, false);
            go.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            playerLight = go.AddComponent<Light2D>();
            playerLight.lightType = Light2D.LightType.Point;
            playerLight.color = playerLightColor;
            playerLight.pointLightOuterRadius = playerLightRadius;
            playerLight.pointLightInnerRadius = playerLightRadius * 0.3f;
            playerLight.intensity = 0f;
            playerLight.enabled = false;
        }

        void OnDestroy()
        {
            if (health) health.Changed -= OnHealthChanged;
        }

        void OnHealthChanged(int current, int max)
        {
            if (current < lastHealth) timer = 0f;
            lastHealth = current;
        }

        void Update()
        {
            float target = AmbientDarkness.TargetFactor;
            bool darknessChanging = !Mathf.Approximately(darkness, target);
            if (timer < 0f && !darknessChanging) return;

            darkness = Mathf.MoveTowards(darkness, target, Time.deltaTime * (1f - AmbientDarkness.MinFactor) / Mathf.Max(0.01f, darknessFadeTime));
            if (playerLight)
            {
                float t = Mathf.InverseLerp(1f, AmbientDarkness.MinFactor, darkness);
                playerLight.intensity = playerLightIntensity * t;
                playerLight.enabled = t > 0.001f;
            }

            float k = 1f;
            if (timer >= 0f)
            {
                timer += Time.unscaledDeltaTime;
                if (timer < fallTime) k = Mathf.Lerp(1f, dipFactor, timer / fallTime);
                else if (timer < fallTime + holdTime) k = dipFactor;
                else
                {
                    float t = (timer - fallTime - holdTime) / Mathf.Max(0.01f, recoverTime);
                    k = Mathf.Lerp(dipFactor, 1f, t * t);
                    if (t >= 1f) { k = 1f; timer = -1f; }
                }
            }
            for (int i = 0; i < ambientLights.Length; i++)
                if (ambientLights[i]) ambientLights[i].intensity = baseIntensity[i] * k * darkness;
        }
    }
}
