using UnityEngine;

namespace LuHanh.Enemies
{
    public enum WardenAttack { RockThrow, Ram, DoubleSlam, Vent }

    [CreateAssetMenu(menuName = "LuHanh/Enemies/Foundry Warden", fileName = "FoundryWarden")]
    public sealed class FoundryWardenConfig : EnemyConfigBase
    {
        [System.Serializable]
        public sealed class AnimationSet
        {
            [Tooltip("Các frame theo thứ tự phát.")]
            public Sprite[] frames;
            [Tooltip("Hệ số chiều ngang đã đo từ frame đứng đầu, giữ kích thước boss giữa các bộ ảnh.")]
            public float widthScale = 1f;
        }

        [System.Serializable]
        public sealed class AttackTiming
        {
            [Tooltip("Frame bắt đầu va chạm/phóng hiệu ứng, đếm từ 0; đo từ bộ ảnh đã duyệt.")]
            public int impactFrame = 5;
            [Tooltip("Frame bắt đầu thu tay, đếm từ 0.")]
            public int recoveryFrame = 8;
            [Tooltip("Tâm găng tại frame va chạm, tính từ pivot chân theo unit; đã bao gồm hệ số ngang của clip.")]
            public Vector2 strikeSource;
        }

        [Header("Hoạt ảnh")]
        [Tooltip("Idle 6 frame.")][SerializeField] AnimationSet idle = new();
        [Tooltip("Ném đá, húc ngang, đập hai tay, xả áp; mỗi bộ 10 frame.")]
        [SerializeField] AnimationSet[] attacks = new AnimationSet[4];
        [Tooltip("Bật timeline từng chiêu sau khi duyệt preview; tách thời gian tay đập khỏi thời gian sóng tồn tại.")]
        [SerializeField] bool synchronizedAttacks;
        [Tooltip("Mốc ảnh riêng: ném đá, húc, đập hai tay, xả áp.")]
        [SerializeField] AttackTiming[] attackTimings =
        {
            new(), new(), new(), new() { impactFrame = 4 }
        };
        [Tooltip("Hurt 4 frame.")][SerializeField] AnimationSet hurt = new();
        [Tooltip("Die 10 frame.")][SerializeField] AnimationSet die = new();
        [Tooltip("Tốc độ Idle, frame/giây.")][SerializeField, Min(0.1f)] float idleFps = 6f;
        [Tooltip("Thời gian Hurt (giây).")][SerializeField, Min(0.01f)] float hurtDuration = 0.2f;
        [Tooltip("Thời gian Die trước khi trả boss về trạng thái tắt (giây).")]
        [SerializeField, Min(0.01f)] float deathDuration = 1.5f;

        [Header("Chu kỳ chiến đấu")]
        [Tooltip("Khoảng cách phát hiện Player, unit.")][SerializeField, Min(0.1f)] float detectRange = 14f;
        [Tooltip("Sát thương mỗi lần tung đòn; cả đòn chỉ trúng Player tối đa một lần.")]
        [SerializeField, Min(1)] int attackDamage = 2;
        [Tooltip("Thời gian báo trước đòn (giây).")][SerializeField, Min(0.01f)] float chargeDuration = 0.8f;
        [Tooltip("Thời gian hồi sau đòn (giây).")][SerializeField, Min(0f)] float recoveryDuration = 1f;
        [Tooltip("Thời gian nghỉ giữa hai đòn (giây).")][SerializeField, Min(0f)] float restDuration = 1f;
        [Tooltip("Thời gian bật vùng đánh đập/quét (giây).")][SerializeField, Min(0.01f)] float meleeDuration = 0.25f;

        [Header("Chu kỳ khiên")]
        [Tooltip("Bật sau khi duyệt preview: 5 giây bất tử, 3 giây nhận sát thương; skill chạy ở cả hai pha.")]
        [SerializeField] bool shieldCycleEnabled;
        [Tooltip("Số giây bất tử kể từ lúc boss được bật, kể cả Player ở xa.")]
        [SerializeField, Min(0.01f)] float shieldDuration = 5f;
        [Tooltip("Số giây nhận sát thương sau khi khiên tắt.")]
        [SerializeField, Min(0.01f)] float vulnerableDuration = 3f;

        [Header("Vùng đánh, tính bằng unit")]
        [Tooltip("Đường kính hình đá và vùng va chạm, unit.")]
        [SerializeField, Min(0.01f)] float rockDiameter = 1.5f;
        [Tooltip("Thời gian bay tới vị trí Player đã chốt khi thả đá, giây.")]
        [SerializeField, Min(0.01f)] float rockFlightDuration = 1.2f;
        [Tooltip("Độ nâng cung so với đường nối điểm thả và điểm đích, unit.")]
        [SerializeField, Min(0f)] float rockArcHeight = 6f;
        [Tooltip("Vận tốc húc ngang, unit/giây.")][SerializeField, Min(0.1f)] float ramSpeed = 20f;
        [Tooltip("Quãng húc tối đa; dừng sớm khi gặp tường hoặc mép vực.")]
        [SerializeField, Min(0.1f)] float ramDistance = 12f;
        [Tooltip("Kích thước mỗi sóng đất; có hai sóng đi về hai phía.")]
        [SerializeField] Vector2 waveSize = new(1.5f, 1f);
        [Tooltip("Tốc độ sóng đất, unit/giây.")][SerializeField, Min(0.1f)] float waveSpeed = 8f;
        [Tooltip("Quãng đường tối đa của sóng, unit.")][SerializeField, Min(0.1f)] float waveDistance = 10f;
        [Tooltip("Kích thước cột xả áp.")][SerializeField] Vector2 ventSize = new(1.5f, 3f);
        [Tooltip("Các vị trí cố định so với tâm boss lúc bắt đầu chiêu, không đuổi Player.")]
        [SerializeField] float[] ventOffsets = { -6f, 0f, 6f };
        [Tooltip("Thời gian cột xả áp tồn tại (giây).")][SerializeField, Min(0.01f)] float ventDuration = 1f;
        [Tooltip("Độ trễ giữa đợt giữa và các cặp cột lan ra hai bên, giây.")]
        [SerializeField, Min(0.01f)] float ventWaveInterval = 0.25f;

        public float VentDelay(int index)
        {
            int rank = 0;
            float distance = Mathf.Abs(ventOffsets[index]);
            // Đếm khoảng cách nhỏ hơn, bỏ các cặp đối xứng trùng khoảng cách.
            for (int i = 0; i < ventOffsets.Length; i++)
            {
                float d = Mathf.Abs(ventOffsets[i]);
                if (d >= distance - 0.001f) continue;
                bool duplicate = false;
                for (int j = 0; j < i; j++)
                    if (Mathf.Abs(Mathf.Abs(ventOffsets[j]) - d) < 0.001f) { duplicate = true; break; }
                if (!duplicate) rank++;
            }
            return rank * ventWaveInterval;
        }
        float LastVentDelay
        {
            get
            {
                float delay = 0f;
                for (int i = 0; i < ventOffsets.Length; i++) delay = Mathf.Max(delay, VentDelay(i));
                return delay;
            }
        }

        public AnimationSet Idle => idle;
        public AnimationSet Hurt => hurt;
        public AnimationSet Die => die;
        public AnimationSet Attack(WardenAttack kind) => attacks[(int)kind];
        public bool SynchronizedAttacks => synchronizedAttacks;
        public AttackTiming Timing(WardenAttack kind) => attackTimings[(int)kind];
        public float BodyMotionDuration(WardenAttack kind) => kind == WardenAttack.DoubleSlam || kind == WardenAttack.RockThrow ?
            meleeDuration : ActiveDuration(kind);
        public float IdleFps => idleFps;
        public float HurtDuration => hurtDuration;
        public float DeathDuration => deathDuration;
        public float DetectRange => detectRange;
        public int AttackDamage => attackDamage;
        public float ChargeDuration => chargeDuration;
        public float RecoveryDuration => recoveryDuration;
        public float RestDuration => restDuration;
        public float MeleeDuration => meleeDuration;
        public bool ShieldCycleEnabled => shieldCycleEnabled;
        public float ShieldDuration => shieldDuration;
        public float VulnerableDuration => vulnerableDuration;
        public float RockDiameter => rockDiameter;
        public float RockFlightDuration => rockFlightDuration;
        public float RockArcHeight => rockArcHeight;
        public float RamSpeed => ramSpeed;
        public float RamDistance => ramDistance;
        public Vector2 WaveSize => waveSize;
        public float WaveSpeed => waveSpeed;
        public float WaveDistance => waveDistance;
        public Vector2 VentSize => ventSize;
        public float[] VentOffsets => ventOffsets;
        public float VentDuration => ventDuration;
        public float VentWaveInterval => ventWaveInterval;
        public float ActiveDuration(WardenAttack kind) => kind == WardenAttack.Vent ? ventDuration + LastVentDelay :
            kind == WardenAttack.DoubleSlam ? waveDistance / waveSpeed :
            kind == WardenAttack.Ram ? ramDistance / ramSpeed : rockFlightDuration;
    }
}
