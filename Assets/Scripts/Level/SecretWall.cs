using System.Collections;
using LuHanh.Combat;
using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Loi bi mat kieu Hollow Knight, trong giong dat that (tile cua vung + vet nut nhe).
    /// - <see cref="Mode.Wall"/>: tuong gia, chem (hoac ban cau) du <see cref="hits"/> don thi vo.
    /// - <see cref="Mode.Floor"/>: san gia, nguoi choi dung len thi rung <see cref="crumbleDelay"/> giay roi sup.
    /// Collider dac tren object goc (layer Ground: dung, va, bam tuong duoc). Tuong gia can them mot collider
    /// trigger o layer Enemy (con "Hitbox") de don chem tim thay qua IDamageable.
    /// Co <see cref="worldFlag"/>: luu vao save, vao lai phong thi loi da mo san.
    /// </summary>
    public class SecretWall : MonoBehaviour, IDamageable
    {
        public enum Mode { Wall, Floor }

        [SerializeField] Mode mode = Mode.Wall;
        [Tooltip("Co luu trong save (theo scene). De trong = moi lan vao phong lai dong.")]
        [SerializeField] string worldFlag;
        [Tooltip("Hinh (tilemap con, vet nut...) - an het khi vo.")]
        [SerializeField] GameObject visualRoot;
        [Tooltip("Tuong gia: so don de vo.")]
        [SerializeField] int hits = 3;
        [Tooltip("San gia: giay rung truoc khi sup.")]
        [SerializeField] float crumbleDelay = 0.35f;
        [SerializeField] float hitShake = 0.08f;
        [SerializeField] float breakShake = 0.25f;
        [SerializeField] float jitter = 0.05f;
        [Tooltip("Bui / da vun khi vo (Emit). De trong = khong co.")]
        [SerializeField] ParticleSystem debris;
        [SerializeField] int debrisCount = 24;

        int hp;
        bool broken, crumbling;
        float shakeTimer;
        Vector3 visualBase;

        void Awake()
        {
            hp = Mathf.Max(1, hits);
            if (visualRoot) visualBase = visualRoot.transform.localPosition;
        }

        void Start()
        {
            if (WorldState.Has(this, worldFlag)) Open(false);
        }

        public void TakeHit(HitInfo hit)
        {
            if (broken || mode != Mode.Wall) return;
            hp--;
            shakeTimer = 0.12f;
            Sfx.Play(SfxId.HitEnemy, 0.5f);
            if (debris) debris.Emit(Mathf.Max(1, debrisCount / 4));
            if (hp <= 0) Open(true);
            else if (hitShake > 0f) CameraShake.Kick(hitShake, 0.12f);
        }

        void OnCollisionEnter2D(Collision2D c)
        {
            if (broken || crumbling || mode != Mode.Floor || !c.collider.CompareTag("Player")) return;
            // Chi tinh khi dap tu tren xuong (chan nguoi choi o tren mat san).
            if (c.collider.bounds.min.y < GetComponent<Collider2D>().bounds.max.y - 0.2f) return;
            StartCoroutine(Crumble());
        }

        IEnumerator Crumble()
        {
            crumbling = true;
            shakeTimer = crumbleDelay;
            yield return new WaitForSeconds(crumbleDelay);
            Open(true);
        }

        void Open(bool withFx)
        {
            broken = true;
            shakeTimer = 0f;
            foreach (var col in GetComponentsInChildren<Collider2D>(true)) col.enabled = false;
            if (visualRoot) visualRoot.SetActive(false);
            if (!withFx) return;
            WorldState.Set(this, worldFlag);
            Sfx.Play(SfxId.WallCollapse, 0.8f);
            if (debris) debris.Emit(debrisCount);
            if (breakShake > 0f) CameraShake.Kick(breakShake, 0.25f);
        }

        void Update()
        {
            if (!visualRoot || shakeTimer <= 0f) return;
            shakeTimer -= Time.deltaTime;
            visualRoot.transform.localPosition = shakeTimer > 0f
                ? visualBase + (Vector3)(Random.insideUnitCircle * jitter)
                : visualBase;
        }

        void OnDrawGizmos()
        {
            var col = GetComponent<BoxCollider2D>();
            if (!col) return;
            Gizmos.color = new Color(0.5f, 0.47f, 0.87f, 0.9f);
            Gizmos.DrawWireCube(transform.TransformPoint(col.offset), col.size);
        }
    }
}
