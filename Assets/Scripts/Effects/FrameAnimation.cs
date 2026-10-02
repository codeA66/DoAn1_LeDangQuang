using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Phat mot day sprite trong <see cref="duration"/> giay: 1 lan roi tu an, hoac lap lai (loop).
    /// Dung cho hieu ung ve tay dang tung khung (khoi, no...). Khong can Animator/clip,
    /// khong Instantiate/Destroy: dat san trong prefab hoac pool, goi Play() khi can.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FrameAnimation : MonoBehaviour
    {
        [Tooltip("Cac khung phat lan luot tu dau toi cuoi.")]
        [SerializeField] Sprite[] frames;
        [Tooltip("Thoi gian phat het cac khung (giay). So khung/giay = so khung / duration.")]
        [SerializeField] float duration = 0.4f;
        [Tooltip("Phat lap lai mai (vd dan dang bay). Tat: phat 1 lan roi tu an.")]
        [SerializeField] bool loop;
        [Tooltip("Tu phat moi khi object duoc bat (vd dan lay ra tu pool).")]
        [SerializeField] bool playOnEnable;

        SpriteRenderer spriteRenderer;
        float timer;
        float playDuration;

        public bool IsPlaying => timer > 0f;
        public float Duration => duration;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = false;
        }

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        /// <summary>Phat lai tu khung dau. <paramref name="overrideDuration"/> &gt; 0 thi dung thoi gian nay.</summary>
        public void Play(float overrideDuration = 0f)
        {
            if (frames == null || frames.Length == 0) return;
            playDuration = overrideDuration > 0f ? overrideDuration : duration;
            timer = playDuration;
            spriteRenderer.sprite = frames[0];
            spriteRenderer.enabled = true;
        }

        public void Stop()
        {
            timer = 0f;
            if (spriteRenderer) spriteRenderer.enabled = false;
        }

        void Update()
        {
            if (timer <= 0f) return;
            timer -= Time.deltaTime;
            if (timer <= 0f && loop) timer += playDuration;
            if (timer <= 0f)
            {
                spriteRenderer.enabled = false;
                return;
            }

            float k = 1f - timer / Mathf.Max(0.001f, playDuration);   // 0 -> 1
            spriteRenderer.sprite = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(k * frames.Length))];
        }
    }
}
