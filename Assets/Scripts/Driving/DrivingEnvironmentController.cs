using UnityEngine;
using UnityEngine.UI;

public class DrivingEnvironmentController : MonoBehaviour
{
    public static DrivingEnvironmentController Instance { get; private set; }

    public bool IsEvening { get; private set; }

    [Header("공통 배경 스프라이트")]
    [SerializeField] private Sprite outsideBackgroundSprite;

    [Header("옆 창문 배경들")]
    [SerializeField] private Image[] sideBackgrounds;
    // LeftBg_A, LeftBg_B, RightBg_A, RightBg_B 넣기

    [Header("정면 창문 배경들")]
    [SerializeField] private Image frontFarBackground;
    [SerializeField] private Image[] frontMovingBackgrounds;
    // FrontMoving_A, FrontMoving_B 넣기

    [Header("창문별 밤 틴트")]
    [SerializeField] private CanvasGroup[] nightTints;
    // LeftWindow NightTint, FrontWindow NightTint, RightWindow NightTint

    [SerializeField] private float eveningTintAlpha = 0.4f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshEnvironment();
    }

    public void RefreshEnvironment()
    {
        IsEvening = DrivingManager.Instance != null &&
                    DrivingManager.Instance.farmedCountToday == 2;

        ApplyCommonSprite();
        ApplyNightTint();
    }

    private void ApplyCommonSprite()
    {
        if (outsideBackgroundSprite == null) return;

        foreach (var img in sideBackgrounds)
        {
            if (img != null)
                img.sprite = outsideBackgroundSprite;
        }

        if (frontFarBackground != null)
            frontFarBackground.sprite = outsideBackgroundSprite;

        foreach (var img in frontMovingBackgrounds)
        {
            if (img != null)
                img.sprite = outsideBackgroundSprite;
        }
    }

    private void ApplyNightTint()
    {
        float alpha = IsEvening ? eveningTintAlpha : 0f;

        foreach (var tint in nightTints)
        {
            if (tint != null)
                tint.alpha = alpha;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}