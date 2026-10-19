using System;
using System.Collections.Generic;
using LuHanh.Core;
using LuHanh.Player;

namespace LuHanh.Upgrades
{
    /// <summary>
    /// Cap nang cap nguoi choi da mua (luu trong <see cref="SaveData.upgrades"/>) va viec mua bang Shards.
    /// Hieu ung len chi so di qua <see cref="UpgradeSystem"/> -> <see cref="PlayerStats"/>, khong sua config.
    /// </summary>
    public static class PlayerUpgrades
    {
        public enum BuyResult { Ok, NotEnoughShards, Maxed, Invalid }

        /// <summary>Vua mua xong: (nang cap, cap moi).</summary>
        public static event Action<UpgradeDefinition, int> Purchased;

        static List<UpgradeEntry> Entries => SaveGame.Data.upgrades ??= new List<UpgradeEntry>();

        public static int Level(UpgradeDefinition def)
        {
            if (!def) return 0;
            foreach (var e in Entries) if (e.id == def.id) return Math.Min(e.level, def.MaxLevel);
            return 0;
        }

        public static bool IsMaxed(UpgradeDefinition def) => def && Level(def) >= def.MaxLevel;

        /// <summary>Gia cap tiep theo; -1 neu da toi da.</summary>
        public static int NextCost(UpgradeDefinition def) => IsMaxed(def) || !def ? -1 : def.costs[Level(def)];

        public static BuyResult Check(UpgradeDefinition def)
        {
            if (!def || def.MaxLevel == 0) return BuyResult.Invalid;
            if (IsMaxed(def)) return BuyResult.Maxed;
            return ShardWallet.Count >= NextCost(def) ? BuyResult.Ok : BuyResult.NotEnoughShards;
        }

        public static BuyResult TryBuy(UpgradeDefinition def)
        {
            var check = Check(def);
            if (check != BuyResult.Ok) return check;
            int cost = NextCost(def);
            // Gan cap truoc roi moi tru tien: TrySpend tu goi SaveGame.Save() -> luu ca hai cung luc.
            int level = Level(def) + 1;
            SetLevel(def.id, level);
            if (!ShardWallet.TrySpend(cost))
            {
                SetLevel(def.id, level - 1);
                return BuyResult.NotEnoughShards;
            }
            PlayerStats.NotifyChanged();
            Purchased?.Invoke(def, level);
            return BuyResult.Ok;
        }

        static void SetLevel(string id, int level)
        {
            foreach (var e in Entries)
                if (e.id == id) { e.level = level; return; }
            Entries.Add(new UpgradeEntry { id = id, level = level });
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Purchased = null;
    }
}
