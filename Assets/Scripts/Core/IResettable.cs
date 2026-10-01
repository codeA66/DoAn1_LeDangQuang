namespace LuHanh.Core
{
    /// <summary>
    /// Thu gi can tro ve trang thai ban dau khi nguoi choi hoi sinh (quai, cua, vat pha duoc...).
    /// Tu dang ky voi <see cref="RespawnManager"/> trong Awake, huy trong OnDestroy.
    /// </summary>
    public interface IResettable
    {
        void ResetState();
    }
}
