
using UnityEngine;
using UnityEngine.UI;

public class FuelGaugeUI : MonoBehaviour
{
    [Header("연료 게이지 Fill 이미지")]
    [SerializeField] private Image fuelFill;

    private void Update()
    {
        if (FuelManager.Instance == null || fuelFill == null)
            return;

        int currentFuel = FuelManager.Instance.currentFuel;
        int maxFuel = FuelManager.Instance.maxFuel;

        if (maxFuel <= 0)
        {
            fuelFill.fillAmount = 0f;
            return;
        }

        // 현재 연료량을 0~1 비율로 변환
        fuelFill.fillAmount = Mathf.Clamp01(
            (float)currentFuel / maxFuel
        );
    }
}
