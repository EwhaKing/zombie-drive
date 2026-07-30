using UnityEngine;
using UnityEngine.UI;

// 현재 파밍 완료 횟수에 따라
// 주행 화면의 낮/저녁 아트를 변경하는 스크립트
public class DrivingEnvironmentController : MonoBehaviour
{
    public static DrivingEnvironmentController Instance { get; private set; }

    // 다른 스크립트에서 현재 저녁인지 확인할 때 사용
    public bool IsEvening { get; private set; }

    [Header("캠핑카 내부 인테리어")]
    [SerializeField] private Image camperInteriorImage;
    [SerializeField] private Sprite dayInteriorSprite;
    [SerializeField] private Sprite eveningInteriorSprite;

    [Header("왼쪽 창문 반복 배경")]
    [SerializeField] private Image leftBackgroundA;
    [SerializeField] private Image leftBackgroundB;
    [SerializeField] private Sprite leftDaySprite;
    [SerializeField] private Sprite leftEveningSprite;

    [Header("오른쪽 창문 반복 배경")]
    [SerializeField] private Image rightBackgroundA;
    [SerializeField] private Image rightBackgroundB;
    [SerializeField] private Sprite rightDaySprite;
    [SerializeField] private Sprite rightEveningSprite;

    [Header("정면 먼 배경")]
    [SerializeField] private Image frontFarBackground;
    [SerializeField] private Sprite frontFarDaySprite;
    [SerializeField] private Sprite frontFarEveningSprite;

    [Header("정면 가까워지는 배경")]
    [SerializeField] private Image frontMovingA;
    [SerializeField] private Image frontMovingB;
    [SerializeField] private Sprite frontMovingDaySprite;
    [SerializeField] private Sprite frontMovingEveningSprite;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // DrivingScene에 처음 들어왔을 때
        // 현재 파밍 횟수에 맞는 시간대를 적용
        RefreshEnvironment();
    }

    public void RefreshEnvironment()
    {
        // 파밍을 정확히 2회 완료한 상태에서만 저녁
        IsEvening =
            DrivingManager.Instance != null &&
            DrivingManager.Instance.farmedCountToday == 2;

        // 내부 인테리어 변경
        ApplySprite(
            camperInteriorImage,
            dayInteriorSprite,
            eveningInteriorSprite
        );

        // 왼쪽 창문 배경 변경
        ApplySprite(
            leftBackgroundA,
            leftDaySprite,
            leftEveningSprite
        );

        ApplySprite(
            leftBackgroundB,
            leftDaySprite,
            leftEveningSprite
        );

        // 오른쪽 창문 배경 변경
        ApplySprite(
            rightBackgroundA,
            rightDaySprite,
            rightEveningSprite
        );

        ApplySprite(
            rightBackgroundB,
            rightDaySprite,
            rightEveningSprite
        );

        // 정면 먼 배경 변경
        ApplySprite(
            frontFarBackground,
            frontFarDaySprite,
            frontFarEveningSprite
        );

        // 정면 가까워지는 배경 변경
        ApplySprite(
            frontMovingA,
            frontMovingDaySprite,
            frontMovingEveningSprite
        );

        ApplySprite(
            frontMovingB,
            frontMovingDaySprite,
            frontMovingEveningSprite
        );

        Debug.Log(
            IsEvening
                ? "주행 화면을 저녁으로 변경했습니다."
                : "주행 화면을 아침/낮으로 변경했습니다."
        );
    }

    private void ApplySprite(
        Image targetImage,
        Sprite daySprite,
        Sprite eveningSprite)
    {
        if (targetImage == null)
        {
            return;
        }

        Sprite targetSprite =
            IsEvening ? eveningSprite : daySprite;

        // Inspector 연결이 빠진 경우 기존 이미지는 유지
        if (targetSprite != null)
        {
            targetImage.sprite = targetSprite;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
