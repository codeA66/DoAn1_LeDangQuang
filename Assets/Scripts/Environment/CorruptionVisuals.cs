using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Dich muc Nhiem thanh hinh anh: day vao shader toan man hinh, rung camera tu nguong
/// Unstable tro len, va keo anh sang toan cuc ve phia tim den.
/// </summary>
// CinemachineBrain chay o order 0. Dat 500 de rung camera duoc ap SAU Brain, neu khong
// Brain se ghi de va rung bi mat sach. ParallaxEffect dat 600 -> no doc duoc vi tri
// camera da rung, nen cac tang background rung theo dung ti le parallax.
[DefaultExecutionOrder(500)]
[DisallowMultipleComponent]
public class CorruptionVisuals : MonoBehaviour
{
    [Header("Shader toan man hinh")]
    [Tooltip("Material dung shader LuHanh/CorruptionFullScreen.")]
    public Material corruptionMaterial;

    [Tooltip("Do manh toi da cua hieu ung khi Nhiem = 100.")]
    [Range(0f, 1f)] public float maxIntensity = 1f;

    [Tooltip("Toc do bam theo gia tri that. Thap = hieu ung dang len muot hon.")]
    public float responseSpeed = 2.5f;

    [Header("Rung camera (tu nguong 50 tro len)")]
    public Transform cameraToShake;
    [Tooltip("Bien do rung toi da, tinh bang unit the gioi.")]
    public float maxShake = 0.06f;
    public float shakeFrequency = 18f;

    [Header("Anh sang toan cuc")]
    public Light2D globalLight;
    public Color cleanLightColor = new Color(0.72f, 0.66f, 0.95f);
    public Color corruptedLightColor = new Color(0.45f, 0.18f, 0.52f);
    public float cleanIntensity = 0.95f;
    public float corruptedIntensity = 0.55f;

    static readonly int CorruptionId = Shader.PropertyToID("_Corruption");

    float displayed;          // gia tri da lam muot, 0-1
    Vector3 lastShakeOffset;

    // Cinemachine ghi lai vi tri camera moi LateUpdate, nen offset khong tich luy va
    // ta chi can cong them. Khong co Brain thi phai tu tra lai offset cua frame truoc.
    bool cameraDrivenByCinemachine;

    void Start()
    {
        if (cameraToShake == null && Camera.main != null)
            cameraToShake = Camera.main.transform;

        if (cameraToShake != null)
        {
            var brain = cameraToShake.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            cameraDrivenByCinemachine = brain != null && brain.enabled;
        }

        if (corruptionMaterial != null)
            corruptionMaterial.SetFloat(CorruptionId, 0f);
    }

    void LateUpdate()
    {
        var sys = CorruptionSystem.Instance;
        float target = sys != null ? sys.Normalised : 0f;

        // Lam muot de nhay skill khong gay giat hinh mot frame.
        displayed = Mathf.Lerp(displayed, target, 1f - Mathf.Exp(-responseSpeed * Time.deltaTime));

        if (corruptionMaterial != null)
            corruptionMaterial.SetFloat(CorruptionId, displayed * maxIntensity);

        ApplyLight();
        ApplyShake();
    }

    void ApplyLight()
    {
        if (globalLight == null) return;
        globalLight.color = Color.Lerp(cleanLightColor, corruptedLightColor, displayed);
        globalLight.intensity = Mathf.Lerp(cleanIntensity, corruptedIntensity, displayed);
    }

    void ApplyShake()
    {
        if (cameraToShake == null) return;

        // Co Brain: no vua ghi lai vi tri sach o LateUpdate truoc do, khong con offset cu
        // de tru. Khong co Brain: phai tu tra lai offset frame truoc keo camera troi dan.
        if (!cameraDrivenByCinemachine)
            cameraToShake.position -= lastShakeOffset;
        lastShakeOffset = Vector3.zero;

        // Chi rung tu nguong Unstable (50) tro len, manh dan toi 100.
        float shakeAmount = Mathf.InverseLerp(0.5f, 1f, displayed);
        if (shakeAmount <= 0f) return;

        float t = Time.time * shakeFrequency;
        // Hai chuoi noise le nhau cho rung khong deu nhu song sin don.
        float x = (Mathf.PerlinNoise(t, 0.37f) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(0.91f, t) - 0.5f) * 2f;

        lastShakeOffset = new Vector3(x, y, 0f) * (maxShake * shakeAmount);
        cameraToShake.position += lastShakeOffset;
    }
}
