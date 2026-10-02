using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>
    /// Hieu ung chem trung kieu Hollow Knight: loe sang trang, manh vun + dom cam, va chum vet chem
    /// trang sac nhon (vet ngan hien ngay tat nhanh, vet dai hien tre mot chut song lau hon).
    /// Chi co MOT ban dung chung: moi lan trung thi doi toi diem trung roi Emit,
    /// khong Instantiate/Destroy (particle Simulation Space = World nen hat cu van bay tiep).
    /// </summary>
    public class HitEffect : MonoBehaviour
    {
        [SerializeField] ParticleSystem flash;
        [SerializeField] ParticleSystem shards;
        [Tooltip("So manh vun moi lan trung (min, max - ca hai deu tinh).")]
        [SerializeField] Vector2Int shardCount = new Vector2Int(10, 14);
        [Tooltip("Dom cam tron to toa ra hinh quat. Shape = Circle, cung (Arc) mo theo huong chem.")]
        [SerializeField] ParticleSystem puffs;
        [SerializeField] Vector2Int puffCount = new Vector2Int(10, 14);
        [Tooltip("Mau quai: dom cam hien sau vet chem bao lau (giay) - vet chem phai thay truoc.")]
        [SerializeField] float puffDelay = 0.08f;
        [Tooltip("Giot mau nho, dam, van xa (cung luc voi Puffs). Shape = Circle + Arc.")]
        [SerializeField] ParticleSystem bloodFar;
        [SerializeField] Vector2Int bloodFarCount = new Vector2Int(14, 20);
        [Tooltip("Nghieng quat giot mau len tren bao nhieu do so voi huong chem (chem ngang) - giot bay len roi roi xuong thanh duong cong.")]
        [SerializeField] float bloodFarUpTilt = 25f;

        [Header("Vet chem ngan - hien ngay, tat nhanh")]
        [SerializeField] ParticleSystem shortStreaks;
        [SerializeField] Vector2Int shortCount = new Vector2Int(8, 12);
        [Tooltip("Chieu dai (unit).")]
        [SerializeField] Vector2 shortLength = new Vector2(0.6f, 1.4f);
        [Tooltip("Do day (unit).")]
        [SerializeField] Vector2 shortWidth = new Vector2(0.2f, 0.3f);
        [SerializeField] Vector2 shortLifetime = new Vector2(0.06f, 0.1f);
        [Tooltip("Phan vet toa quanh huong chem (con lai toa ngau nhien moi huong).")]
        [Range(0f, 1f)] [SerializeField] float shortAlongDirection = 0.7f;
        [Tooltip("Lech toi da so voi huong chem (do).")]
        [SerializeField] float shortSpread = 55f;
        [Tooltip("Khoang cach tu diem trung toi dau gan cua vet (unit) - rong thi vet rai thanh vong, khong tu mot cho.")]
        [SerializeField] Vector2 shortOffset = new Vector2(0.4f, 1.8f);
        [Tooltip("Mau vet ngan - hoi khac vet dai de phan biet.")]
        [SerializeField] Color shortColor = new Color(1f, 0.95f, 0.8f, 1f);

        [Header("Vet chem dai - hien tre, song lau hon")]
        [SerializeField] ParticleSystem longStreaks;
        [SerializeField] Vector2Int longCount = new Vector2Int(2, 3);
        [SerializeField] Vector2 longLength = new Vector2(3f, 6f);
        [SerializeField] Vector2 longWidth = new Vector2(0.2f, 0.3f);
        [SerializeField] Vector2 longLifetime = new Vector2(0.18f, 0.25f);
        [SerializeField] float longSpread = 5f;
        [Tooltip("Lech ngang toi da (vuong goc huong chem, unit) - cac vet dai song song tach nhau ra.")]
        [SerializeField] float longSideOffset = 0.7f;
        [Tooltip("Vet dai hien sau vet ngan bao lau (giay).")]
        [SerializeField] float longDelay = 0.04f;

        // Hang doi nho cho cac lop phat tre (vet dai, dom cam) - mang co dinh, khong cap phat.
        enum Layer : byte { LongStreaks, Puffs }
        const int MaxPending = 8;
        readonly float[] pendingTime = new float[MaxPending];
        readonly Vector2[] pendingPoint = new Vector2[MaxPending];
        readonly float[] pendingAngle = new float[MaxPending];
        readonly Layer[] pendingLayer = new Layer[MaxPending];
        int pendingCount;

        /// <summary>Phat tai <paramref name="point"/>, theo huong chem <paramref name="direction"/>.</summary>
        public void Play(Vector2 point, Vector2 direction)
        {
            transform.position = point;
            if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Shape Cone phat theo truc +Z cua transform -> xoay +Z ve huong chem.
            if (shards)
            {
                shards.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, 0f), Vector3.back);
                shards.Emit(Random.Range(shardCount.x, shardCount.y + 1));
            }
            if (flash) flash.Emit(1);

            // Nhip: vet ngan ngay lap tuc -> vet dai tre longDelay -> dom cam tre puffDelay.
            EmitShortStreaks(point, angle);
            if (longStreaks) Schedule(Layer.LongStreaks, longDelay, point, angle);
            if (puffs) Schedule(Layer.Puffs, puffDelay, point, angle);
        }

        void Schedule(Layer layer, float delay, Vector2 point, float angle)
        {
            if (delay <= 0f || pendingCount >= MaxPending)
            {
                EmitLayer(layer, point, angle);
                return;
            }
            pendingTime[pendingCount] = Time.time + delay;
            pendingPoint[pendingCount] = point;
            pendingAngle[pendingCount] = angle;
            pendingLayer[pendingCount] = layer;
            pendingCount++;
        }

        void Update()
        {
            // Phat cac lop da den gio; xoa khoi hang doi bang cach doi phan tu cuoi len.
            for (int i = pendingCount - 1; i >= 0; i--)
            {
                if (Time.time < pendingTime[i]) continue;
                EmitLayer(pendingLayer[i], pendingPoint[i], pendingAngle[i]);
                pendingCount--;
                pendingTime[i] = pendingTime[pendingCount];
                pendingPoint[i] = pendingPoint[pendingCount];
                pendingAngle[i] = pendingAngle[pendingCount];
                pendingLayer[i] = pendingLayer[pendingCount];
            }
        }

        void EmitLayer(Layer layer, Vector2 point, float angle)
        {
            if (layer == Layer.LongStreaks) EmitLongStreaks(point, angle);
            else EmitPuffs(point, angle);
        }

        // Cone goc rong ban hat vao truc Z (khong thay trong 2D) nen dom to dung Circle nam phang
        // tren mat phang XY: cung bat dau tu +X cuc bo -> xoay de giua cung trung huong chem.
        void EmitPuffs(Vector2 point, float angle)
        {
            // Mau gan: dom to, nhat, dong gan cho trung.
            EmitArc(puffs, point, angle, Random.Range(puffCount.x, puffCount.y + 1));

            // Mau xa: giot nho, dam, van xa. Chem ngang thi nghieng quat len tren (ve phia +Y)
            // de giot bay len roi roi xuong; chem len / xuong thi giu nguyen huong.
            if (bloodFar)
            {
                float dirX = Mathf.Cos(angle * Mathf.Deg2Rad);
                float tilt = Mathf.Abs(dirX) > 0.3f ? Mathf.Sign(dirX) * bloodFarUpTilt : 0f;
                EmitArc(bloodFar, point, angle + tilt, Random.Range(bloodFarCount.x, bloodFarCount.y + 1));
            }
        }

        /// <summary>Phat quat: cung cua Shape Circle bat dau tu +X cuc bo -> xoay de giua cung trung goc.</summary>
        static void EmitArc(ParticleSystem ps, Vector2 point, float centreAngle, int count)
        {
            ps.transform.position = point;
            ps.transform.rotation = Quaternion.Euler(0f, 0f, centreAngle - ps.shape.arc * 0.5f);
            ps.Emit(count);
        }

        void EmitShortStreaks(Vector2 point, float angle)
        {
            if (!shortStreaks) return;
            int n = Random.Range(shortCount.x, shortCount.y + 1);
            for (int i = 0; i < n; i++)
            {
                float a = i < n * shortAlongDirection ? angle + Random.Range(-shortSpread, shortSpread) : Random.Range(0f, 360f);
                float length = Random.Range(shortLength.x, shortLength.y);
                // Lui ra nua chieu dai + mot khoang ngau nhien: vet toa ra tu tam va rai thanh vong.
                float offset = length * 0.5f + Random.Range(shortOffset.x, shortOffset.y);
                EmitStreak(shortStreaks, point, a, length, Random.Range(shortWidth.x, shortWidth.y),
                    Random.Range(shortLifetime.x, shortLifetime.y), offset, shortColor);
            }
        }

        void EmitLongStreaks(Vector2 point, float angle)
        {
            int n = Random.Range(longCount.x, longCount.y + 1);
            for (int i = 0; i < n; i++)
            {
                // Vet dai gan song song, xuyen qua diem trung; lech ngang de cac duong tach nhau.
                var side = new Vector2(-Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
                EmitStreak(longStreaks, point + side * Random.Range(-longSideOffset, longSideOffset),
                    angle + Random.Range(-longSpread, longSpread),
                    Random.Range(longLength.x, longLength.y), Random.Range(longWidth.x, longWidth.y),
                    Random.Range(longLifetime.x, longLifetime.y), Random.Range(-0.5f, 0.8f), Color.white);
            }
        }

        /// <summary>Mot vet: dat vi tri, dai x day (Start Size 3D) va goc rieng; dung yen tai cho.</summary>
        static void EmitStreak(ParticleSystem ps, Vector2 centre, float angleDeg, float length, float width, float lifetime, float offset, Color color)
        {
            var dir = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
            var p = new ParticleSystem.EmitParams
            {
                position = centre + dir * offset,
                startSize3D = new Vector3(length, width, 1f),
                rotation = -angleDeg,              // goc xoay cua particle tinh theo chieu kim dong ho
                startLifetime = lifetime,
                startColor = color,
                velocity = Vector3.zero,
                applyShapeToPosition = false
            };
            ps.Emit(p, 1);
        }
    }
}
