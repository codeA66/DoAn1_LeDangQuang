using System;
using System.Collections.Generic;

namespace LuHanh.Player
{
    /// <summary>Cac chi so cua nguoi choi co the bi tang/giam boi nguon ben ngoai (nang cap, charm, buff...).</summary>
    public enum PlayerStat
    {
        /// <summary>Sat thuong moi nhat chem (goc: PlayerAttackConfig.damage).</summary>
        AttackDamage,
        /// <summary>Thoi gian hoi giua hai nhat chem, giay (goc: PlayerAttackConfig.cooldown).</summary>
        AttackCooldown,
        /// <summary>So mask mau toi da (goc: Health.maxHealth tren Player).</summary>
        MaxMasks,
    }

    /// <summary>
    /// Mot nguon thay doi chi so. Moi nguon cong them vao <paramref name="add"/> va/hoac nhan vao <paramref name="multiply"/>
    /// cho chi so <paramref name="stat"/>. Khong lien quan thi de nguyen.
    /// </summary>
    public interface IPlayerStatSource
    {
        void Collect(PlayerStat stat, ref float add, ref float multiply);
    }

    /// <summary>
    /// Noi DUY NHAT tinh chi so cuoi cung cua nguoi choi: (goc + tong cong) x tich nhan.
    /// Gia tri goc van nam o config/component nhu cu - KHONG sua ScriptableObject luc chay
    /// (Editor se ghi de vao asset). Them nguon moi (nang cap, charm...) chi can <see cref="Register"/>,
    /// khong phai sua PlayerAttack / PlayerHealth. Chua co nguon nao thi chi so = goc, gameplay khong doi.
    /// </summary>
    public static class PlayerStats
    {
        static readonly List<IPlayerStatSource> sources = new();

        /// <summary>Mot nguon vua dang ky / huy / doi gia tri: cac noi dung chi so can doc lai.</summary>
        public static event Action Changed;

        public static void Register(IPlayerStatSource source)
        {
            if (source == null || sources.Contains(source)) return;
            sources.Add(source);
            Changed?.Invoke();
        }

        public static void Unregister(IPlayerStatSource source)
        {
            if (sources.Remove(source)) Changed?.Invoke();
        }

        /// <summary>Nguon da dang ky doi gia tri (vd vua mua them mot cap nang cap).</summary>
        public static void NotifyChanged() => Changed?.Invoke();

        public static float Apply(PlayerStat stat, float baseValue)
        {
            float add = 0f, multiply = 1f;
            for (int i = 0; i < sources.Count; i++) sources[i].Collect(stat, ref add, ref multiply);
            return (baseValue + add) * multiply;
        }

        public static int ApplyInt(PlayerStat stat, int baseValue) =>
            (int)Math.Round(Apply(stat, baseValue), MidpointRounding.AwayFromZero);

        // Vao Play khong reload domain: xoa nguon cua lan choi truoc.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            sources.Clear();
            Changed = null;
        }
    }
}
