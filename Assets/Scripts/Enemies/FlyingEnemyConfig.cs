using LuHanh.Combat;
using LuHanh.Effects;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Thong so quai bay ban dan (doi, kieu Aspid trong Hollow Knight).
    /// Phan chung (mau, trung don, ngu...) o <see cref="EnemyConfigBase"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "LuHanh/Flying Enemy Config", fileName = "FlyingEnemyConfig")]
    public class FlyingEnemyConfig : EnemyConfigBase
    {
        [Header("Bay")]
        [Tooltip("Toc do toi da khi danh nhau.")]
        public float maxSpeed = 4f;
        [Tooltip("Toc do toi da khi luon ranh.")]
        public float hoverSpeed = 1.5f;
        [Tooltip("Gia toc doi huong - nho = bay luot mem, lon = bay gat.")]
        public float acceleration = 10f;

        [Header("Luon ranh quanh cho xuat phat")]
        [Tooltip("Luon qua lai bao xa moi ben (unit).")]
        public float hoverRange = 2f;
        [Tooltip("Mot vong luon qua lai mat bao lau (giay).")]
        public float hoverPeriod = 4f;
        [Tooltip("Nhap nho len xuong (chi hinh anh).")]
        public float bobAmplitude = 0.25f;
        public float bobFrequency = 1.5f;

        [Header("Phat hien")]
        [Tooltip("Nguoi choi trong ban kinh nay va khong bi tuong che thi vao tran.")]
        public float detectRadius = 8f;
        [Tooltip("Nguoi choi xa hon khoang nay...")]
        public float loseRange = 12f;
        [Tooltip("...lien tuc chung nay giay thi bay ve cho cu.")]
        public float loseTime = 2f;

        [Header("Giu khoang cach khi danh")]
        public float keepDistanceMin = 4f;
        public float keepDistanceMax = 6f;
        [Tooltip("Bay cao hon nguoi choi bao nhieu.")]
        public float heightAbovePlayer = 3f;

        [Header("Ban cau")]
        public Projectile projectilePrefab;
        [Tooltip("Hieu ung no (FrameAnimation) khi cau trung dat / nguoi choi. Pool tao san 3 ban dung xoay vong.")]
        public FrameAnimation projectileBurstPrefab;
        public float shootInterval = 2.5f;
        [Tooltip("Dung lai rung nhe bao lau truoc khi ban - bao truoc de nguoi choi ne.")]
        public float telegraphTime = 0.4f;
        public float telegraphShake = 0.06f;
        public float projectileSpeed = 7f;
        [Tooltip("Diem ban tinh tu tam doi (X theo huong nhin).")]
        public Vector2 muzzleOffset = new Vector2(0.3f, -0.1f);
        [Tooltip("Nham vao diem cao bao nhieu tinh tu chan nguoi choi.")]
        public float aimHeight = 0.9f;
        public int poolPrewarm = 6;

        [Header("Animation (ten state trong Animator)")]
        public string flyState = "bat_fly";
        public string attackState = "bat_attack";
    }
}
