using UnityEngine;
using UnityEngine.InputSystem;

namespace LuHanh.Level
{
    /// <summary>Man hinh sau man cuoi: bam phim bat ky (sau mot khoang ngan) de choi lai tu man dau.</summary>
    public class LevelCompleteScreen : MonoBehaviour
    {
        [SerializeField] LevelList levels;
        [Tooltip("Cho bao lau moi nhan phim, tranh bam nham ngay khi vua hien.")]
        [SerializeField] float inputDelay = 1.5f;

        float timer;

        void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer < inputDelay || SceneTransition.IsBusy) return;

            bool pressed = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
            if (!pressed || !levels) return;

            var first = levels.Get(0);
            if (first != null) SceneTransition.Load(first.sceneName, first.code, first.title, levels);
        }
    }
}
