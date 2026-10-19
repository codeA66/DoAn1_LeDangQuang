using LuHanh.Player;
using UnityEngine;

namespace LuHanh.Upgrades
{
    /// <summary>
    /// Dat trong LevelEssentials/GameSystems: dang ky cac nang cap da mua lam nguon chi so cho <see cref="PlayerStats"/>.
    /// Chi so tinh lai moi lan hoi (khong cache), nen mua xong la co hieu luc ngay.
    /// </summary>
    public class UpgradeSystem : MonoBehaviour, IPlayerStatSource
    {
        [SerializeField] UpgradeCatalog catalog;

        public UpgradeCatalog Catalog => catalog;

        void Awake()
        {
            if (!catalog) Debug.LogError("UpgradeSystem chua gan UpgradeCatalog.", this);
        }

        void OnEnable() => PlayerStats.Register(this);
        void OnDisable() => PlayerStats.Unregister(this);

        public void Collect(PlayerStat stat, ref float add, ref float multiply)
        {
            if (!catalog || catalog.upgrades == null) return;
            foreach (var def in catalog.upgrades)
                if (def) def.Collect(PlayerUpgrades.Level(def), stat, ref add, ref multiply);
        }
    }
}
