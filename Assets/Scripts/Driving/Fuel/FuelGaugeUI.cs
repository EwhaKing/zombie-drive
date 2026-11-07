
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

        float fuelRatio = Mathf.Clamp01(
            (float)currentFuel / maxFuel
        );

        fuelFill.fillAmount = fuelRatio;
    }
}
