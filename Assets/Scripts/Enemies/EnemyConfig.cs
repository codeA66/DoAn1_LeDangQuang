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

        [Header("Don lao (dash) - chi gay sat thuong luc dang lao")]
        [Tooltip("Nguoi choi o phia truoc trong khoang nay (unit) thi bat dau gong de lao.")]
        public float dashTriggerRange = 5f;
        [Tooltip("Gong truoc khi lao (giay) - khoang thoi gian nguoi choi kip ne.")]
        public float dashWindup = 0.45f;
        [Tooltip("Van toc lao (unit/giay).")]
        public float dashSpeed = 14f;
        [Tooltip("Lao trong bao lau (giay). Quang duong ~ dashSpeed x dashTime.")]
        public float dashTime = 0.3f;
        [Tooltip("Dung lai thoi sau cu lao (giay).")]
        public float dashRecover = 0.6f;
        [Tooltip("Cho bao lau (giay, tinh tu luc het pha dung thoi) moi lao tiep duoc.")]
        public float dashCooldown = 1.5f;

        [Header("Bi danh bay")]
        [Tooltip("Bi chem ngang thi nay len voi van toc nay (0 = chi bi day ngang).")]
        public float knockbackUp = 4f;

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
