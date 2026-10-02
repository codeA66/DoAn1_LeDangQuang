using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Vung chet (day ho, gai...). Gan len object co collider trigger; nguoi choi cham vao
    /// thi mat mau va duoc dua ve cho dung an toan (xem PlayerHealth.TakeHazardHit).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HazardZone : MonoBehaviour
    {
        [Min(1)] public int damage = 1;
        [Tooltip("Chi tac dung len collider mang tag nay.")]
        public string targetTag = "Player";

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled || !other.CompareTag(targetTag)) return;
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
