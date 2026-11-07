using UnityEngine;
using UnityEngine.UI;

// 상단 UI에 현재 날짜를 숫자 이미지로 표시하는 스크립트 (0~99일)
public class TopUIController : MonoBehaviour
{
    [Header("숫자 이미지 연결")]
    public Image tensDigitImage;   // 십의 자리 Image
    public Image onesDigitImage;   // 일의 자리 Image

    [Header("숫자 스프라이트 (인덱스 0~9 = day0~day9)")]
    public Sprite[] digitSprites = new Sprite[10];

    private int lastDisplayedDay = -1; // 값이 바뀔 때만 갱신하기 위한 캐시

    private void Update()
    {
        if (DrivingManager.Instance == null) return;

        int day = Mathf.Clamp(DrivingManager.Instance.currentDay, 0, 99);
        if (day == lastDisplayedDay) return; // 변화 없으면 스킵

        lastDisplayedDay = day;
        UpdateDayImages(day);
    }

    private void UpdateDayImages(int day)
    {
        int tens = day / 10;  // 십의 자리
        int ones = day % 10;  // 일의 자리

        // 일의 자리는 항상 표시
        if (onesDigitImage != null)
        {
            onesDigitImage.sprite = digitSprites[ones];
        }

        // 십의 자리는 10일 이상일 때만 표시 (1일 -> "1", 12일 -> "12")
        if (tensDigitImage != null)
        {
            tensDigitImage.gameObject.SetActive(tens > 0);
            if (tens > 0) tensDigitImage.sprite = digitSprites[tens];
        }
    }
}