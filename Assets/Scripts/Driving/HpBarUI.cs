using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HpBarUI : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    private void Update()
    {
        if (DrivingManager.Instance == null)
            return;

        hpSlider.maxValue = DrivingManager.Instance.maxHp;
        hpSlider.value = DrivingManager.Instance.currentHp;

        hpText.text =
            $"{DrivingManager.Instance.currentHp} / {DrivingManager.Instance.maxHp}";
    }
}