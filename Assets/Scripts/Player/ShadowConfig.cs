using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Thong so dang Shadow: thanh nang luong, cach bien hinh va suc manh khi dang Shadow.
    /// Tach ScriptableObject de can bang game ma khong sua code.
    /// </summary>
    [CreateAssetMenu(menuName = "LuHanh/Shadow Config", fileName = "ShadowConfig")]
    public class ShadowConfig : ScriptableObject
    {
        [Header("Thanh nang luong")]
        [Min(1f)] public float maxEnergy = 100f;
        [Tooltip("Moi hat nang luong cong bao nhieu.")]
        [Min(0f)] public float energyPerOrb = 5f;

        [Header("Bien hinh")]
        [Tooltip("Giu phim Shadow (Y) bao lau khi thanh day thi bien hinh (giay).")]
        [Min(0.05f)] public float holdTime = 0.8f;
        [Tooltip("Thoi gian o dang Shadow (giay). Thanh can dan trong thoi gian nay.")]
        [Min(0.5f)] public float duration = 8f;
        [Tooltip("Khung hinh dung lai mot thoang luc bien hinh (giay thuc).")]
        public float transformHitStop = 0.1f;

        [Header("Suc manh")]
        [Tooltip("Nhan toc do chay toi da.")]
        [Min(1f)] public float speedMultiplier = 1.4f;
        [Tooltip("Nhan kich thuoc vung chem va vet chem.")]
        [Min(1f)] public float attackRangeMultiplier = 1.5f;
        [Tooltip("Nhan sat thuong moi nhat chem.")]
        [Min(1)] public int damageMultiplier = 2;
        [Tooltip("Ban kinh tim quai de chem tu dinh vi (unit). 0 = tat.")]
        [Min(0f)] public float homingRadius = 6f;
        [Tooltip("Dung cach quai bao xa sau khi lao toi (unit).")]
        [Min(0.2f)] public float homingStopDistance = 1.1f;

        [Header("Hinh")]
        [Tooltip("Mau phu len than nhan vat khi dang Shadow.")]
        public Color bodyTint = new Color(0.55f, 0.85f, 0.85f, 1f);
        [Tooltip("Mau bong mo khi di chuyen o dang Shadow (#45DCC4).")]
        public Color afterimageColor = new Color(0.27f, 0.86f, 0.77f, 0.45f);
        [Tooltip("Giay giua hai bong mo khi dang di chuyen.")]
        [Min(0.02f)] public float afterimageInterval = 0.06f;
        [Tooltip("Bong mo mo dan trong bao lau.")]
        [Min(0.05f)] public float afterimageLife = 0.25f;
    }
}
