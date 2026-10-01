using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Cham vao la mat mau - kieu quai Hollow Knight. Gan len object co collider trigger
    /// (vd object con "Hitbox" cua quai). Bat tu sau khi trung do ben nhan don tu lo,
    /// nen o day cu ban don moi buoc vat ly ma khong can dem gio.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ContactDamage : MonoBehaviour
    {
        [Min(1)] public int damage = 1;
        [Tooltip("Chi gay sat thuong cho collider mang tag nay.")]
        public string targetTag = "Player";

        void OnTriggerStay2D(Collider2D other)
        {
            // Unity van goi ham va cham khi component bi tat, nen phai tu kiem tra.
            if (!enabled || !other.CompareTag(targetTag)) return;

            var target = other.GetComponentInParent<IDamageable>();
            if (target == null) return;

            // Day nan nhan ra xa: cung phia voi nan nhan so voi tam vung cham.
            float side = other.bounds.center.x >= transform.position.x ? 1f : -1f;
            target.TakeHit(new HitInfo
            {
                damage = damage,
                direction = new Vector2(side, 0f),
                knockback = 0f,
                source = gameObject
            });
        }
    }
}
