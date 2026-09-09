using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WarningUI : MonoBehaviour
{
    public static WarningUI Instance;

    [Header("연결할 오브젝트")]
    public GameObject popupPanel;        // PopPanel 오브젝트 자체[cite: 9]
    public Button getOffButton;          // "내리기" 버튼[cite: 9]
    public Button stayButton;            // "내리지 않기" 버튼[cite: 9]
    public TextMeshProUGUI warningText;   // "체력이 부족합니다" 표시용 텍스트[cite: 9]
    public TextMeshProUGUI locationText;  // [추가] "약 1km 앞에 OOO가 있습니다" 표기용 텍스트

    private int currentCost;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false); // 시작 시 팝업 비활성화[cite: 9]
        }

        // 버튼 이벤트 연결[cite: 9]
        if (getOffButton != null) getOffButton.onClick.AddListener(OnClickGetOff);
        if (stayButton != null) stayButton.onClick.AddListener(OnClickStay);
    }

    // DrivingManager에서 미리 정해진 장소와 함께 호출
    public void Show(int cost, string sceneName)
    {
        currentCost = cost;
        
        if (warningText != null) warningText.text = ""; // 경고 문구 초기화[cite: 9]

        // 씬 영문명을 한글 장소명으로 변환하여 UI 적용
        string displayLocationName = GetDisplayLocationName(sceneName);
        if (locationText != null)
        {
            locationText.text = $"네비게이션 알림!\n약 --km 앞에 <b>[{displayLocationName}]</b>이(가) 있습니다.";
        }

        if (popupPanel != null) popupPanel.SetActive(true); // 팝업 활성화[cite: 9]
    }

    // 씬 이름을 유저에게 보여줄 한글 장소명으로 변환
    private string GetDisplayLocationName(string sceneName)
    {
        switch (sceneName)
        {
            case "StoreGame":
                return "편의점";
            case "ChargingMinigame":
                return "충전소";
            case "RepairShop":
                return "정비소";
            default:
                return "알 수 없는 장소";
        }
    }

    // "내리지 않기" 클릭 시[cite: 9]
    private void OnClickStay()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        if (DrivingManager.Instance != null) DrivingManager.Instance.OnStayInCar();
    }

    // "내리기" 클릭 시[cite: 9]
    private void OnClickGetOff()
    {
        if (DrivingManager.Instance != null)
        {
            if (DrivingManager.Instance.currentHp >= currentCost)
            {
                if (popupPanel != null) popupPanel.SetActive(false);
                DrivingManager.Instance.StartFarming(currentCost);
            }
            else
            {
                if (warningText != null) warningText.text = "체력이 부족합니다";
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}