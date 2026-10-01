using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Thong so mot loai quai di bo (tuan tra + duoi theo). Moi loai quai mot asset,
    /// chinh duoc ngay luc dang Play. Phan chung (mau, trung don, ngu...) o <see cref="EnemyConfigBase"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "LuHanh/Enemy Config", fileName = "EnemyConfig")]
    public class EnemyConfig : EnemyConfigBase
    {
        [Header("Tuan tra")]
        public float patrolSpeed = 2f;

        [Header("Duoi theo")]
        public float chaseSpeed = 4.5f;
        [Tooltip("Thay nguoi choi o phia truoc trong khoang nay (unit) thi duoi.")]
        public float detectRange = 6f;
        [Tooltip("Chenh do cao toi da van tinh la thay.")]
        public float detectHeight = 2f;
        [Tooltip("Nguoi choi ra xa hon khoang nay...")]
        public float loseRange = 9f;
        [Tooltip("...trong chung nay giay thi bo cuoc, quay lai tuan tra.")]
        public float loseTime = 1.5f;
        [Tooltip("Gan hon khoang nay thi phat animation tan cong.")]
        public float attackAnimRange = 1.5f;

        [Header("Do mep vuc / tuong")]
        [Tooltip("Do sau kiem tra mep vuc phia truoc.")]
        public float edgeCheckDepth = 0.5f;
        [Tooltip("Khoang kiem tra tuong phia truoc, tinh tu mep collider.")]
        public float wallCheckDistance = 0.1f;

        [Header("Animation (ten state trong Animator)")]
        public string idleState = "ghoul_idle";
        public string walkState = "ghoul_walk";
        public string attackState = "ghoul_attack";
    }
}
