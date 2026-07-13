using UnityEngine;
using TMPro;

// 상단 UI에 현재 날짜를 표시하는 스크립트
public class TopUIController : MonoBehaviour
{
    [Header("날짜 텍스트 연결")]
    public TextMeshProUGUI dayText;

    private void Update()
    {
        // DrivingManager가 없는 상태에서는 실행하지 않음
        if (DrivingManager.Instance == null)
        {
            return;
        }

        // Day 1, Day 2, Day 3 형식으로 표시
        if (dayText != null)
        {
            dayText.text = "Day " + DrivingManager.Instance.currentDay;
        }
    }
}