using UnityEngine;
using UnityEngine.UI;

public class ChargingGameLogic : MonoBehaviour
{
    public RectTransform iconTransform;
    public RectTransform point1;
    public RectTransform point2;
    public RectTransform point3;
    public Image gaugeBar;

    private float gaugeValue = 0f;
    private float gaugeIncreasePerHit = 0.1f; // 한 번 성공할 때마다 오르는 양

    private bool isFrozen = false;   // 실패 후 1초 멈춤 상태
    private float freezeTimer = 0f;

    private float elapsedTime = 0f;
    public ElectricIconMover iconMover; // 2단계에서 만든 이동 스크립트 참조

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // 실패 후 1초간 멈춤 처리
        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0)
            {
                isFrozen = false;
                iconMover.enabled = true; // 다시 이동 재개
            }
            return; // 멈춰있는 동안은 터치 판정도 안 함
        }

        // 화면 터치(마우스 클릭) 감지
        if (Input.GetMouseButtonDown(0))
        {
            CheckTiming();
        }
    }

    RectTransform GetActiveTarget()
    {
        if (elapsedTime < 10f) return point1;
        else if (elapsedTime < 20f) return point2;
        else return point3;
    }

    void CheckTiming()
    {
        RectTransform activeTarget = GetActiveTarget();
        float distance = Vector3.Distance(iconTransform.localPosition, activeTarget.localPosition);
        float tolerance = 50f; // 오차범위 (나중에 아트 확정되면 조정)

        if (distance <= tolerance)
        {
            // 성공
            gaugeValue = Mathf.Clamp01(gaugeValue + gaugeIncreasePerHit);
            gaugeBar.fillAmount = gaugeValue;
            Debug.Log("성공! 게이지: " + gaugeValue);
        }
        else
        {
            // 실패
            Debug.Log("실패! 1초간 정지");
            isFrozen = true;
            freezeTimer = 1f;
            iconMover.enabled = false; // 이동 멈추기
            // TODO: 나중에 진동 효과(애니메이션) 여기 추가
        }
    }

    public float GetGaugeValue()
    {
        return gaugeValue;
    }
}