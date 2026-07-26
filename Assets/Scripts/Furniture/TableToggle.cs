using UnityEngine;

public class TableToggle : MonoBehaviour
{
    private bool isSitting = false;
    public GameObject eatButton;
    public GameObject warningText;
    public bool hasFood = false; // 테스트용: Inspector에서 체크박스로 바꿔볼 수 있음

    public void OnTableClick()
    {
        if (!isSitting)
        {
            isSitting = true;
            Debug.Log("캐릭터가 식탁으로 이동하여 앉음, 냉장고 문 열림");
        }
        else
        {
            eatButton.SetActive(true);
        }
    }

    public void OnEatClick()
    {
        if (hasFood)
        {
            Debug.Log("밥 먹는 모션 재생, 배고픔 게이지 채워짐");
        }
        else
        {
            warningText.SetActive(true);
            Invoke(nameof(HideWarning), 2f);
        }
    }

    void HideWarning()
    {
        warningText.SetActive(false);
    }
}