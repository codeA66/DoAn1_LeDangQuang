using UnityEngine;

namespace LuHanh.Upgrades
{
    /// <summary>Danh sach nang cap ban o tho ren, theo thu tu hien trong bang.</summary>
    [CreateAssetMenu(menuName = "LuHanh/Upgrades/Catalog", fileName = "UpgradeCatalog")]
    public class UpgradeCatalog : ScriptableObject
    {
        public UpgradeDefinition[] upgrades;
    }
}
