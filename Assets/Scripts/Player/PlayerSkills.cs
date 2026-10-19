using System;
using LuHanh.Core;

namespace LuHanh.Player
{
    /// <summary>Them ky nang moi: chi them ten vao enum (luu theo ten, khong can them truong vao SaveData).</summary>
    public enum Skill { WallCling, OrbShot }

    /// <summary>
    /// Ky nang mo khoa bang vat pham (<see cref="LuHanh.Level.SkillPickup"/>), luu trong
    /// <see cref="SaveData.unlockedSkills"/> theo ten enum. File luu cu (bool wallCling / orbShot) van doc duoc.
    /// </summary>
    public static class PlayerSkills
    {
        public static event Action<Skill> Unlocked;

        public static bool Has(Skill skill) => List.Contains(skill.ToString()) || LegacyHas(skill);

        public static void Unlock(Skill skill)
        {
            if (Has(skill)) return;
            List.Add(skill.ToString());
            SaveGame.Save();
            Unlocked?.Invoke(skill);
        }

        static System.Collections.Generic.List<string> List => SaveGame.Data.unlockedSkills ??= new();

        // File luu truoc khi co unlockedSkills.
        static bool LegacyHas(Skill skill) => skill switch
        {
            Skill.WallCling => SaveGame.Data.wallCling,
            Skill.OrbShot => SaveGame.Data.orbShot,
            _ => false
        };
    }
}
