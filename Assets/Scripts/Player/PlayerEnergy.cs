using System;
using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Thanh nang luong cho ky nang ban cau (kieu Soul cua Hollow Knight): chem trung quai +1 nac,
    /// moi phat ban ton <see cref="shotCost"/> nac. Chet thi ve 0. Chi co tac dung khi da mo ky nang ban.
    /// </summary>
    public class PlayerEnergy : MonoBehaviour, IResettable
    {
        [SerializeField, Min(1)] int maxEnergy = 6;
        [SerializeField, Min(1)] int shotCost = 2;
        [SerializeField, Min(0)] int gainPerHit = 1;

        PlayerAttack attack;

        public int Max => maxEnergy;
        public int Current { get; private set; }
        public int ShotCost => shotCost;
        public bool CanShoot => Current >= shotCost;

        /// <summary>(hien tai, toi da).</summary>
        public event Action<int, int> Changed;

        void Awake()
        {
            attack = GetComponent<PlayerAttack>();
            RespawnManager.Register(this);
        }

        void OnEnable() { if (attack) attack.EnemyHit += OnEnemyHit; }
        void OnDisable() { if (attack) attack.EnemyHit -= OnEnemyHit; }
        void OnDestroy() => RespawnManager.Unregister(this);

        void OnEnemyHit()
        {
            if (PlayerSkills.Has(Skill.OrbShot)) Set(Current + gainPerHit);
        }

        public bool TrySpendShot()
        {
            if (!CanShoot) return false;
            Set(Current - shotCost);
            return true;
        }

        public void Fill() => Set(maxEnergy);

        // Nguoi choi chet: mat het nang luong.
        public void ResetState() => Set(0);

        void Set(int value)
        {
            value = Mathf.Clamp(value, 0, maxEnergy);
            if (value == Current) return;
            Current = value;
            Changed?.Invoke(Current, maxEnergy);
        }
    }
}
