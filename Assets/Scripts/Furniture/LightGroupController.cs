using UnityEngine;

public class LightGroupController : MonoBehaviour
{
    [Header("켜졌을 때 보여줄 빛 효과")]
    [SerializeField] private GameObject wallLightGlow;

    private bool isLightOn = false;

    private void Start()
    {
        // 게임 시작 시 조명은 꺼진 상태
        isLightOn = false;

        if (wallLightGlow != null)
        {
            wallLightGlow.SetActive(false);
        }
    }

    // 왼쪽 또는 오른쪽 조명을 클릭하면 호출
    public void ToggleLights()
    {
        isLightOn = !isLightOn;

        if (wallLightGlow != null)
        {
            wallLightGlow.SetActive(isLightOn);
        }

        if (isLightOn)
        {
            Debug.Log("조명이 켜졌습니다.");
        }
        else
        {
            Debug.Log("조명이 꺼졌습니다.");
        }
    }

    // 나중에 낮/밤이나 다른 시스템에서
    // 강제로 조명을 켜고 끌 때 사용할 수 있음
    public void SetLights(bool turnOn)
    {
        isLightOn = turnOn;

        if (wallLightGlow != null)
        {
            wallLightGlow.SetActive(isLightOn);
        }

        Debug.Log(
            isLightOn
                ? "조명이 켜졌습니다."
                : "조명이 꺼졌습니다."
        );
    }
}