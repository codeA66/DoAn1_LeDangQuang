using LuHanh.Combat;
using LuHanh.Effects;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Thong so quai long dat co ky nang rieng (ban xa / chui dat / dap song). Moi loai mot asset;
    /// controller chi doc nhom thong so cua minh.
    /// </summary>
    [CreateAssetMenu(menuName = "LuHanh/Enemies/Buried Enemy Config", fileName = "BuriedEnemy")]
    public class BuriedEnemyConfig : EnemyConfigBase
    {
        [Header("Di chuyen / phat hien")]
        public float moveSpeed = 1.6f;
        [Tooltip("Thay nguoi choi trong khoang nay (unit). Nen xa hon tam chem cua nguoi choi.")]
        public float detectRange = 8f;
        public float detectHeight = 2.5f;
        [Tooltip("Do sau kiem tra mep vuc / khoang kiem tra tuong.")]
        public float edgeCheckDepth = 0.6f;
        public float wallCheckDistance = 0.15f;

        [Header("Animation (ten state trong Animator)")]
        public string idleState = "idle";
        public string walkState = "walk";
        public string attackState = "attack";
        [Tooltip("Chi quai chui dat dung.")]
        public string emergeState = "emerge";
        public string burrowState = "burrow";

        [Header("Ban xa")]
        [Tooltip("Giu khoang cach nay khi ban.")]
        public float preferredRange = 6f;
        [Tooltip("Nguoi choi gan hon khoang nay thi lui lai.")]
        public float retreatRange = 3f;
        [Tooltip("Bao truoc (giay) - dai de nguoi choi kip doc.")]
        public float shootWindup = 0.7f;
        public float shootRecover = 0.8f;
        public float shootCooldown = 1.6f;
        public Projectile projectilePrefab;
        public FrameAnimation projectileBurstPrefab;
        public float projectileSpeed = 9f;
        [Tooltip("Diem ban, tinh tu chan theo huong dang nhin.")]
        public Vector2 muzzleOffset = new Vector2(0.8f, 1.1f);

        [Header("Chui dat")]
        public float undergroundSpeed = 4.5f;
        [Tooltip("Ngam duoi dat toi khi cach nguoi choi khoang nay (unit) thi troi len.")]
        public float emergeDistance = 0.8f;
        [Tooltip("Ngam toi da bao lau roi troi len du chua toi.")]
        public float maxHuntTime = 3f;
        [Tooltip("Bui bao truoc tai cho se troi len (giay).")]
        public float emergeTelegraph = 0.7f;
        public float biteTime = 0.5f;
        [Tooltip("Lo than sau khi can - luc nguoi choi danh duoc.")]
        public float exposedTime = 1.6f;
        public float burrowTime = 0.5f;

        [Header("Dap dat + song xung kich")]
        public float slamRange = 3.2f;
        public float slamWindup = 0.9f;
        public float slamActive = 0.25f;
        public float slamRecover = 1.2f;
        public float slamCooldown = 2.2f;
        [Tooltip("Song xung kich chay doc mat dat ve hai phia. De trong = chi dap gan.")]
        public Projectile wavePrefab;
        public float waveSpeed = 9f;
        [Tooltip("Do cao tam song so voi chan.")]
        public float waveHeight = 0.45f;
    }
}
