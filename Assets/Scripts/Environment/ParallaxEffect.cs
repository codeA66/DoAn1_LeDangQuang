using UnityEngine;

namespace LuHanh.Environment
{
    /// <summary>
    /// Parallax cho mot tang background. Tang cang xa camera (z cang lon) thi troi cang cham.
    /// Tu dong nhan ban sprite sang trai/phai de cuon vo han theo chieu ngang.
    /// </summary>
    // CinemachineBrain khong dat DefaultExecutionOrder nen chay o order 0. Dat order duong
    // de LateUpdate o day chay SAU Brain: neu chay truoc, parallax doc vi tri camera cua
    // frame truoc va ca background tre mot frame -> nhin nhu bi giat.
    [DefaultExecutionOrder(600)]
    [ExecuteAlways]
    public class ParallaxEffect : MonoBehaviour
    {
        public Camera cam;
        public Transform followTarget;

        [Tooltip("Bo qua tinh toan theo z, dung truc tiep gia tri ben duoi (0 = dung yen theo camera, 1 = bam chat camera).")]
        public bool overrideFactor = false;
        [Range(0f, 1f)] public float manualFactor = 0.5f;

        [Tooltip("Nhan ban sprite sang hai ben de nen khong bao gio het.")]
        public bool infiniteHorizontal = true;

        [Tooltip("Cho tang troi theo ca chieu doc.")]
        public bool followVertical = false;
        [Range(0f, 1f)] public float verticalDamping = 0.3f;

        Vector3 startingPosition;
        Vector3 startingCamPosition;
        float tileWidth;
        bool initialised;

        float ZDistanceFromTarget => transform.position.z - (followTarget ? followTarget.position.z : 0f);

        float ClippingPlane => cam.transform.position.z + (ZDistanceFromTarget > 0f ? cam.farClipPlane : cam.nearClipPlane);

        /// <summary>0 = dung im so voi the gioi, 1 = dinh chat vao camera.</summary>
        public float ParallaxFactor
        {
            get
            {
                if (overrideFactor || cam == null) return manualFactor;
                return Mathf.Clamp01(Mathf.Abs(ZDistanceFromTarget) / Mathf.Abs(ClippingPlane));
            }
        }

        void OnEnable()
        {
            if (cam == null) cam = Camera.main;
            Initialise();
        }

        void Initialise()
        {
            if (cam == null) return;

            startingPosition = transform.position;
            startingCamPosition = cam.transform.position;

            if (infiniteHorizontal && Application.isPlaying)
                BuildTiles();

            initialised = true;
        }

        /// <summary>Nhan ban sprite goc sang trai va phai mot khoang bang be rong cua no.</summary>
        void BuildTiles()
        {
            var source = GetComponentInChildren<SpriteRenderer>();
            if (source == null || source.sprite == null) return;

            tileWidth = source.bounds.size.x;
            if (tileWidth <= 0.001f) return;

            // Can du tile de phu het be ngang camera cong hai ben du phong.
            float viewWidth = cam.orthographicSize * 2f * cam.aspect;
            int sideCount = Mathf.Max(1, Mathf.CeilToInt(viewWidth / tileWidth));

            for (int i = 1; i <= sideCount; i++)
            {
                CloneAt(source, -tileWidth * i);
                CloneAt(source, tileWidth * i);
            }
        }

        void CloneAt(SpriteRenderer source, float offsetX)
        {
            var clone = Instantiate(source.gameObject, source.transform.parent);
            clone.name = source.name + "_tile" + offsetX.ToString("F0");
            clone.transform.localPosition = source.transform.localPosition + Vector3.right * offsetX;
            // Ban sao giu nguyen SubtleSway: moi tile tu boc pha rieng nen lay khong dong loat.
        }

        void LateUpdate()
        {
            if (cam == null) return;
            if (!initialised) Initialise();

            Vector3 camDelta = cam.transform.position - startingCamPosition;
            float factor = ParallaxFactor;

            float x = startingPosition.x + camDelta.x * factor;
            float y = followVertical
                ? startingPosition.y + camDelta.y * factor * verticalDamping
                : startingPosition.y;

            // Keo tang ve gan camera khi da troi qua tron mot tile -> cuon vo han.
            if (infiniteHorizontal && tileWidth > 0.001f)
            {
                float drift = cam.transform.position.x - x;
                x += Mathf.Round(drift / tileWidth) * tileWidth;
            }

            transform.position = new Vector3(x, y, startingPosition.z);
        }
    }
}
