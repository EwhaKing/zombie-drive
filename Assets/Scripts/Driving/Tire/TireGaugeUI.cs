
using UnityEngine;
using UnityEngine.UI;

public class TireGaugeUI : MonoBehaviour
{
    [Header("타이어 게이지 Fill 이미지")]
    [SerializeField] private Image tireFill;

    private void Update()
    {
        // 상단바 전용 타이어 매니저 확인
        if (VehicleTireStatusManager.Instance == null)
            return;

        if (tireFill == null)
            return;

        // 현재 타이어 상태 가져오기
        int currentTire =
            VehicleTireStatusManager.Instance.currentTire;

        int maxTire =
            VehicleTireStatusManager.Instance.maxTire;

        // 최대값이 0 이하일 경우 오류 방지
        if (maxTire <= 0)
        {
            tireFill.fillAmount = 0f;
            return;
        }

        // 현재 타이어 상태를 0~1 비율로 변환
        float tireRatio = Mathf.Clamp01(
            (float)currentTire / maxTire
        );

        // 상단바 게이지에 반영
        tireFill.fillAmount = tireRatio;
    }
}
