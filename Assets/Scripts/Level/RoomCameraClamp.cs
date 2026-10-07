using Unity.Cinemachine;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Extension Cinemachine giu camera trong phong hien tai (RoomManager bao phong nao).
    /// - Mep phong "mem": camera cham dan khi toi gan mep thay vi khung lai dot ngot (van khong vuot mep).
    /// - Phong nho hon khung hinh theo truc nao thi camera dung giua phong theo truc do.
    /// - Chuyen phong: camera truot sang vi tri hop le cua phong moi bang noi suy Hermite bat dau DUNG
    ///   van toc camera dang co (khong giat toc do o khung dau), giam dan va dung em, khong vuot dich.
    ///   Thoi gian truot tinh theo quang duong; dem bang thoi gian thuc nen chay ca khi game dang dung.
    /// Gan len CinemachineCamera (cung object voi Position Composer).
    /// </summary>
    [AddComponentMenu("LuHanh/Room Camera Clamp")]
    public class RoomCameraClamp : CinemachineExtension
    {
        [Tooltip("Do rong vung cham dan o mep phong (unit). 0 = khung cung.")]
        [SerializeField] float edgeSoftness = 1f;

        [Header("Chuyen phong")]
        [Tooltip("Toc do truot trung binh (unit/giay thuc). Thoi gian = quang duong / toc do, gioi han trong [Min, Max].")]
        [SerializeField] float transitionSpeed = 60f;
        [SerializeField] float minTransitionTime = 0.35f;
        [SerializeField] float maxTransitionTime = 0.6f;

        Rect room;
        bool hasRoom;

        bool transitioning;
        float transitionStart;
        float transitionDuration;
        Vector3 fromPosition;
        Vector3 startVelocity;

        Vector3 lastPosition;
        Vector3 lastDesired;
        Vector3 velocity;
        bool hasLastPosition;
        float lastHalfW, lastHalfH;

        public bool IsTransitioning => transitioning;

        /// <summary>Doi phong ngay, khong truot (luc vao man, hoi sinh khi man hinh dang toi).</summary>
        public void SnapTo(Rect bounds)
        {
            room = bounds;
            hasRoom = true;
            transitioning = false;
            velocity = Vector3.zero;
        }

        /// <summary>Truot tu vi tri camera hien tai sang phong moi. Tra ve thoi gian truot (giay thuc).</summary>
        public float TransitionTo(Rect bounds)
        {
            if (!hasLastPosition)
            {
                SnapTo(bounds);
                return 0f;
            }

            room = bounds;
            hasRoom = true;
            fromPosition = lastPosition;
            startVelocity = velocity;

            // Uoc luong diem den tu vi tri camera "muon" toi o frame truoc, de tinh thoi gian truot.
            Vector3 to = Constrain(lastDesired, lastHalfW, lastHalfH);
            float distance = Vector2.Distance(fromPosition, to);
            transitionDuration = Mathf.Clamp(distance / Mathf.Max(1f, transitionSpeed), minTransitionTime, maxTransitionTime);
            transitionStart = Time.unscaledTime;
            transitioning = true;
            return transitionDuration;
        }

        /// <summary>Bo damping o frame ke tiep de camera dat thang vao cho nhan vat (sau khi dich chuyen nhan vat).</summary>
        public void SkipDamping()
        {
            if (ComponentOwner) ComponentOwner.PreviousStateIsValid = false;
            velocity = Vector3.zero;
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Body || !hasRoom) return;

            lastHalfH = state.Lens.OrthographicSize;
            lastHalfW = lastHalfH * state.Lens.Aspect;
            Vector3 desired = state.GetCorrectedPosition();
            Vector3 target = Constrain(desired, lastHalfW, lastHalfH);

            if (transitioning)
            {
                float k = Mathf.Clamp01((Time.unscaledTime - transitionStart) / transitionDuration);
                target.x = Hermite(fromPosition.x, target.x, startVelocity.x * transitionDuration, k);
                target.y = Hermite(fromPosition.y, target.y, startVelocity.y * transitionDuration, k);
                if (k >= 1f) transitioning = false;
            }
            else if (deltaTime > 0f && hasLastPosition)
            {
                // Van toc camera (lam muot) - dung lam van toc bat dau khi chuyen phong.
                velocity = Vector3.Lerp(velocity, (target - lastPosition) / deltaTime, 0.5f);
            }

            state.PositionCorrection += target - desired;
            lastDesired = desired;
            lastPosition = target;
            hasLastPosition = true;
        }

        Vector3 Constrain(Vector3 p, float halfW, float halfH)
        {
            p.x = Limit(p.x, room.xMin + halfW, room.xMax - halfW);
            p.y = Limit(p.y, room.yMin + halfH, room.yMax - halfH);
            return p;
        }

        /// <summary>
        /// Gioi han x trong [lo, hi]. Trong vung <see cref="edgeSoftness"/> sat mep, x tien lai gan mep theo ham mu:
        /// khop muot voi doan giua (cung do doc 1) va khong bao gio vuot qua mep.
        /// </summary>
        float Limit(float x, float lo, float hi)
        {
            if (lo > hi) return (lo + hi) * 0.5f;
            float w = Mathf.Min(edgeSoftness, (hi - lo) * 0.5f);
            if (w <= 0.0001f) return Mathf.Clamp(x, lo, hi);
            if (x > hi - w) return hi - w + w * (1f - Mathf.Exp(-(x - (hi - w)) / w));
            if (x < lo + w) return lo + w - w * (1f - Mathf.Exp(-((lo + w) - x) / w));
            return x;
        }

        /// <summary>
        /// Duong Hermite tu p0 (van toc dau m0, da nhan thoi gian) toi p1 (van toc cuoi 0).
        /// Van toc dau nguoc huong thi bo; lon qua thi chan o 3 lan quang duong - nguong de duong cong
        /// di mot chieu, khong vuot dich roi giat nguoc.
        /// </summary>
        static float Hermite(float p0, float p1, float m0, float t)
        {
            float delta = p1 - p0;
            if (delta * m0 <= 0f) m0 = 0f;
            else m0 = Mathf.Clamp(m0, -3f * Mathf.Abs(delta), 3f * Mathf.Abs(delta));
            float t2 = t * t, t3 = t2 * t;
            return (2f * t3 - 3f * t2 + 1f) * p0 + (t3 - 2f * t2 + t) * m0 + (-2f * t3 + 3f * t2) * p1;
        }
    }
}
