using System.Collections.Generic;
using UnityEngine;

namespace LuHanh.Core
{
    /// <summary>
    /// Quai da giet trong the gioi moi phong 1 scene: ra vao phong quai van nam yen (khong cay tien bang
    /// cach di di ve ve). Ngoi ghe hoac chet thi xoa het -> quai ca vung hoi lai. Chi giu trong luc choi,
    /// khong luu file (vao lai game luon bat dau o ghe nen quai cung hoi het).
    /// Quai do phong khoa (RoomEncounter) va boss khong dung danh sach nay.
    /// </summary>
    public static class WorldKills
    {
        static readonly HashSet<string> killed = new();

        public static void Add(string id) => killed.Add(id);
        public static bool Has(string id) => killed.Contains(id);
        public static void Clear() => killed.Clear();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => killed.Clear();
    }
}
