using System;
using LuHanh.Core;

namespace LuHanh.Player
{
    public enum Skill { WallCling, OrbShot }

    /// <summary>Ky nang mo khoa bang vat pham (<see cref="LuHanh.Level.SkillPickup"/>), luu trong <see cref="SaveGame"/>.</summary>
    public static class PlayerSkills
    {
        public static event Action<Skill> Unlocked;

        public static bool Has(Skill skill) => skill switch
        {
            Skill.WallCling => SaveGame.Data.wallCling,
            Skill.OrbShot => SaveGame.Data.orbShot,
            _ => false
        };

        public static void Unlock(Skill skill)
        {
            if (Has(skill)) return;
            if (skill == Skill.WallCling) SaveGame.Data.wallCling = true;
            else if (skill == Skill.OrbShot) SaveGame.Data.orbShot = true;
            SaveGame.Save();
            Unlocked?.Invoke(skill);
        }
    }
}
