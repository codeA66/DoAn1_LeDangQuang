using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thanh Mau va thanh Nhiem goc tren trai theo GDD muc 11.
/// Tu dung UI luc chay nen khong can prefab; thay bang UI that khi co art.
/// </summary>
[DisallowMultipleComponent]
public class CorruptionHUD : MonoBehaviour
{
    [Header("Mau")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("Bo cuc")]
    public Vector2 origin = new Vector2(24f, -24f);
    public Vector2 barSize = new Vector2(260f, 18f);
    public float gap = 6f;

    [Header("Mau sac")]
    public Color healthColor = new Color(0.85f, 0.18f, 0.24f);
    public Color corruptionColor = new Color(0.46f, 0.15f, 0.62f);
    public Color corruptionHotColor = new Color(0.78f, 0.30f, 0.95f);
    public Color backdropColor = new Color(0.05f, 0.03f, 0.09f, 0.78f);

    Image healthFill;
    Image corruptionFill;
    RectTransform corruptionRoot;

    void Start()
    {
        Build();
    }

    void Build()
    {
        var canvasGO = new GameObject("HUD_Canvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        healthFill = MakeBar(canvasGO.transform, "HealthBar", origin, healthColor, out _);
        var corruptionPos = origin + new Vector2(0f, -(barSize.y + gap));
        corruptionFill = MakeBar(canvasGO.transform, "CorruptionBar", corruptionPos,
                                 corruptionColor, out corruptionRoot);
    }

    Image MakeBar(Transform parent, string name, Vector2 pos, Color fillColor, out RectTransform root)
    {
        var backdrop = new GameObject(name, typeof(Image));
        backdrop.transform.SetParent(parent, false);

        root = backdrop.GetComponent<RectTransform>();
        // Neo goc tren trai de bo cuc khong doi khi cua so doi ti le.
        root.anchorMin = root.anchorMax = new Vector2(0f, 1f);
        root.pivot = new Vector2(0f, 1f);
        root.anchoredPosition = pos;
        root.sizeDelta = barSize;
        backdrop.GetComponent<Image>().color = backdropColor;

        var fillGO = new GameObject("Fill", typeof(Image));
        fillGO.transform.SetParent(backdrop.transform, false);

        var fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        var img = fillGO.GetComponent<Image>();
        img.color = fillColor;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;
        img.fillAmount = 1f;
        return img;
    }

    void Update()
    {
        if (healthFill != null)
            healthFill.fillAmount = maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f;

        var sys = CorruptionSystem.Instance;
        if (sys == null || corruptionFill == null) return;

        float n = sys.Normalised;
        corruptionFill.fillAmount = n;

        // Tu nguong Unstable tro len thanh Nhiem tu doi mau va dap nhip.
        float heat = Mathf.InverseLerp(0.5f, 1f, n);
        var c = Color.Lerp(corruptionColor, corruptionHotColor, heat);
        if (heat > 0f)
        {
            float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * Mathf.Lerp(4f, 14f, heat));
            c = Color.Lerp(corruptionColor, c, pulse);
        }
        corruptionFill.color = c;

        // Nguong HalfDemon: thanh rung nhe tai cho cho nguoi choi biet sap mat kiem soat.
        if (corruptionRoot != null)
        {
            var basePos = origin + new Vector2(0f, -(barSize.y + gap));
            float jitter = n >= 0.8f ? (Mathf.PerlinNoise(Time.time * 22f, 0.5f) - 0.5f) * 5f : 0f;
            corruptionRoot.anchoredPosition = basePos + new Vector2(jitter, 0f);
        }
    }
}
