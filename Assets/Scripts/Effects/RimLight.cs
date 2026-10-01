using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Vien sang mong quanh sprite (rim light) de nhan vat toi mau van tach khoi nen toi.
    /// Cach lam: 8 ban sao cua sprite to mot mau phang (shader LuHanh/SpriteFlash), lech ra
    /// 8 huong mot khoang nho va nam SAU sprite goc -> chi lo phan mep. Ban sao duoc tao
    /// mot lan trong Awake; LateUpdate chep khung hinh hien tai (sau Animator).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class RimLight : MonoBehaviour
    {
        [Tooltip("Material dung shader LuHanh/SpriteFlash, mau vien dat o Flash Color.")]
        [SerializeField] Material rimMaterial;
        [Tooltip("Do day vien (unit).")]
        [SerializeField] float thickness = 0.02f;
        [Range(0f, 1f)] [SerializeField] float opacity = 0.5f;

        SpriteRenderer source;
        SpriteRenderer[] copies;

        void Awake()
        {
            source = GetComponent<SpriteRenderer>();
            if (!rimMaterial) return;

            copies = new SpriteRenderer[8];
            for (int i = 0; i < copies.Length; i++)
            {
                float a = i * Mathf.PI / 4f;
                var go = new GameObject("Rim" + i);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * thickness;

                var r = go.AddComponent<SpriteRenderer>();
                r.sharedMaterial = rimMaterial;
                r.sortingLayerID = source.sortingLayerID;
                r.sortingOrder = source.sortingOrder - 1;
                r.color = new Color(1f, 1f, 1f, opacity);
                copies[i] = r;
            }
        }

        void LateUpdate()
        {
            if (copies == null) return;
            bool visible = source.enabled;   // nhap nhay bat tu thi vien tat theo
            for (int i = 0; i < copies.Length; i++)
            {
                var r = copies[i];
                r.enabled = visible;
                r.sprite = source.sprite;
                r.flipX = source.flipX;
                r.flipY = source.flipY;
            }
        }
    }
}
