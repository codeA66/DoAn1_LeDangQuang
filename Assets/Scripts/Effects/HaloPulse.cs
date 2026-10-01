using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Quang sang "tho": cac SpriteRenderer con to nho va sang toi nhe theo mot chu ky,
    /// nhu sinh vat mang anh sang. Chi doi scale + alpha, khong cap phat gi moi frame.
    /// </summary>
    public class HaloPulse : MonoBehaviour
    {
        [Tooltip("Mot nhip tho mat bao lau (giay).")]
        [SerializeField] float period = 4f;
        [Tooltip("To nho them / bot bao nhieu phan (0.05 = 5%).")]
        [SerializeField] float scaleAmount = 0.05f;
        [Tooltip("Sang toi them / bot bao nhieu phan alpha goc (0.15 = 15%).")]
        [SerializeField] float alphaAmount = 0.15f;

        SpriteRenderer[] layers;
        Vector3[] baseScales;
        float[] baseAlphas;

        void Awake()
        {
            layers = GetComponentsInChildren<SpriteRenderer>(true);
            baseScales = new Vector3[layers.Length];
            baseAlphas = new float[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                baseScales[i] = layers[i].transform.localScale;
                baseAlphas[i] = layers[i].color.a;
            }
        }

        void Update()
        {
            float wave = Mathf.Sin(Time.time * Mathf.PI * 2f / Mathf.Max(0.1f, period));
            for (int i = 0; i < layers.Length; i++)
            {
                // Lop sau lech pha mot chut de hai tang khong tho dong loat.
                float w = i == 0 ? wave : Mathf.Sin(Time.time * Mathf.PI * 2f / Mathf.Max(0.1f, period) + 0.8f * i);
                layers[i].transform.localScale = baseScales[i] * (1f + w * scaleAmount);
                var c = layers[i].color;
                c.a = baseAlphas[i] * (1f + w * alphaAmount);
                layers[i].color = c;
            }
        }
    }
}
