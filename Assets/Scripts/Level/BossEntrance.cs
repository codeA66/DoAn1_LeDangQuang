using LuHanh.Core;
using LuHanh.Enemies;
using LuHanh.UI;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Boss xuat hien kieu Hollow Knight: RoomEncounter bat boss o tren cao, boss roi xuong;
    /// cham dat thi bui bung ra, rung manh, khung hinh khung lai, hien ten boss. Boss chi danh sau khi ten hien xong.
    /// Nguoi choi chet: boss ve cho cu (tren cao) qua ResetState cua EnemyBase, lan sau roi lai.
    /// </summary>
    public class BossEntrance : MonoBehaviour, IResettable
    {
        [SerializeField] FoundryWardenController boss;
        [SerializeField] BossTitleCard title;
        [Tooltip("Bui bung ra o chan boss luc cham dat (Emit).")]
        [SerializeField] ParticleSystem landDust;
        [SerializeField] int landDustCount = 60;
        [SerializeField] float landShake = 0.7f;
        [SerializeField] float landShakeTime = 0.6f;
        [Tooltip("Khung hinh dung lai luc cham dat (giay thuc).")]
        [SerializeField] float landHitStop = 0.12f;
        [Tooltip("Van toc roi (unit/giay) phai vuot qua thi moi tinh la dang roi.")]
        [SerializeField] float fallSpeed = 2f;

        Rigidbody2D body;
        bool wasActive, falling, landed;

        void Awake()
        {
            RespawnManager.Register(this);
            if (boss) body = boss.GetComponent<Rigidbody2D>();
        }

        void OnDestroy() => RespawnManager.Unregister(this);

        public void ResetState() => wasActive = falling = landed = false;

        void Update()
        {
            if (!boss || !body || landed) return;
            bool active = boss.gameObject.activeInHierarchy;
            if (active && !wasActive) boss.DelayAttacks(float.MaxValue); // dung yen trong luc roi
            wasActive = active;
            if (!active) return;

            float vy = body.linearVelocity.y;
            if (vy < -fallSpeed) falling = true;
            else if (falling && vy > -0.1f) Land();
        }

        void Land()
        {
            landed = true;
            if (landDust)
            {
                landDust.transform.position = boss.transform.position;
                landDust.Emit(landDustCount);
            }
            CameraShake.Kick(landShake, landShakeTime);
            if (landHitStop > 0f) HitStop.Request(landHitStop);
            if (title)
            {
                title.Show();
                boss.DelayAttacks(title.Duration);
            }
            else boss.DelayAttacks(0f);
        }
    }
}
