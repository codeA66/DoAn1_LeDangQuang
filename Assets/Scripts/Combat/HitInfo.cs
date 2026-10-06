using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>Thong tin mot don danh, truyen cho <see cref="IDamageable.TakeHit"/>.</summary>
    public struct HitInfo
    {
        /// <summary>Luong sat thuong.</summary>
        public int damage;
        /// <summary>Huong don danh (da chuan hoa) - huong muc tieu bi day ra.</summary>
        public Vector2 direction;
        /// <summary>Luc day muc tieu, unit/giay.</summary>
        public float knockback;
        /// <summary>Doi tuong gay ra don danh.</summary>
        public GameObject source;
        /// <summary>Phan ung cua nguoi choi khi trung don nay. Null = phan ung mac dinh cua nguoi choi.</summary>
        public HitReaction reaction;
    }
}
