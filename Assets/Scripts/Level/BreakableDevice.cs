using System.Collections;
using LuHanh.Combat;
using LuHanh.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Level
{
    /// <summary>
    /// Thiet bi pha duoc (chem hoac ban cau): chiu <see cref="hits"/> don, moi don chop sang + toe tia;
    /// don cuoi vo, tat den roi goi <see cref="CollapsingWall.Trigger"/> (camera chuyen sang tuong sap).
    /// Collider trigger o layer bi chem (Enemy) de PlayerAttack / OrbProjectile tim thay qua IDamageable.
    /// Co <see cref="worldFlag"/> thi luu vao file save (vao lai phong thiet bi da vo); de trong thi nhu cu:
    /// tai lai man thi thiet bi lanh lai.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BreakableDevice : MonoBehaviour, IDamageable
    {
        [Tooltip("Ten co luu trang thai trong phong (vd Device). De trong = khong luu.")]
        [SerializeField] string worldFlag;
        [SerializeField] CollapsingWall target;
        [SerializeField] int hits = 3;
        [Tooltip("Chi vo bang cau nang luong (phim K) - loi can ky nang Ban cau. Chem vao chi toe tia + keng, khong mat do ben.")]
        [SerializeField] bool orbOnly;

        [Header("Hinh")]
        [SerializeField] SpriteRenderer visual;
        [SerializeField] Sprite[] idleFrames;
        [Tooltip("Frame luc vo (phat 1 lan, giu frame cuoi).")]
        [SerializeField] Sprite[] breakFrames;
        [SerializeField] float fps = 8f;
        [SerializeField] Color hitColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] float hitFlashTime = 0.08f;

        [Header("Hieu ung")]
        [SerializeField] Light2D coreLight;
        [SerializeField] ParticleSystem hitSparks;
        [SerializeField] int sparkCount = 10;
        [SerializeField] ParticleSystem breakBurst;
        [SerializeField] int burstCount = 30;
        [SerializeField] float hitStop = 0.05f;
        [SerializeField] float hitShake = 0.12f;
        [SerializeField] float breakShake = 0.3f;
        [Tooltip("Giay thuc tu luc vo toi luc camera chuyen sang tuong.")]
        [SerializeField] float delayBeforeWall = 0.6f;

        int hp;
        bool broken;
        float flashTimer, frameTimer;
        int frame;
        Color baseColor = Color.white;

        void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            hp = Mathf.Max(1, hits);
            if (visual) baseColor = visual.color;
        }

        void Start()
        {
            if (!WorldState.Has(this, worldFlag)) return;
            // Da pha tu truoc (co the gioi): hien khung vo cuoi, khong pha lai, tuong lien ket tu doc co rieng.
            broken = true;
            GetComponent<Collider2D>().enabled = false;
            if (coreLight) coreLight.enabled = false;
            if (visual && breakFrames != null && breakFrames.Length > 0)
            {
                frame = breakFrames.Length - 1;
                visual.sprite = breakFrames[frame];
            }
        }

        public void TakeHit(HitInfo hit)
        {
            if (broken) return;
            if (orbOnly && !hit.fromOrb)
            {
                // Chem khong an: bao bang tia + tieng kim loai, khong rung, khong mat do ben.
                if (hitSparks) hitSparks.Emit(Mathf.Max(1, sparkCount / 3));
                Sfx.Play(SfxId.Lever, 0.6f);
                return;
            }
            hp--;
            flashTimer = hitFlashTime;
            if (visual) visual.color = hitColor;
            if (hitSparks) hitSparks.Emit(sparkCount);
            if (hitStop > 0f) HitStop.Request(hitStop);
            if (hp <= 0) Break();
            else if (hitShake > 0f) CameraShake.Kick(hitShake, 0.15f);
        }

        void Break()
        {
            broken = true;
            WorldState.Set(this, worldFlag);
            frame = 0;
            frameTimer = 0f;
            GetComponent<Collider2D>().enabled = false;
            if (breakBurst) breakBurst.Emit(burstCount);
            if (coreLight) coreLight.enabled = false;
            if (breakShake > 0f) CameraShake.Kick(breakShake, 0.3f);
            if (visual && breakFrames != null && breakFrames.Length > 0) visual.sprite = breakFrames[0];
            StartCoroutine(OpenWall());
        }

        IEnumerator OpenWall()
        {
            yield return new WaitForSecondsRealtime(delayBeforeWall);
            if (target) target.Trigger();
        }

        void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.unscaledDeltaTime;
                if (flashTimer <= 0f && visual) visual.color = baseColor;
            }
            if (!visual) return;
            var frames = broken ? breakFrames : idleFrames;
            if (frames == null || frames.Length == 0) return;
            if (broken && frame >= frames.Length - 1) return;
            frameTimer += Time.deltaTime;
            if (frameTimer < 1f / fps) return;
            frameTimer -= 1f / fps;
            frame = broken ? frame + 1 : (frame + 1) % frames.Length;
            visual.sprite = frames[frame];
        }
    }
}
