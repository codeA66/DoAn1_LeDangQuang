using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Nhin len/xuong kieu Hollow Knight: dung yen tren dat, giu W hoac S qua <see cref="holdDelay"/> giay
    /// thi camera lia len/xuong <see cref="distance"/> unit. Nha phim, di chuyen hoac roi khoi dat thi camera truot ve.
    /// Do lech cong sau khi kep phong nen camera duoc vuot mep phong (nhin sang phong ke ben).
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerLook : MonoBehaviour
    {
        [Tooltip("Khoang lia toi da (unit, 1 o = 1 unit).")]
        [Min(0)] [SerializeField] float distance = 4f;
        [Tooltip("Phai giu W/S bao lau (giay) thi camera moi bat dau lia.")]
        [Min(0)] [SerializeField] float holdDelay = 0.6f;
        [Tooltip("Thoi gian camera truot toi / ve (giay). Nho = nhanh.")]
        [Min(0.01f)] [SerializeField] float smoothTime = 0.25f;

        PlayerController controller;
        float holdTimer;
        int holdDir;
        float offset, velocity;

        void Awake() => controller = GetComponent<PlayerController>();

        void Update()
        {
            Vector2 move = controller.RawMove;
            int dir = Mathf.Abs(move.x) < 0.5f && controller.IsGrounded ? (int)Mathf.Round(move.y) : 0;
            if (dir != holdDir) { holdDir = dir; holdTimer = 0f; }
            else if (dir != 0) holdTimer += Time.deltaTime;

            float target = dir != 0 && holdTimer >= holdDelay ? dir * distance : 0f;
            offset = Mathf.SmoothDamp(offset, target, ref velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            CameraLook.Offset = new Vector2(0f, offset);
#if UNITY_EDITOR
            // TAM - do loi W lia nguoc. Xoa sau khi kiem tra.
            if (dir != 0 && Time.frameCount % 20 == 0)
            {
                var cam = Camera.main;
                Debug.Log($"[LookDebug] moveY={move.y:F2} dir={dir} grounded={controller.IsGrounded} hold={holdTimer:F2} " +
                          $"target={target:F2} offset={offset:F2} playerY={transform.position.y:F2} camY={(cam ? cam.transform.position.y : 0f):F2}");
            }
#endif
        }

        void OnDisable()
        {
            offset = velocity = holdTimer = 0f;
            holdDir = 0;
            CameraLook.Offset = Vector2.zero;
        }
    }
}
