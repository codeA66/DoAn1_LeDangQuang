using UnityEngine;

namespace LuHanh.Core
{
    /// <summary>
    /// Co trang thai the gioi luu trong <see cref="SaveData.worldFlags"/>: tuong da sap, can gat da gat, bi mat da nhat...
    /// Moi phong la mot scene va bi tai lai moi lan vao, nen vat nao can nho trang thai thi doc co nay o Start.
    /// Co cua mot object trong phong: <see cref="Key"/> = "TenScene/ten" (vd "A21/Wall").
    /// </summary>
    public static class WorldState
    {
        /// <summary>Khoa co cua <paramref name="owner"/>: ten scene chua no + "/" + <paramref name="flag"/>.</summary>
        public static string Key(Component owner, string flag) => owner.gameObject.scene.name + "/" + flag;

        public static bool Has(string key) => !string.IsNullOrEmpty(key) && SaveGame.Data.worldFlags.Contains(key);

        /// <summary>Bat co va ghi file luu ngay (de thoat game giua chung khong mat).</summary>
        public static void Set(string key)
        {
            if (string.IsNullOrEmpty(key) || SaveGame.Data.worldFlags.Contains(key)) return;
            SaveGame.Data.worldFlags.Add(key);
            SaveGame.Save();
        }

        /// <summary>Tien cho vat trong phong: co rong = vat khong luu trang thai (cach cu cua Level1-3).</summary>
        public static bool Has(Component owner, string flag) => !string.IsNullOrEmpty(flag) && Has(Key(owner, flag));

        public static void Set(Component owner, string flag)
        {
            if (!string.IsNullOrEmpty(flag)) Set(Key(owner, flag));
        }
    }
}
