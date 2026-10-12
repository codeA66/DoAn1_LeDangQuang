using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Lop vien sau sprite chinh (object con, material LuHanh/SpriteOutline): moi frame chep sprite + lat hinh
    /// cua SpriteRenderer goc de vien chay theo animation. Dung cho quai de tach khoi nen toi.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteOutline : MonoBehaviour
    {
        [Tooltip("SpriteRenderer cua than quai (Visual).")]
        [SerializeField] SpriteRenderer source;

        SpriteRenderer outline;

        void Awake()
        {
            outline = GetComponent<SpriteRenderer>();
            if (!source && transform.parent) source = transform.parent.GetComponent<SpriteRenderer>();
        }

        void LateUpdate()
        {
            if (!source) return;
            outline.enabled = source.enabled;
            outline.sprite = source.sprite;
            outline.flipX = source.flipX;
            outline.flipY = source.flipY;
        }
    }
}
