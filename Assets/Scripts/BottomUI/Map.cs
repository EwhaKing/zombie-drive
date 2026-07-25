using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject navigationPopup; // 네비게이션 팝업 패널

    // 1. 지도 팝업 열기
    public void OpenMap()
    {
        if (navigationPopup != null)
        {
            navigationPopup.SetActive(true);
            // 필요 시 게임 일시정지: Time.timeScale = 0f;
        }
    }

    // 2. 지도 팝업 닫기
    public void CloseMap()
    {
        if (navigationPopup != null)
        {
            navigationPopup.SetActive(false);
            // 필요 시 게임 재개: Time.timeScale = 1f;
        }
    }

    // 3. 지역(노드) 선택 시 실행될 함수
    public void SelectDestination(string locationName)
    {
        Debug.Log(locationName + "(으)로 이동합니다!");

        // TODO: 여기서 선택한 지역에 따른 이벤트 처리
        // 예: 회복 쉘터 이벤트, 스토리 진행, 씬 내부 위치 이동 등

        // 목적지를 선택했으니 지도 팝업을 닫음
        CloseMap();
    }
}