using UnityEngine;
using System.Collections;

public class BottomUIMove : MonoBehaviour
{
    public RectTransform bottomUI;   // BottomUI 오브젝트를 여기에 드래그
    public float hiddenY = -150f;    // 숨겼을 때 y좌표 (화면 아래로)
    public float shownY = 0f;        // 보일 때 y좌표
    public float duration = 0.3f;    // 움직이는 시간

    private bool isShown = true;
    private Coroutine currentMove;

    // 화살표 버튼의 OnClick()에 이 함수를 연결하세요
    public void ToggleUI()
    {
        if (currentMove != null) StopCoroutine(currentMove);

        float targetY = isShown ? hiddenY : shownY;
        currentMove = StartCoroutine(MoveTo(targetY));
        isShown = !isShown;
    }

    private IEnumerator MoveTo(float targetY)
    {
        Vector2 start = bottomUI.anchoredPosition;
        Vector2 end = new Vector2(start.x, targetY);
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            bottomUI.anchoredPosition = Vector2.Lerp(start, end, t / duration);
            yield return null;
        }
        bottomUI.anchoredPosition = end;
    }
}
