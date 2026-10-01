namespace LuHanh.Combat
{
    /// <summary>
    /// Thu gi bi danh duoc: quai, binh vo, cong tac... Gan len cung object (hoac object cha)
    /// voi collider nam tren layer Enemy thi don chem cua nguoi choi se tim thay.
    /// </summary>
    public interface IDamageable
    {
        void TakeHit(HitInfo hit);
    }
}
