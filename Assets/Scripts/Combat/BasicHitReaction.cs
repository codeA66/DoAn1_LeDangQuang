using LuHanh.Player;
using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Phan ung co ban kieu Nine Sols: khung hinh mot thoang, bi day lui nhe ra xa nguon don,
    /// nhan vat chop sang vai lan rat nhanh. Khong co bat tu.
    /// </summary>
    [CreateAssetMenu(menuName = "LuHanh/Hit Reaction/Basic", fileName = "HR_Basic")]
    public class BasicHitReaction : HitReaction
    {
        [Header("Khung hinh")]
        [Tooltip("Khung hinh dung lai bao lau (giay thuc). 0 = tat.")]
        public float hitStopTime = 0.08f;

        [Header("Day lui")]
        [Tooltip("Van toc bat ra: X theo huong ra xa nguon don, Y huong len.")]
        public Vector2 knockback = new Vector2(5f, 3f);
        [Tooltip("Mat dieu khien bao lau sau khi bi day.")]
        public float hurtTime = 0.15f;

        [Header("Dang bi trung (than)")]
        [Tooltip("Than ngua ra sau bao nhieu do (bi danh tu sau lung thi chui ve truoc), tu tro ve.")]
        public float leanAngle = 15f;
        [Tooltip("Than bep lai (x, y), tu tro ve 1.")]
        public Vector2 hurtSquash = new Vector2(0.85f, 1.1f);

        [Header("Chop sang")]
        public Color flashColor = Color.white;
        [Min(0)] public int flashCount = 2;
        [Tooltip("Moi lan sang bao lau (giay thuc).")]
        public float flashOnTime = 0.04f;
        [Tooltip("Khoang tat giua hai lan sang (giay thuc).")]
        public float flashOffTime = 0.03f;

        public override void Apply(PlayerHealth target, HitInfo hit)
        {
            target.HitStopFor(hitStopTime);
            target.Knockback(hit.direction, knockback, hurtTime);
            target.HurtPose(hit.direction, hurtSquash, leanAngle);
            target.Flash(flashColor, flashCount, flashOnTime, flashOffTime);
        }
    }
}
