using UnityEngine;

public class ElectricIconMover : MonoBehaviour
{
    public RectTransform iconTransform;
    public RectTransform point1;
    public RectTransform point2;
    public RectTransform point3;

    public float baseSpeed = 1f;      // 시작 속도
    public float speedIncreasePerSecond = 0.05f; // 시간이 지날수록 빨라지는 정도

    private float elapsedTime = 0f;
    private RectTransform[] points;

    void Start()
    {
        points = new RectTransform[] { point1, point2, point3 };
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        float currentSpeed = baseSpeed + (elapsedTime * speedIncreasePerSecond);

        // 0~1 사이를 왕복하는 값 (PingPong)
        float t = Mathf.PingPong(elapsedTime * currentSpeed, 2f);

        Vector3 newPosition;
        if (t <= 1f)
        {
            // Point1 -> Point2 -> Point3 순서로 이동 (0~1 구간: 1->2, 1~2 구간: 2->3 이런 식으로 세분화 가능)
            newPosition = Vector3.Lerp(point1.localPosition, point3.localPosition, t);
        }
        else
        {
            newPosition = Vector3.Lerp(point3.localPosition, point1.localPosition, t - 1f);
        }

        iconTransform.localPosition = newPosition;
    }
}