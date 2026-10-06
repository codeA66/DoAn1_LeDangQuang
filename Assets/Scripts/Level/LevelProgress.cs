using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>Luu man xa nhat da mo (PlayerPrefs) - dung cho menu chon man sau nay.</summary>
    public static class LevelProgress
    {
        const string Key = "LuHanh.UnlockedLevel";

        /// <summary>Chi so man xa nhat da mo (0 = man dau).</summary>
        public static int Unlocked => PlayerPrefs.GetInt(Key, 0);

        public static void Unlock(int index)
        {
            if (index <= Unlocked) return;
            PlayerPrefs.SetInt(Key, index);
            PlayerPrefs.Save();
        }
    }
}
