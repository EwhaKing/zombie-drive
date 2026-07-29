using UnityEngine;

// 팝업 창을 열고 닫는 간단한 스크립트
public class SimplePopup : MonoBehaviour
{
    [Header("열고 닫을 팝업")]
    [SerializeField] private GameObject popupPanel;

    private void Start()
    {
        // 게임 시작 시 팝업을 숨긴다.
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    // 오브젝트 버튼을 클릭했을 때 실행
    public void OpenPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    // 팝업의 닫기 버튼을 클릭했을 때 실행
    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }
}