using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Thong so chung cua moi loai quai. Config cua tung kieu quai (di bo, bay...)
    /// ke thua lop nay va them phan rieng.
    /// </summary>
    public abstract class EnemyConfigBase : ScriptableObject
    {
        [Header("Mau")]
        [Min(1)] public int maxHealth = 3;

        [Header("Don danh len nguoi choi")]
        [Tooltip("Phan ung cua nguoi choi khi trung don cua quai nay (day lui, chop...). De trong = phan ung mac dinh cua nguoi choi.")]
        public LuHanh.Combat.HitReaction hitReaction;

        [Header("Trung don")]
        [Tooltip("Thoi gian khung lai, bi day lui sau moi nhat chem.")]
        public float hurtTime = 0.2f;
        [Tooltip("Nhan luc day cua don chem len quai nay. Quai bay khong co trong luc/ma sat keo lai nen can nho hon.")]
        [Min(0f)] public float knockbackScale = 1f;
        public float flashTime = 0.12f;
        [Tooltip("Hinh bi bop lai khi trung don (X rong ra, Y dep xuong) roi bat ve 1.")]
        public Vector2 hitSquash = new Vector2(1.3f, 0.7f);
        [Tooltip("Thoi gian bat ve hinh dang cu (giay).")]
        public float squashTime = 0.1f;

        [Header("Phan thuong")]
        [Tooltip("So hat nang luong Shadow roi ra khi bi chem chet (roi vuc thi khong roi).")]
        [Min(0)] public int soulOrbs = 3;

        [Header("Dia hinh")]
        public LayerMask groundLayers = 1 << 8;

        [Header("Toi uu")]
        [Tooltip("Xa camera hon khoang nay thi quai ngu: khong chay AI, khong tinh vat ly.")]
        public float sleepDistance = 25f;

        [Header("Hinh")]
        [Tooltip("Ban ve goc quay sang phai.")]
        public bool artFacesRight = true;
    }
}
