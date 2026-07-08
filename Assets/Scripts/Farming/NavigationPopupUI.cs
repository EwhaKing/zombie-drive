using UnityEngine;
using UnityEngine.UI;
// TMP(TextMeshPro)로 텍스트/버튼을 만들었다면 아래 줄 추가 필요
using TMPro;

public class NavigationPopupUI : MonoBehaviour
{
    public static NavigationPopupUI Instance;

    [Header("연결할 오브젝트")]
    public GameObject popupPanel;      // PopPanel 오브젝트 자체
    public Button getOffButton;        // "내리기" 버튼 (Getoff)
    public Button stayButton;          // "내리지 않기" 버튼 (Stay)
    public TextMeshProUGUI warningText; // "체력이 부족합니다" 표시용 텍스트 (없다면 새로 하나 만들어서 연결)

    private int currentCost; // 이번 알림창에서 소모될 체력 (Show()로 전달받음)

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        popupPanel.SetActive(false); // 게임 시작 시엔 알림창 꺼둠

        // 버튼 클릭 시 실행될 함수를 코드로 연결
        getOffButton.onClick.AddListener(OnClickGetOff);
        stayButton.onClick.AddListener(OnClickStay);
    }

    // DrivingManager가 5분마다 이 함수를 호출해서 알림창을 띄움
    public void Show(int cost)
    {
        currentCost = cost;
        if (warningText != null) warningText.text = ""; // 경고 문구 초기화
        popupPanel.SetActive(true);
    }

    // "내리지 않기" 버튼 클릭 시 실행
    private void OnClickStay()
    {
        popupPanel.SetActive(false);
        DrivingManager.Instance.OnStayInCar(); // DrivingManager에게 "안 내렸다"고 알림
    }

    // "차에서 내리기" 버튼 클릭 시 실행
    private void OnClickGetOff()
    {
        if (DrivingManager.Instance.currentHp >= currentCost)
        {
            // 체력 충분 → 알림창 닫고 파밍 시작
            popupPanel.SetActive(false);
            DrivingManager.Instance.StartFarming(currentCost);
        }
        else
        {
            // 체력 부족 → 알림창은 그대로 두고 경고 문구만 표시
            if (warningText != null) warningText.text = "체력이 부족합니다";
        }
    }
}