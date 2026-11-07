
using UnityEngine;
using UnityEngine.UI;

public class TireGaugeUI : MonoBehaviour
{
    [Header("타이어 게이지 Fill 이미지")]
    [SerializeField] private Image tireFill;

    private void Update()
    {
        if (TireManager.Instance == null || tireFill == null)
            return;

        int currentTire = TireManager.Instance.currentTire;
        int maxTire = TireManager.Instance.maxTire;

        if (maxTire <= 0)
        {
            tireFill.fillAmount = 0f;
            return;
        }

        // 현재 타이어 상태를 0~1 비율로 변환
        tireFill.fillAmount = Mathf.Clamp01(
            (float)currentTire / maxTire
        );
    }
}
