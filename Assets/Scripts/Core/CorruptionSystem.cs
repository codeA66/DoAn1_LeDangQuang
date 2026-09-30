using System;
using UnityEngine;

/// <summary>Bon nguong Nhiem theo GDD muc 5.</summary>
public enum CorruptionTier
{
    /// <summary>0-49: hoat dong binh thuong.</summary>
    Calm,
    /// <summary>50-79: vien man hinh rung nhe, am thanh bat dau re.</summary>
    Unstable,
    /// <summary>80-99: mo khoa Ban Hoa Quy.</summary>
    HalfDemon,
    /// <summary>100: Hoa Quy hoan toan va chet.</summary>
    Lost
}

/// <summary>
/// Nguon su that duy nhat cho thanh Nhiem. Moi he thong khac (hinh anh, am thanh, HUD,
/// chi so chien dau) deu doc tu day chu khong tu giu so rieng.
/// </summary>
[DisallowMultipleComponent]
public class CorruptionSystem : MonoBehaviour
{
    public static CorruptionSystem Instance { get; private set; }

    [Header("Trang thai")]
    [SerializeField, Range(0f, 100f)] float corruption;

    [Header("Chi phi skill (GDD muc 4)")]
    public float spiritEyeCost = 5f;    // Linh Nhan
    public float doorwayCost = 12f;     // Canh Cua

    [Header("Dien Tho Thanh Tay")]
    [Tooltip("Nhiem giam bao nhieu diem moi giay khi dung yen tai Dien Tho.")]
    public float cleanseRate = 10f;

    [Header("Ban Hoa Quy")]
    [Tooltip("Mau toi da mat moi giay khi dang Ban Hoa Quy.")]
    public float halfDemonHealthDrainPercent = 2f;
    public float halfDemonDamageBonus = 0.5f;
    public float halfDemonSpeedBonus = 0.2f;

    [Header("Hoi sinh")]
    [Tooltip("Nhiem nen giu lai sau khi Hoa Quy va hoi sinh trong cung mot man.")]
    public float residualOnDeath = 20f;

    /// <summary>Nhiem tho, 0-100.</summary>
    public float Value => corruption;

    /// <summary>Nhiem chuan hoa 0-1, dung cho shader va thanh HUD.</summary>
    public float Normalised => corruption / 100f;

    public CorruptionTier Tier => TierOf(corruption);

    public bool CanEnterHalfDemon => corruption >= 80f && corruption < 100f;

    /// <summary>Dang bat Ban Hoa Quy hay khong.</summary>
    public bool HalfDemonActive { get; private set; }

    /// <summary>Ban ra khi gia tri doi. Tham so la Nhiem moi.</summary>
    public event Action<float> Changed;

    /// <summary>Ban ra khi vuot sang nguong khac. (cu, moi)</summary>
    public event Action<CorruptionTier, CorruptionTier> TierChanged;

    /// <summary>Ban ra khi Nhiem cham 100.</summary>
    public event Action FullyCorrupted;

    static CorruptionTier TierOf(float v)
    {
        if (v >= 100f) return CorruptionTier.Lost;
        if (v >= 80f) return CorruptionTier.HalfDemon;
        if (v >= 50f) return CorruptionTier.Unstable;
        return CorruptionTier.Calm;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (HalfDemonActive && corruption >= 100f)
            HalfDemonActive = false;
    }

    /// <summary>Cong Nhiem. Dung khi nguoi choi tung Linh Nhan / Canh Cua.</summary>
    public void Add(float amount)
    {
        if (amount == 0f) return;
        SetValue(corruption + amount);
    }

    /// <summary>Giam Nhiem dan theo thoi gian - goi moi frame khi dung tai Dien Tho.</summary>
    public void Cleanse(float deltaTime)
    {
        SetValue(corruption - cleanseRate * deltaTime);
    }

    /// <summary>Bat Ban Hoa Quy neu dang o nguong 80-99.</summary>
    public bool TryEnterHalfDemon()
    {
        if (!CanEnterHalfDemon || HalfDemonActive) return false;
        HalfDemonActive = true;
        return true;
    }

    public void ExitHalfDemon() => HalfDemonActive = false;

    /// <summary>Hoi sinh tai checkpoint: giu lai mot phan Nhiem nen (GDD muc 8).</summary>
    public void ResetToCheckpoint()
    {
        HalfDemonActive = false;
        SetValue(residualOnDeath);
    }

    void SetValue(float next)
    {
        next = Mathf.Clamp(next, 0f, 100f);
        if (Mathf.Approximately(next, corruption)) return;

        var before = Tier;
        corruption = next;
        var after = Tier;

        Changed?.Invoke(corruption);
        if (before != after) TierChanged?.Invoke(before, after);
        if (after == CorruptionTier.Lost) FullyCorrupted?.Invoke();
    }

#if UNITY_EDITOR
    // Keo thanh truot trong Inspector luc dang chay -> he thong khac cap nhat theo ngay.
    void OnValidate()
    {
        if (!Application.isPlaying) return;
        Changed?.Invoke(corruption);
    }
#endif
}
