namespace LuHanh.Combat
{
    /// <summary>
    /// Thu gi phan ung voi bay / ho sau (hien la nguoi choi). Khac don danh thuong:
    /// khong bi day lui ma bi dua ve cho dung an toan gan nhat, kieu Hollow Knight.
    /// </summary>
    public interface IHazardReceiver
    {
        void TakeHazardHit(int damage);
    }
}
