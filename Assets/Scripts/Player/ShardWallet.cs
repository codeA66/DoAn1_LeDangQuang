using System;
using LuHanh.Core;

namespace LuHanh.Player
{
    /// <summary>
    /// Tui tien Shards cua nguoi choi (luu trong <see cref="SaveGame"/>). Quai chet roi Shards (<see cref="ShardPool"/>),
    /// sau nay dung de nang ky nang.
    /// </summary>
    public static class ShardWallet
    {
        /// <summary>So Shards moi (goi moi khi doi).</summary>
        public static event Action<int> Changed;

        public static int Count => SaveGame.Data.shards;

        public static void Add(int amount)
        {
            if (amount == 0) return;
            SaveGame.Data.shards = Math.Max(0, SaveGame.Data.shards + amount);
            Changed?.Invoke(SaveGame.Data.shards);
        }

        /// <summary>Tru tien neu du (vd mua nang cap). Tra ve false neu khong du.</summary>
        public static bool TrySpend(int amount)
        {
            if (amount < 0 || SaveGame.Data.shards < amount) return false;
            SaveGame.Data.shards -= amount;
            Changed?.Invoke(SaveGame.Data.shards);
            SaveGame.Save();
            return true;
        }

        /// <summary>Lay het tien ra (khi chet, roi lai cho chet). Tra ve so vua lay.</summary>
        public static int TakeAll()
        {
            int n = SaveGame.Data.shards;
            SaveGame.Data.shards = 0;
            Changed?.Invoke(0);
            return n;
        }
    }
}
