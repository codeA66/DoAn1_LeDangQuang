using LuHanh.Player;
using UnityEngine;

namespace LuHanh.Upgrades
{
    /// <summary>
    /// Mot nang cap mua bang Shards o NPC tho ren. Them nang cap moi: tao asset nay, them vao
    /// <see cref="UpgradeCatalog"/> - khong phai sua code (chi so da noi qua <see cref="PlayerStats"/>).
    /// </summary>
    [CreateAssetMenu(menuName = "LuHanh/Upgrades/Upgrade", fileName = "UP_New")]
    public class UpgradeDefinition : ScriptableObject
    {
        public enum Mode
        {
            /// <summary>Moi cap cong them valuePerLevel.</summary>
            Add,
            /// <summary>Moi cap nhan them valuePerLevel (vd 0.85 = giam 15%).</summary>
            Multiply,
        }

        [Tooltip("Khoa luu trong file save. KHONG doi sau khi da phat hanh (doi la mat cap da mua).")]
        public string id = "new_upgrade";
        [Tooltip("Ten hien trong game (tieng Anh).")]
        public string displayName = "New Upgrade";
        [TextArea, Tooltip("Mo ta hien trong bang nang cap (tieng Anh).")]
        public string description;
        public Sprite icon;

        [Header("Hieu ung")]
        public PlayerStat stat;
        public Mode mode = Mode.Add;
        public float valuePerLevel = 1f;

        [Header("Gia")]
        [Tooltip("Gia tung cap theo thu tu. So cap toi da = so phan tu.")]
        public int[] costs = { 50 };

        public int MaxLevel => costs != null ? costs.Length : 0;

        /// <summary>Cong hieu ung cua <paramref name="level"/> cap vao chi so (dung boi UpgradeStatSource).</summary>
        public void Collect(int level, PlayerStat target, ref float add, ref float multiply)
        {
            if (level <= 0 || target != stat) return;
            if (mode == Mode.Add) add += valuePerLevel * level;
            else multiply *= Mathf.Pow(valuePerLevel, level);
        }

        void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id)) id = name;
        }
    }
}
