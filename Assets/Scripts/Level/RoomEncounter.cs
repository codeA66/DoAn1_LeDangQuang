using System.Collections;
using LuHanh.Core;
using LuHanh.Enemies;
using UnityEngine;
using UnityEngine.Events;

namespace LuHanh.Level
{
    /// <summary>
    /// Phong chien dau kieu Soul Knight: nguoi choi buoc vao vung trigger thi cua sap,
    /// quai xuat hien theo tung dot; diet het dot cuoi thi cua mo va goi <see cref="onCleared"/>
    /// (vd mo LevelPortal).
    ///
    /// Quai cua cac dot dat san trong scene va TAT san (khong Instantiate). Quai chet tu SetActive(false)
    /// nen chi can dem quai con bat. Nguoi choi chet: moi thu ve ban dau, phong danh lai tu dau.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RoomEncounter : MonoBehaviour, IResettable
    {
        [System.Serializable]
        public class Wave
        {
            [Tooltip("Quai cua dot nay, de TAT san trong scene.")]
            public EnemyBase[] enemies;
        }

        [SerializeField] Gate[] gates;
        [SerializeField] Wave[] waves;
        [Tooltip("Giay tu luc cua sap den dot dau, va giua cac dot.")]
        [SerializeField] float waveDelay = 0.6f;
        [Tooltip("Hieu ung xuat hien tai cho moi con quai (dung chung, Emit). De trong thi bo qua.")]
        [SerializeField] ParticleSystem spawnFx;
        [SerializeField] int spawnFxCount = 14;
        [Tooltip("Khung hinh dung lai mot thoang khi cua sap (giay thuc). 0 = tat.")]
        [SerializeField] float lockHitStop = 0.06f;
        [SerializeField] UnityEvent onCleared;

        enum Phase { Idle, Fighting, Cleared }
        Phase phase;

        /// <summary>Da diet het cac dot (cua da mo). Nguoi choi chet thi ve false.</summary>
        public bool IsCleared => phase == Phase.Cleared;
        int wave;
        bool pendingReset;

        void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            RespawnManager.Register(this);
            HideAll();
        }

        void OnDestroy() => RespawnManager.Unregister(this);

        void OnTriggerEnter2D(Collider2D other)
        {
            if (phase != Phase.Idle || other.isTrigger || !other.CompareTag("Player")) return;
            if (RespawnManager.Instance && RespawnManager.Instance.IsRespawning) return;
            StartCoroutine(Fight());
        }

        IEnumerator Fight()
        {
            phase = Phase.Fighting;
            foreach (var g in gates) if (g) g.Close();
            if (lockHitStop > 0f) HitStop.Request(lockHitStop);

            for (wave = 0; wave < waves.Length; wave++)
            {
                yield return new WaitForSeconds(waveDelay);
                if (phase != Phase.Fighting) yield break;
                Spawn(waves[wave]);
                while (phase == Phase.Fighting && Alive(waves[wave])) yield return null;
                if (phase != Phase.Fighting) yield break;
            }

            phase = Phase.Cleared;
            foreach (var g in gates) if (g) g.Open();
            onCleared?.Invoke();
        }

        void Spawn(Wave w)
        {
            foreach (var e in w.enemies)
            {
                if (!e) continue;
                e.gameObject.SetActive(true);
                if (spawnFx)
                {
                    spawnFx.transform.position = e.transform.position;
                    spawnFx.Emit(spawnFxCount);
                }
            }
        }

        static bool Alive(Wave w)
        {
            foreach (var e in w.enemies)
                if (e && e.gameObject.activeSelf) return true;
            return false;
        }

        void HideAll()
        {
            foreach (var w in waves)
                foreach (var e in w.enemies)
                    if (e) e.gameObject.SetActive(false);
        }

        /// <summary>Nguoi choi hoi sinh: phong ve trang thai chua danh.</summary>
        public void ResetState()
        {
            StopAllCoroutines();
            phase = Phase.Idle;
            foreach (var g in gates) if (g) g.OpenInstant();
            // Quai cung tu ResetState (bat lai) trong cung luot, thu tu khong co dinh:
            // de sang frame sau moi tat quai cua phong.
            pendingReset = true;
        }

        void LateUpdate()
        {
            if (!pendingReset) return;
            pendingReset = false;
            HideAll();
        }

        void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            if (!box) return;
            Gizmos.color = new Color(0.92f, 0.4f, 0.28f, 0.25f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.offset, box.size);
        }
    }
}
