using UnityEngine;
using UnityEngine.UI;

public class ChargingGameLogic : MonoBehaviour
{
    public RectTransform iconTransform;
    public RectTransform point1;
    public RectTransform point2;
    public RectTransform point3;
    public Image point1Circle;
    public Image point2Circle;
    public Image point3Circle;
    public Image gaugeBar;
    public ElectricIconMover iconMover;

    public Color normalColor = Color.white;
    public Color activeColor = Color.yellow; // 지금 눌러야 하는 위치 표시

    private RectTransform[] points;
    private Image[] circles;
    private int currentTargetIndex = 0; // 지금 목표 위치 (0=1번, 1=2번, 2=3번)

    private float gaugeValue = 0f;
    private float gaugeIncreasePerHit = 0.1f;

    private bool isFrozen = false;
    private float freezeTimer = 0f;

    private bool isGameActive = true;  

    void Start()
    {
        points = new RectTransform[] { point1, point2, point3 };
        circles = new Image[] { point1Circle, point2Circle, point3Circle };
        UpdateHighlight();
    }

    void Update()
    {
        if (!isGameActive) return;   // ← 추가: 게임 끝났으면 아무것도 안 함

        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0)
            {
                isFrozen = false;
                iconMover.enabled = true;
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            CheckTiming();
        }
    }

    public void StopGame()   // ← 추가: 매니저가 게임 끝날 때 호출할 함수
    {
        isGameActive = false;
    }

    void UpdateHighlight()
    {
        // 지금 목표인 위치만 색깔 바꿔서 강조
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i].color = (i == currentTargetIndex) ? activeColor : normalColor;
        }
    }

    void CheckTiming()
    {
        RectTransform activeTarget = points[currentTargetIndex];
        float distance = Vector3.Distance(iconTransform.localPosition, activeTarget.localPosition);
        float tolerance = 50f;

        if (distance <= tolerance)
        {
            // 성공
            gaugeValue = Mathf.Clamp01(gaugeValue + gaugeIncreasePerHit);
            gaugeBar.fillAmount = gaugeValue;
            Debug.Log("성공! 게이지: " + gaugeValue);

            // 다음 목표로 이동 (3번 성공하면 다시 1번으로 순환)
            currentTargetIndex = (currentTargetIndex + 1) % points.Length;
            UpdateHighlight();
        }
        else
        {
            // 실패
            Debug.Log("실패! 1초간 정지");
            isFrozen = true;
            freezeTimer = 1f;
            iconMover.enabled = false;
        }
    }

    public float GetGaugeValue()
    {
        return gaugeValue;
    }
}