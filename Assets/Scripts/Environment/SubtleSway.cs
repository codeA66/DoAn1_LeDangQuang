using UnityEngine;

/// <summary>
/// Dao nhe theo song sin - dung cho cay, co, den treo o tang gan.
/// Moi doi tuong duoc lech pha ngau nhien de ca man hinh khong dao dong cung nhip.
/// </summary>
public class SubtleSway : MonoBehaviour
{
    [Header("Nghieng")]
    [Tooltip("Do nghieng toi da, tinh bang do.")]
    public float tiltAmplitude = 1.5f;
    public float tiltSpeed = 0.5f;

    [Header("Truot ngang")]
    public float swayAmplitude = 0.05f;
    public float swaySpeed = 0.35f;

    [Header("Nhap nho")]
    public float bobAmplitude = 0.03f;
    public float bobSpeed = 0.7f;

    [Tooltip("Lech pha ngau nhien luc bat dau.")]
    public bool randomisePhase = true;

    Vector3 basePosition;
    Quaternion baseRotation;
    float phase;

    void Start()
    {
        basePosition = transform.localPosition;
        baseRotation = transform.localRotation;
        phase = randomisePhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    void Update()
    {
        float t = Time.time;

        float tilt = Mathf.Sin(t * tiltSpeed + phase) * tiltAmplitude;
        // Tan so le nhau -> chuyen dong khong bao gio lap lai y het.
        float sway = Mathf.Sin(t * swaySpeed + phase * 1.3f) * swayAmplitude;
        float bob = Mathf.Sin(t * bobSpeed + phase * 0.7f) * bobAmplitude;

        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, tilt);
        transform.localPosition = basePosition + new Vector3(sway, bob, 0f);
    }
}
