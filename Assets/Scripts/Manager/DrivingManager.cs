using UnityEngine;

// 주행 화면 전체 상태 관리 (타이머, 체력, 내린 횟수, 랜덤 목적지 이동)
public class DrivingManager : MonoBehaviour
{
    public static DrivingManager Instance;
    private string currentSelectedScene = ""; // 팝업 생성 시점에 미리 뽑아둘 목적지

    [Header("날짜 설정")]
    public int currentDay = 1; 

    [Header("타이머 설정")]
    public float popupInterval = 5f; 
    private float timer; 

    [Header("체력 설정")]
    public int maxHp = 100; 
    public int currentHp = 100; 
    public int hpCostPerFarm = 20; 

    [Header("파밍 진행도")]
    public int farmedCountToday = 0; 
    public const int REQUIRED_FARM_COUNT = 3; 

    [Header("내리기 시 이동 가능한 장소")]
    [SerializeField]
    private string[] destinationScenes =
    {
        "StoreGame",
        "ChargingMinigame",
        "RepairShop"
    }; 

    public bool isPopupActive = false; 

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("GameManager"); 
            if (prefab != null)
            {
                Instantiate(prefab); 
            }
            else
            {
                Debug.LogError("GameManager 프리팹을 Resources 폴더에서 찾을 수 없습니다."); 
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
            transform.SetParent(null); 
            DontDestroyOnLoad(gameObject); 
        }
        else if (Instance != this)
        {
            Destroy(this); 
            return;
        }
    }

    private void Update()
    {
        if (isPopupActive) return; 

        timer += Time.deltaTime; 

        if (timer >= popupInterval) 
        {
            timer = 0f; 
            ShowNavigationPopup(); 
        }
    }

    // 일정 시간이 지나면 랜덤 장소를 확정하고 네비게이션 팝업 표시
    private void ShowNavigationPopup()
    {
        if (WarningUI.Instance == null)
        {
            Debug.LogWarning("WarningUI를 찾을 수 없습니다."); 
            return;
        }

        if (destinationScenes == null || destinationScenes.Length == 0)
        {
            Debug.LogError("이동 가능한 씬이 등록되어 있지 않습니다."); 
            return;
        }

        // 1. 팝업이 뜨는 순간 랜덤 목적지 하나를 선정
        int randomIndex = Random.Range(0, destinationScenes.Length); 
        currentSelectedScene = destinationScenes[randomIndex]; 

        isPopupActive = true; 

        // 2. UI 팝업에 체력 소모량과 뽑힌 장소명을 전달
        WarningUI.Instance.Show(hpCostPerFarm, currentSelectedScene); 
    }

    // 내리지 않기 선택 시
    public void OnStayInCar()
    {
        isPopupActive = false; 
        timer = 0f; 
        Debug.Log("차에서 내리지 않았습니다."); 
    }

    // 내리기 선택 시 확정된 장소로 이동
    public void StartFarming(int cost)
    {
        if (currentHp < cost)
        {
            Debug.LogWarning("체력이 부족합니다."); 
            return;
        }

        currentHp -= cost; 
        isPopupActive = true; 

        if (string.IsNullOrEmpty(currentSelectedScene))
        {
            Debug.LogError("목적지가 설정되지 않았습니다."); 
            isPopupActive = false; 
            return;
        }

        Debug.Log("확정된 목적지로 이동 : " + currentSelectedScene); 

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(currentSelectedScene); 
        }
        else
        {
            Debug.LogError("SceneController가 씬에 존재하지 않습니다!"); 
        }
    }

    // 미니게임/상점 등의 씬에서 돌아올 때 호출
    public void ReturnFromDestination()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("SceneController를 찾을 수 없습니다."); 
            return;
        }

        SceneController.Instance.LoadScene("DrivingScene"); 
        OnReturnedToDrivingScene(); 
    }

    public void ReturnFromFarming()
    {
        ReturnFromDestination(); 
    }

    private void OnReturnedToDrivingScene()
    {
        farmedCountToday++; 
        timer = 0f; 

        Debug.Log($"DrivingScene 복귀 / 오늘 내린 횟수 : {farmedCountToday} / {REQUIRED_FARM_COUNT}"); 

        if (farmedCountToday >= REQUIRED_FARM_COUNT)
        {
            EndDay(); 
        }
        else
        {
            isPopupActive = false; 
        }
    }

    private void EndDay()
    {
        currentDay++; 
        farmedCountToday = 0; 
        timer = 0f; 
        isPopupActive = false; 

        Debug.Log("다음 날 시작! Day " + currentDay); 
    }
}