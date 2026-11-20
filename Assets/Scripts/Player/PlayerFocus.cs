using LuHanh.Combat;
using LuHanh.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LuHanh.Player
{
    /// <summary>
    /// Focus kieu Hollow Knight: dung tren dat, giu phim (action "Shadow" = Y) trong <see cref="ShadowConfig.focusTime"/>
    /// giay -> tieu <see cref="ShadowConfig.focusCost"/> tu binh nang luong (<see cref="PlayerShadow"/>), hoi 1 mask.
    /// Giu tiep thi Focus tiep (con du nang luong va chua day mau). Bi danh, roi khoi mat dat, dash hoac buong phim
    /// thi huy, khong mat nang luong. Trong luc Focus dung yen. Cung binh voi ban cau K -> chon ban hay hoi mau.
    /// Chi chay khi bien hinh Shadow dang tat (phim Y danh cho Focus).
    /// </summary>
    [RequireComponent(typeof(PlayerController), typeof(PlayerShadow))]
    public class PlayerFocus : MonoBehaviour
    {
        [Tooltip("Ten action trong PlayerInput (giu phim de Focus).")]
        [SerializeField] string actionName = "Shadow";
        [Tooltip("Mau chop sang khi hoi xong 1 mask.")]
        [SerializeField] Color healFlash = new(0.72f, 1f, 0.94f);   // #B8FFF0

        PlayerController controller;
        PlayerShadow vessel;
        PlayerHealth playerHealth;
        Health health;
        InputAction action;
        float timer;
        bool channeling;

        /// <summary>Tien do mask dang Focus 0..1, -1 khi khong Focus.</summary>
        public float Progress => channeling ? Mathf.Clamp01(timer / vessel.Config.focusTime) : -1f;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            vessel = GetComponent<PlayerShadow>();
            playerHealth = GetComponent<PlayerHealth>();
            health = GetComponent<Health>();
            var input = GetComponent<PlayerInput>();
            if (input) action = input.actions.FindAction(actionName);
            if (action == null) Debug.LogWarning($"PlayerFocus: khong tim thay action '{actionName}'.", this);
        }

        // Bi tat giua chung (ngoi ghe, noi chuyen NPC, chet...): huy, tra lai toc do chay.
        void OnDisable() => Stop();

        void Update()
        {
            if (vessel.TransformEnabled) { Stop(); return; } // phim Y dang dung de bien hinh Shadow
            var cfg = vessel.Config;
            bool held = action != null && action.IsPressed();
            bool canFocus = held && controller.enabled && controller.IsGrounded && !controller.IsHurt &&
                            !controller.IsDashing && vessel.CanAfford(cfg.focusCost) && health && !health.IsDead &&
                            health.Current < health.Max;
            if (!canFocus) { Stop(); return; }

            if (!channeling)
            {
                channeling = true;
                timer = 0f;
                controller.SetRunMultiplier(0f); // dung yen khi Focus
            }

            timer += Time.deltaTime;
            vessel.ShowHoldProgress(timer / cfg.focusTime);
            if (timer < cfg.focusTime) return;

            if (vessel.TrySpend(cfg.focusCost))
            {
                health.Heal(1);
                Sfx.Play(SfxId.Heal);
                if (playerHealth) playerHealth.Flash(healFlash, 1, 0.12f, 0.05f);
            }
            timer = 0f; // giu tiep thi Focus mask tiep theo
            vessel.ShowHoldProgress(0f);
        }

        void Stop()
        {
            if (!channeling) return;
            channeling = false;
            timer = 0f;
            if (controller) controller.SetRunMultiplier(1f);
            if (vessel) vessel.ShowHoldProgress(0f);
        }
    }
}
