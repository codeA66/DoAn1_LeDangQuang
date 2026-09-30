using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cong cu thu tay de xem hieu ung Nhiem ma chua can gameplay that.
/// K = Linh Nhan (+5), L = Canh Cua (+12), giu E = Dien Tho thanh tay, T = bat Ban Hoa Quy.
/// Go component nay ra khi skill that da noi vao <see cref="CorruptionSystem"/>.
/// </summary>
public class CorruptionDebugDriver : MonoBehaviour
{
    [Tooltip("Tu chay mot vong Nhiem len roi xuong de xem hieu ung ma khong can bam phim.")]
    public bool autoCycle;
    [Tooltip("Giay cho mot vong len-xuong tron ven.")]
    public float cycleSeconds = 24f;

    float t;

    void Update()
    {
        var sys = CorruptionSystem.Instance;
        if (sys == null) return;

        if (autoCycle)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, cycleSeconds);
            float wave = Mathf.PingPong(t * 2f, 1f);
            float target = wave * 100f;
            sys.Add(target - sys.Value);
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.kKey.wasPressedThisFrame) sys.Add(sys.spiritEyeCost);   // Linh Nhan
        if (kb.lKey.wasPressedThisFrame) sys.Add(sys.doorwayCost);     // Canh Cua
        if (kb.eKey.isPressed) sys.Cleanse(Time.deltaTime);            // Dien Tho
        if (kb.tKey.wasPressedThisFrame) sys.TryEnterHalfDemon();
        if (kb.rKey.wasPressedThisFrame) sys.ResetToCheckpoint();
    }
}
