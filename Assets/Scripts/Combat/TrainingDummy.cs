using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Hinh nom tap chem: nhan don, nhay mau, rung lech theo huong don roi ve cho cu.
    /// Khong bao gio chet - chi de thu don chem truoc khi co quai that.
    /// </summary>
    public class TrainingDummy : MonoBehaviour, IDamageable
    {
        [Tooltip("Sprite cua hinh nom (co the la object con).")]
        public SpriteRenderer sprite;
        public Color hitColor = new Color(1f, 0.55f, 0.55f);
        public float flashTime = 0.15f;
        [Tooltip("Do lech toi da khi bi danh, unit.")]
        public float shakeDistance = 0.15f;
        public float shakeTime = 0.15f;

        public int HitCount { get; private set; }

        float flashTimer;
        float shakeTimer;
        Vector2 shakeDir;
        Vector3 spriteBasePos;
        Color baseColor = Color.white;

        void Awake()
        {
            if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
            if (sprite != null)
            {
                spriteBasePos = sprite.transform.localPosition;
                baseColor = sprite.color;
            }
        }

        public void TakeHit(HitInfo hit)
        {
            HitCount++;
            flashTimer = flashTime;
            shakeTimer = shakeTime;
            shakeDir = hit.direction;
            Debug.Log($"{name} trung don #{HitCount}: sat thuong {hit.damage}, huong {hit.direction}", this);
        }

        void Update()
        {
            if (sprite == null) return;

            if (flashTimer > 0f) flashTimer -= Time.deltaTime;
            sprite.color = Color.Lerp(baseColor, hitColor, Mathf.Clamp01(flashTimer / flashTime));

            if (shakeTimer > 0f) shakeTimer -= Time.deltaTime;
            float k = Mathf.Clamp01(shakeTimer / shakeTime);
            // Lech theo huong don roi nay ve, giam dan.
            Vector3 offset = (Vector3)(shakeDir * shakeDistance * k * Mathf.Cos((1f - k) * Mathf.PI * 3f));
            sprite.transform.localPosition = spriteBasePos + offset;
        }
    }
}
