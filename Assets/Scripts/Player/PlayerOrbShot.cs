using LuHanh.Combat;
using LuHanh.Core;
using LuHanh.Effects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LuHanh.Player
{
    /// <summary>
    /// Ky nang ban cau (phim K): chi ban ngang theo huong dang nhin (dang bam tuong thi ban ra phia ngoai tuong).
    /// Ton nang luong <see cref="PlayerEnergy"/>; phat dong tac Cast / CastAir roi tha cau sau mot nhip ngan.
    /// Dang Shadow thi cau to hon va sat thuong nhan theo Shadow. Can mo khoa bang vat pham.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerOrbShot : MonoBehaviour
    {
        [SerializeField] OrbProjectile orbPrefab;
        [Tooltip("Chum tia + song dung luc ban, ve tay theo frame (Prefabs/Fx/OrbCastFx).")]
        [SerializeField] CastFx castFxPrefab;
        [SerializeField] float castShake = 0.12f;
        [SerializeField] float castHitStop = 0.03f;
        [SerializeField] string actionName = "Shoot";
        [Tooltip("Tu luc bam toi luc cau bay ra (khop frame day tay cua Cast).")]
        [SerializeField] float releaseDelay = 0.15f;
        [SerializeField] float cooldown = 0.4f;
        [Tooltip("Ban tren khong: nhan vat dung yen giua khong trung bao lau (giay). 0 = roi binh thuong.")]
        [SerializeField] float airHoldTime = 0.25f;
        [Tooltip("Diem tha cau, tinh tu chan theo huong ban.")]
        [SerializeField] Vector2 muzzleOffset = new Vector2(0.9f, 1.0f);

        PlayerController controller;
        PlayerEnergy energy;
        PlayerShadow shadow;
        PlayerActionAnimation actionAnimation;
        PlayerInput playerInput;
        InputAction action;
        float cooldownTimer, releaseTimer = -1f, pendingDir;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            energy = GetComponent<PlayerEnergy>();
            shadow = GetComponent<PlayerShadow>();
            actionAnimation = GetComponent<PlayerActionAnimation>();
            playerInput = GetComponent<PlayerInput>();
        }

        void Start()
        {
            // Lay action o Start: luc Awake PlayerInput co the chua khoi tao ban actions dang dung.
            if (playerInput) action = playerInput.actions.FindAction(actionName);
            if (action == null) Debug.LogWarning($"PlayerOrbShot: khong tim thay action '{actionName}'.", this);
        }

        void Update()
        {
            if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

            if (releaseTimer >= 0f)
            {
                releaseTimer -= Time.deltaTime;
                if (releaseTimer < 0f) Release();
                return;
            }

            if (action == null || !action.WasPressedThisFrame()) return;
            if (!PlayerSkills.Has(Skill.OrbShot) || !controller.enabled || controller.IsHurt || controller.IsDashing) return;
            if (cooldownTimer > 0f || !orbPrefab || !energy || !energy.TrySpendShot()) return;

            pendingDir = controller.IsWallSliding ? -controller.WallDir : (controller.FacingRight ? 1f : -1f);
            if (controller.IsWallSliding) controller.Face(pendingDir);
            if (actionAnimation) actionAnimation.PlayCast(!controller.IsGrounded);
            // Tren khong: khung lo lung mot nhip de ngam ban (khong bi trong luc keo xuong truoc khi cau bay ra).
            if (!controller.IsGrounded && !controller.IsWallSliding && airHoldTime > 0f) controller.HoldInAir(Mathf.Max(airHoldTime, releaseDelay + 0.05f));
            cooldownTimer = cooldown;
            releaseTimer = releaseDelay;
        }

        void Release()
        {
            if (!controller.enabled || controller.IsHurt) return;
            Vector2 pos = (Vector2)transform.position + new Vector2(muzzleOffset.x * pendingDir, muzzleOffset.y);
            var orb = Instantiate(orbPrefab, pos, Quaternion.identity);
            bool shadowForm = shadow && shadow.IsActive;
            orb.Launch(pendingDir, shadowForm, shadow ? shadow.DamageMultiplier : 1, gameObject);
            if (castFxPrefab)
            {
                var fx = Instantiate(castFxPrefab, pos, Quaternion.identity);
                fx.Play(pendingDir, shadowForm);
            }
            if (castShake > 0f) CameraShake.Kick(castShake, 0.15f);
            if (castHitStop > 0f) HitStop.Request(castHitStop);
        }
    }
}
