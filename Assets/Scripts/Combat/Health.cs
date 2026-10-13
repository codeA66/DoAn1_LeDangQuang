using System;
using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Mau dung chung cho moi thu song/chet: nguoi choi, quai, boss.
    /// Chi giu so mau va ban su kien - khong biet gi ve bat tu, hieu ung hay hoi sinh;
    /// nhung viec do do component rieng (PlayerHealth, EnemyController) lam.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [SerializeField, Min(1)] int maxHealth = 5;

        public int Max => maxHealth;
        public int Current { get; private set; }
        public bool IsDead => Current <= 0;

        /// <summary>Ban ra moi khi mau doi: (hien tai, toi da).</summary>
        public event Action<int, int> Changed;
        /// <summary>Ban ra dung mot lan khi mau ve 0.</summary>
        public event Action Died;

        void Awake() => Current = maxHealth;

        /// <summary>Doi mau toi da (vd tu config cua quai) va hoi day.</summary>
        public void SetMax(int max)
        {
            maxHealth = Mathf.Max(1, max);
            Refill();
        }

        /// <summary>Tru mau. Tra ve false neu da chet tu truoc hoac luong &lt;= 0.</summary>
        public bool Damage(int amount)
        {
            if (IsDead || amount <= 0) return false;

            Current = Mathf.Max(0, Current - amount);
            Changed?.Invoke(Current, maxHealth);
            if (Current == 0) Died?.Invoke();
            return true;
        }

        /// <summary>Hoi <paramref name="amount"/> mau (khong vuot toi da). Da chet thi khong hoi.</summary>
        public void Heal(int amount)
        {
            if (IsDead || amount <= 0 || Current >= maxHealth) return;
            Current = Mathf.Min(maxHealth, Current + amount);
            Changed?.Invoke(Current, maxHealth);
        }

        public void Refill()
        {
            Current = maxHealth;
            Changed?.Invoke(Current, maxHealth);
        }
    }
}
