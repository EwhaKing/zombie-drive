
using UnityEngine;
using TMPro;

public class HpNumberUI : MonoBehaviour
{
    [Header("체력 숫자 텍스트")]
    [SerializeField] private TextMeshProUGUI hpValueText;

    [Header("화면에 표시할 최대 체력 칸 수")]
    [SerializeField] private int displayMaxHp = 16;

    private void Update()
    {
        if (DrivingManager.Instance == null)
            return;

        int currentHp = DrivingManager.Instance.currentHp;
        int maxHp = DrivingManager.Instance.maxHp;

        if (maxHp <= 0 || displayMaxHp <= 0)
            return;

        // 실제 체력을 16칸 기준으로 환산
        float hpRatio = Mathf.Clamp01(
            (float)currentHp / maxHp
        );

        int displayHp = Mathf.CeilToInt(
            hpRatio * displayMaxHp
        );

        // 화면에 16/16, 12/16 등의 형태로 표시
        if (hpValueText != null)
        {
            hpValueText.text =
                $"{displayHp}/{displayMaxHp}";
        }
    }
}
