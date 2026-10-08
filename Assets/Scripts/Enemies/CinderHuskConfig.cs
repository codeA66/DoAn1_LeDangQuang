using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>Thong so va cac khung hinh cho quai may Cinder Husk.</summary>
    [CreateAssetMenu(menuName = "LuHanh/Enemies/Cinder Husk Config", fileName = "CinderHusk")]
    public sealed class CinderHuskConfig : EnemyConfigBase
    {
        [Header("Animation frames")]
        [Tooltip("Bon khung idle co nhap nho nhe; controller phat theo vong ping-pong.")]
        [SerializeField] Sprite[] idleFrames;
        [Tooltip("Cac khung bi trung don.")]
        [SerializeField] Sprite[] hurtFrames;
        [Tooltip("Sau Hurt: ba khung tu luc giong, mot khung no, hai khung hoi phuc.")]
        [SerializeField] Sprite[] attackFrames;
        [Tooltip("Sau khi chet: ba khung truoc no, khung no lon, hai khung tan.")]
        [SerializeField] Sprite[] dieFrames;

        [Header("Animation timing")]
        [Tooltip("Toc do lap animation Idle, tinh bang khung moi giay.")]
        [SerializeField, Min(0.1f)] float idleFramesPerSecond = 5f;
        [Tooltip("Do dai Hurt truoc khi tro lai Idle (giay); bi danh khong kich hoat ban.")]
        [SerializeField, Min(0.01f)] float hurtDuration = 0.15f;
        [Tooltip("Thoi gian tu luc khi Player vao tam truoc khi ban ngang (giay).")]
        [SerializeField, Min(0.05f)] float attackChargeDuration = 0.65f;
        [Tooltip("Thoi gian giu khung no thuong (giay).")]
        [SerializeField, Min(0.01f)] float attackBurstDuration = 0.12f;
        [Tooltip("Thoi gian hoi phuc sau vu no thuong (giay).")]
        [SerializeField, Min(0f)] float attackRecoveryDuration = 0.35f;
        [Tooltip("Do tre truoc khi vu no khi chet gay sat thuong (giay).")]
        [SerializeField, Min(0f)] float deathBurstDelay = 0.28f;
        [Tooltip("Tong thoi gian giu animation chet truoc khi an object (giay).")]
        [SerializeField, Min(0.1f)] float deathCleanupTime = 0.75f;

        [Header("Explosion")]
        [Tooltip("Sat thuong moi vu no len Player.")]
        [SerializeField, Min(1)] int blastDamage = 3;
        [Tooltip("Ban kinh vu no sau khi bi Player danh (Unity units).")]
        [SerializeField, Min(0.1f)] float attackBlastRadius = 2.5f;
        [Tooltip("Ban kinh vu no khi chet; mac dinh gap doi vu no thuong.")]
        [SerializeField, Min(0.1f)] float deathBlastRadius = 5f;
        [Tooltip("He so phong to sprite trong khung no thuong.")]
        [SerializeField, Min(1f)] float attackBurstScale = 2f;
        [Tooltip("He so phong to sprite trong khung no khi chet; gap doi Attack Burst Scale.")]
        [SerializeField, Min(1f)] float deathBurstScale = 4f;

        [Header("Loi truy duoi sau khi chet")]
        [Tooltip("Thoi gian loi bam Player truoc khi no, giay.")]
        [SerializeField, Min(0.1f)] float coreFuseDuration = 2f;
        [Tooltip("Toc do loi di thang ve Player va xuyen moi vat can, unit/giay.")]
        [SerializeField, Min(0f)] float coreChaseSpeed = 4f;
        [Tooltip("Ban kinh hinh loi de can diem xuat hien tren san; loi truy duoi khong va cham vat can.")]
        [SerializeField, Min(0.05f)] float coreCollisionRadius = 0.25f;
        [Tooltip("Thong so cu duoc giu de bao toan du lieu; loi hien di xuyen vat can nen khong dung.")]
        [SerializeField, Min(0.001f)] float coreWallSkin = 0.02f;
        [Tooltip("Thoi gian lua bung khi tan cong, giay.")]
        [SerializeField, Min(0.1f)] float fireBurstDuration = 0.6f;

        public float CoreFuseDuration => coreFuseDuration;
        [Header("Cau lua ngang")]
        [Tooltip("Player trong khoang cach nay va cung dai cao duong dan thi quai tu tu luc; tuong che se khong kich hoat.")]
        [SerializeField, Min(0.1f)] float attackRange = 10f;
        [Tooltip("Sat thuong cau lua ban trung Player, tach rieng sat thuong loi no.")]
        [SerializeField, Min(1)] int fireballDamage = 2;
        public float AttackRange => attackRange;
        public int FireballDamage => fireballDamage;
        [Tooltip("Ban kinh va cham cau lua, Unity units.")]
        [SerializeField, Min(0.05f)] float fireballRadius = 0.35f;
        [Tooltip("Toc do cau lua bay ngang, unit/giay.")]
        [SerializeField, Min(0.1f)] float fireballSpeed = 8f;
        [Tooltip("Tu tat sau so giay nay neu khong cham gi.")]
        [SerializeField, Min(0.1f)] float fireballLifetime = 2f;
        public float FireballRadius => fireballRadius;
        public float FireballSpeed => fireballSpeed;
        public float FireballLifetime => fireballLifetime;
        [Tooltip("Thoi gian lua/khoi bung khi cau lua cham tuong, giay; chi VFX.")]
        [SerializeField, Min(0.1f)] float wallImpactDuration = 0.65f;
        [Tooltip("Kich thuoc dam lua va tuong so voi ban kinh cau lua; chi VFX.")]
        [SerializeField, Min(0.1f)] float wallImpactScale = 2f;
        public float WallImpactDuration => wallImpactDuration;
        public float WallImpactScale => wallImpactScale;
        public float CoreChaseSpeed => coreChaseSpeed;
        public float CoreCollisionRadius => coreCollisionRadius;
        public float CoreWallSkin => coreWallSkin;
        public float FireBurstDuration => fireBurstDuration;
        public Sprite[] IdleFrames => idleFrames;
        public Sprite[] HurtFrames => hurtFrames;
        public Sprite[] AttackFrames => attackFrames;
        public Sprite[] DieFrames => dieFrames;
        public float IdleFramesPerSecond => idleFramesPerSecond;
        public float HurtDuration => hurtDuration;
        public float AttackChargeDuration => attackChargeDuration;
        public float AttackBurstDuration => attackBurstDuration;
        public float AttackRecoveryDuration => attackRecoveryDuration;
        public float DeathBurstDelay => deathBurstDelay;
        public float DeathCleanupTime => deathCleanupTime;
        public int BlastDamage => blastDamage;
        public float AttackBlastRadius => attackBlastRadius;
        public float DeathBlastRadius => deathBlastRadius;
        public float AttackBurstScale => attackBurstScale;
        public float DeathBurstScale => deathBurstScale;
    }
}
