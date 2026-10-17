using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>Thông số boss Rootwraith (màn 2). Đòn theo kiểu Grimm/Hollow Knight: dịch chuyển, lao chéo, bắn đạn ngang, gai trồi khắp sàn.</summary>
    [CreateAssetMenu(menuName = "LuHanh/Enemies/Rootwraith Config", fileName = "Rootwraith")]
    public class RootwraithConfig : EnemyConfigBase
    {
        [Header("Nhịp chung")]
        [Tooltip("Nghỉ giữa các đòn ở giai đoạn 1 / 2 (giây).")]
        public float restPhase1 = 1.1f;
        public float restPhase2 = 0.5f;
        [Tooltip("Tỉ lệ máu còn lại để sang giai đoạn 2.")]
        [Range(0.1f, 0.9f)] public float phase2At = 0.5f;
        [Tooltip("Thời gian clip Teleport_Out / Teleport_In (giây).")]
        public float teleportOutTime = 0.44f;
        public float teleportInTime = 0.44f;
        [Tooltip("Thời gian ẩn giữa hai lần hiện (giây).")]
        public float hiddenTime = 0.12f;
        [Tooltip("Hồi phục sau mỗi đòn trước khi dịch chuyển đi (giây) - khoảng hở để người chơi đánh.")]
        public float recoverTime = 0.55f;

        [Header("Sát thương")]
        public int contactDamage = 1;
        public int diveDamage = 2;
        public int spikeDamage = 2;
        public int orbDamage = 1;

        [Header("Lao chéo")]
        [Tooltip("Độ cao và khoảng cách ngang so với người chơi khi hiện ra trước cú lao.")]
        public float diveStartHeight = 7f;
        public float diveStartSide = 6.5f;
        public float diveWindup = 0.5f;
        public float diveSpeed = 26f;
        [Tooltip("Trượt ngang sau khi chạm sàn.")]
        public float slideDistance = 5f;
        public float slideSpeed = 20f;
        [Tooltip("Số cú lao liên tiếp ở giai đoạn 2.")]
        public int phase2DiveCount = 2;

        [Header("Bắn đạn ngang")]
        public float volleyWindup = 0.5f;
        public int volleyCountPhase1 = 3;
        public int volleyCountPhase2 = 5;
        public float volleyInterval = 0.28f;
        public float orbSpeed = 11f;
        [Tooltip("Hai làn đạn: cao / thấp, tính từ sàn.")]
        public float orbHighLane = 2.3f;
        public float orbLowLane = 0.7f;
        [Tooltip("Biên độ / tần số lượn sóng của đạn.")]
        public float orbWaveAmp = 0.25f;
        public float orbWaveFreq = 5f;
        public float orbRadius = 0.32f;

        [Header("Gai trồi")]
        public float pillarWindup = 0.55f;
        public float spikeWarn = 0.65f;
        public float spikeRise = 0.1f;
        public float spikeHold = 0.45f;
        public float spikeRetract = 0.2f;
        [Tooltip("Khoảng cách giữa các cây gai / chiều rộng mỗi cây / chiều cao.")]
        public float spikeSpacing = 1.25f;
        public float spikeWidth = 0.8f;
        public float spikeHeight = 2.2f;
        [Tooltip("Cứ N cây thì chừa một khe trống (kiểu Grimm).")]
        public int spikeGapEvery = 3;
        [Tooltip("Giai đoạn 2: đợt gai thứ hai lệch nửa ô, trồi sau đợt đầu.")]
        public float secondWaveDelay = 0.75f;

        [Header("Chuyển giai đoạn")]
        public float phaseShiftHeight = 4.5f;
        public float phaseShiftDuration = 3.2f;
        public float burstInterval = 0.38f;
        public int burstCount = 10;
        public float burstSpeed = 6.5f;

        [Header("Màn xuất hiện")]
        [Tooltip("Thời gian tàn đỏ/khói hút về giữa phòng trước khi boss hiện ra (giây).")]
        public float introGatherTime = 1.6f;
        [Tooltip("Thời gian boss gào sau khi hiện ra, trước khi hiện tên (giây).")]
        public float introRoarTime = 1.1f;
        [Tooltip("Độ rung mạnh nhất của màn xuất hiện.")]
        public float introShake = 0.7f;

        [Header("Rung màn hình")]
        public float diveLandShake = 0.55f;
        public float spikeShake = 0.4f;
        public float phaseShake = 0.35f;
        public float deathShake = 0.9f;
        public float hitStopOnLand = 0.06f;
    }
}
