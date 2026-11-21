using System;
using System.Collections.Generic;
using LuHanh.Core;
using UnityEngine;

namespace LuHanh.World
{
    /// <summary>
    /// Du lieu ban do the gioi (so do 50 phong da duyet, HUONG_DAN_BUILD_LEVEL.md): moi phong mot o tren luoi,
    /// thuoc vung nao, manh ban do nao, loai gi (ghe, tram, ky nang, boss, bi mat...).
    /// Ban do kieu Hollow Knight: moi vung chia nhieu manh; chua co manh thi khu do khong hien (ke ca da di qua);
    /// co manh thi hien ca khu, phong da di qua sang hon, phong bi mat chi hien khi da vao.
    /// Co luu trong WorldState: "MapPiece/[id manh]" (da co manh), "Visited/[ma phong]" (da vao phong).
    /// </summary>
    [CreateAssetMenu(menuName = "LuHanh/World/World Map", fileName = "WorldMap")]
    public class WorldMapData : ScriptableObject
    {
        public enum RoomKind { Normal, Bench, Station, Skill, Boss, Secret, Exit }

        [Serializable]
        public class Zone
        {
            public string id = "Foundry";
            public string title = "Forgotten Foundry";
            [Tooltip("Goc cua vung tren luoi chung (o phong A01/B01 = goc + o cua no).")]
            public Vector2Int origin;
        }

        [Serializable]
        public class Room
        {
            public string id = "A01";
            public string zone = "Foundry";
            [Tooltip("O trong vung: x sang phai, y XUONG duoi (giong so do).")]
            public Vector2Int cell;
            public RoomKind kind;
            [Tooltip("Co tho ren (ban nang cap + manh ban do).")]
            public bool shop;
            [Tooltip("Thuoc manh ban do so may cua vung (1, 2, 3).")]
            public int piece = 1;
            [Tooltip("Phong noi voi phong nay (ve duong noi tren ban do).")]
            public string[] links = Array.Empty<string>();
        }

        [Serializable]
        public class Piece
        {
            [Tooltip("Khoa luu (vd Foundry.1). KHONG doi sau khi phat hanh.")]
            public string id = "Foundry.1";
            public string zone = "Foundry";
            public int index = 1;
            [Tooltip("Ten hien trong game (tieng Anh).")]
            public string displayName = "Forgotten Foundry Map I";
            [TextArea] public string description = "Charts the upper halls of the Foundry.";
            [Tooltip("Gia khi ban o tho ren (Shards). 0 = khong ban (tim trong phong).")]
            public int price;
            public Sprite icon;
        }

        public List<Zone> zones = new();
        public List<Room> rooms = new();
        public List<Piece> pieces = new();

        public const string PieceFlag = "MapPiece/";
        public const string VisitedFlag = "Visited/";

        public static bool HasPiece(string pieceId) => WorldState.Has(PieceFlag + pieceId);
        public static void GivePiece(string pieceId) => WorldState.Set(PieceFlag + pieceId);
        public static bool Visited(string roomId) => WorldState.Has(VisitedFlag + roomId);

        public Zone GetZone(string id) => zones.Find(z => z.id == id);
        public Room GetRoom(string id) => rooms.Find(r => r.id == id);
        public Piece GetPiece(string id) => pieces.Find(p => p.id == id);

        /// <summary>O cua phong tren luoi chung (goc vung + o trong vung).</summary>
        public Vector2Int GlobalCell(Room room)
        {
            var z = GetZone(room.zone);
            return (z != null ? z.origin : Vector2Int.zero) + room.cell;
        }

        /// <summary>Phong co hien tren ban do khong: da co manh cua khu do; phong bi mat thi phai da vao.</summary>
        public bool IsShown(Room room) =>
            HasPiece(room.zone + "." + room.piece) && (room.kind != RoomKind.Secret || Visited(room.id));
    }
}
