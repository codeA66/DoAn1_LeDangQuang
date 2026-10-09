using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Vung chet (day ho, gai...). Gan len object co collider trigger. Tac dung len moi thu co
    /// IHazardReceiver: nguoi choi mat mau va ve cho dung an toan (PlayerHealth.TakeHazardHit),
    /// quai chet ngay (EnemyBase.TakeHazardHit).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HazardZone : MonoBehaviour
    {
        [Tooltip("Mau mat khi roi vao. 0 = chi dua ve cho dung an toan, khong mat mau (vd gai o phong thu gian).")]
        [Min(0)] public int damage = 1;

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void OnTriggerEnter2D(Collider2D other)
        {
            // Bo qua trigger (hitbox don danh, vung cam bien...): chi than that moi roi vao bay.
            if (!enabled || other.isTrigger) return;
            var receiver = other.GetComponentInParent<IHazardReceiver>();
            receiver?.TakeHazardHit(damage);
        }

        void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            if (!box) return;
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.offset, box.size);
        }
    }
}
