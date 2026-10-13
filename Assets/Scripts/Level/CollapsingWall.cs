using System.Collections;
using LuHanh.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Buc tuong da chan duong, sap khi thiet bi lien ket bi pha (<see cref="BreakableDevice"/>).
    /// <see cref="Trigger"/>: game dung (HitStop.Hold), camera lia tu nguoi choi sang tuong (xuyen phong),
    /// tuong rung + nut, lun xuong san kem bui / da vun / rung camera -> camera lia ve nguoi choi.
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

        [Header("Lia camera")]
        [Tooltip("Toc do lia (unit/giay thuc); thoi gian lia gioi han trong [Min, Max].")]
        [SerializeField] float panSpeed = 60f;
        [SerializeField] float minPanTime = 0.8f;
        [SerializeField] float maxPanTime = 1.8f;

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
            var clamp = FindAnyObjectByType<RoomCameraClamp>();
            var rooms = FindAnyObjectByType<RoomManager>();
            var vcam = FindAnyObjectByType<CinemachineCamera>();
            var focus = focusPoint ? focusPoint : transform;
            bool moveCamera = vcam && clamp && vcam.Follow;
            var oldFollow = vcam ? vcam.Follow : null;
            // Diem camera bam theo trong luc lia; bo kep phong de lia xuyen qua cac phong.
            var proxy = new GameObject("CameraPanTarget").transform;
            try
            {
                if (moveCamera)
                {
                    proxy.position = oldFollow.position;
                    vcam.Follow = proxy;
                    clamp.SnapTo(new Rect(-100000f, -100000f, 200000f, 200000f));
                    yield return Pan(proxy, oldFollow.position, focus.position);
                }

                yield return new WaitForSecondsRealtime(holdBefore);
                yield return Collapse();
                yield return new WaitForSecondsRealtime(holdAfter);

                if (moveCamera) yield return Pan(proxy, focus.position, oldFollow.position);
            }
            finally
            {
                if (vcam && moveCamera)
                {
                    vcam.Follow = oldFollow;
                    if (rooms && rooms.Current) clamp.SnapTo(rooms.Current.Bounds);
                    clamp.SkipDamping();
                }
                Destroy(proxy.gameObject);
                HitStop.Unhold();
                running = false;
            }
        }

        /// <summary>Lia diem bam cua camera tu a toi b (em dau em cuoi), camera dat thang vao diem moi frame.</summary>
        IEnumerator Pan(Transform proxy, Vector3 a, Vector3 b)
        {
            var clamp = FindAnyObjectByType<RoomCameraClamp>();
            float time = Mathf.Clamp(Vector2.Distance(a, b) / Mathf.Max(1f, panSpeed), minPanTime, maxPanTime);
            for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
            {
                proxy.position = Vector3.Lerp(a, b, Mathf.SmoothStep(0f, 1f, t / time));
                if (clamp) clamp.SkipDamping();
                yield return null;
            }
            proxy.position = b;
            if (clamp) clamp.SkipDamping();
            yield return null;
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
