using System.Collections;
using LuHanh.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Buc tuong da chan duong, sap khi thiet bi lien ket bi pha (<see cref="BreakableDevice"/>).
    /// <see cref="Trigger"/>: game dung (HitStop.Hold), man hinh toi -> camera chuyen sang tuong -> sang lai,
    /// tuong rung + nut, lun xuong san kem bui / da vun / rung camera -> toi -> camera ve nguoi choi.
    /// Hinh dat order thap hon tilemap dat nen phan lun xuong bi san che. Chay bang thoi gian thuc.
    /// Chua luu vao file save: tai lai man thi tuong dung lai.
    /// </summary>
    public class CollapsingWall : MonoBehaviour
    {
        [Tooltip("Goc chua hinh tuong - phan nay rung va lun xuong.")]
        [SerializeField] Transform visualRoot;
        [Tooltip("Hinh chinh doi theo frame nut (de trong = bo qua).")]
        [SerializeField] SpriteRenderer crackRenderer;
        [SerializeField] Sprite[] crackFrames;
        [Tooltip("Collider chan nguoi choi (layer Ground).")]
        [SerializeField] Collider2D blocker;
        [Tooltip("Diem camera nhin vao (de trong = giua tuong).")]
        [SerializeField] Transform focusPoint;

        [Header("Sap")]
        [SerializeField] float holdBefore = 0.35f;
        [Tooltip("Rung + nut truoc khi lun (giay).")]
        [SerializeField] float crackTime = 0.5f;
        [SerializeField] float jitter = 0.06f;
        [SerializeField] float sinkDistance = 11f;
        [SerializeField] float sinkTime = 1.1f;
        [SerializeField] float holdAfter = 0.6f;
        [SerializeField] float cameraShake = 0.3f;

        [Header("Hieu ung (ParticleSystem nen bat Use Unscaled Time)")]
        [Tooltip("Bui bot len o chan tuong trong luc lun.")]
        [SerializeField] ParticleSystem dust;
        [Tooltip("Da vun bung ra luc bat dau lun.")]
        [SerializeField] ParticleSystem debris;
        [SerializeField] int debrisCount = 24;

        bool running;
        public bool IsDown { get; private set; }

        public void Trigger()
        {
            if (IsDown || running) return;
            StartCoroutine(Cutscene());
        }

        IEnumerator Cutscene()
        {
            running = true;
            HitStop.Hold();
            var fx = FindAnyObjectByType<RoomTransitionFx>();
            var clamp = FindAnyObjectByType<RoomCameraClamp>();
            var rooms = FindAnyObjectByType<RoomManager>();
            var vcam = FindAnyObjectByType<CinemachineCamera>();
            var focus = focusPoint ? focusPoint : transform;
            var wallRoom = Room.At(focus.position);
            bool moveCamera = vcam && clamp && wallRoom;
            var oldFollow = vcam ? vcam.Follow : null;
            try
            {
                if (moveCamera)
                {
                    if (fx && fx.HasFade) yield return fx.FadeOut();
                    vcam.Follow = focus;
                    clamp.SnapTo(wallRoom.Bounds);
                    clamp.SkipDamping();
                    yield return null;
                    if (fx && fx.HasFade) yield return fx.FadeIn();
                }

                yield return new WaitForSecondsRealtime(holdBefore);
                yield return Collapse();
                yield return new WaitForSecondsRealtime(holdAfter);

                if (moveCamera)
                {
                    if (fx && fx.HasFade) yield return fx.FadeOut();
                    vcam.Follow = oldFollow;
                    if (rooms && rooms.Current) clamp.SnapTo(rooms.Current.Bounds);
                    clamp.SkipDamping();
                    yield return null;
                    if (fx && fx.HasFade) yield return fx.FadeIn();
                }
            }
            finally
            {
                if (vcam && vcam.Follow == focus) vcam.Follow = oldFollow;
                if (fx) fx.ResetFade();
                HitStop.Unhold();
                running = false;
            }
        }

        IEnumerator Collapse()
        {
            var root = visualRoot ? visualRoot : transform;
            Vector3 basePos = root.localPosition;

            // Rung + nut dan.
            for (float t = 0f; t < crackTime; t += Time.unscaledDeltaTime)
            {
                if (crackRenderer && crackFrames != null && crackFrames.Length > 0)
                    crackRenderer.sprite = crackFrames[Mathf.Min(crackFrames.Length - 1, (int)(t / crackTime * crackFrames.Length))];
                root.localPosition = basePos + (Vector3)(Random.insideUnitCircle * jitter * (t / crackTime));
                yield return null;
            }

            // Lun xuong: nhanh dan (ease-in), van rung nhe.
            if (blocker) blocker.enabled = false;
            if (debris) debris.Emit(debrisCount);
            if (dust) dust.Play();
            if (cameraShake > 0f) CameraShake.Kick(cameraShake, sinkTime);
            for (float t = 0f; t < sinkTime; t += Time.unscaledDeltaTime)
            {
                float k = t / sinkTime;
                root.localPosition = basePos + Vector3.down * (sinkDistance * k * k) + (Vector3)(Random.insideUnitCircle * jitter * 0.5f);
                yield return null;
            }
            root.gameObject.SetActive(false);
            if (dust) dust.Stop();
            IsDown = true;
        }
    }
}
