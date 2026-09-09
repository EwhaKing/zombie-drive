using UnityEngine;

// 주행 화면 전체 상태 관리 (타이머, 체력, 내린 횟수, 랜덤 목적지 이동)[cite: 10]
public class DrivingManager : MonoBehaviour
{
    public static DrivingManager Instance;
    private string currentSelectedScene = ""; // 팝업 생성 시점에 미리 뽑아둘 목적지[cite: 10]

    [Header("날짜 설정")]
    public int currentDay = 1; //[cite: 10]

    [Header("타이머 설정")]
    public float popupInterval = 5f; //[cite: 10]
    private float timer; //[cite: 10]

    [Header("체력 설정")]
    public int maxHp = 100; //[cite: 10]
    public int currentHp = 100; //[cite: 10]
    public int hpCostPerFarm = 20; //[cite: 10]

    [Header("파밍 진행도")]
    public int farmedCountToday = 0; //[cite: 10]
    public const int REQUIRED_FARM_COUNT = 3; //[cite: 10]

    [Header("내리기 시 이동 가능한 장소")]
    [SerializeField]
    private string[] destinationScenes =
    {
        "StoreGame",
        "ChargingMinigame",
        "RepairShop"
    }; //[cite: 10]

    public bool isPopupActive = false; //[cite: 10]

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("GameManager"); //[cite: 10]
            if (prefab != null)
            {
                Instantiate(prefab); //[cite: 10]
            }
            else
            {
                Debug.LogError("GameManager 프리팹을 Resources 폴더에서 찾을 수 없습니다."); //[cite: 10]
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; //[cite: 10]
            transform.SetParent(null); //[cite: 10]
            DontDestroyOnLoad(gameObject); //[cite: 10]
        }
        else if (Instance != this)
        {
            Destroy(this); //[cite: 10]
            return;
        }
    }

    private void Update()
    {
        if (isPopupActive) return; //[cite: 10]

        timer += Time.deltaTime; //[cite: 10]

        if (timer >= popupInterval) //[cite: 10]
        {
            timer = 0f; //[cite: 10]
            ShowNavigationPopup(); //[cite: 10]
        }
    }

    // 일정 시간이 지나면 랜덤 장소를 확정하고 네비게이션 팝업 표시[cite: 10]
    private void ShowNavigationPopup()
    {
        if (WarningUI.Instance == null)
        {
            Debug.LogWarning("WarningUI를 찾을 수 없습니다."); //[cite: 10]
            return;
        }

        if (destinationScenes == null || destinationScenes.Length == 0)
        {
            Debug.LogError("이동 가능한 씬이 등록되어 있지 않습니다."); //[cite: 10]
            return;
        }

        // 1. 팝업이 뜨는 순간 랜덤 목적지 하나를 선정
        int randomIndex = Random.Range(0, destinationScenes.Length); //[cite: 10]
        currentSelectedScene = destinationScenes[randomIndex]; //[cite: 10]

        isPopupActive = true; //[cite: 10]

        // 2. UI 팝업에 체력 소모량과 뽑힌 장소명을 전달
        WarningUI.Instance.Show(hpCostPerFarm, currentSelectedScene); //[cite: 10]
    }

    // "내리지 않기" 선택 시[cite: 10]
    public void OnStayInCar()
    {
        isPopupActive = false; //[cite: 10]
        timer = 0f; //[cite: 10]
        Debug.Log("차에서 내리지 않았습니다."); //[cite: 10]
    }

    // "내리기" 선택 시 확정된 장소로 이동[cite: 10]
    public void StartFarming(int cost)
    {
        if (currentHp < cost)
        {
            Debug.LogWarning("체력이 부족합니다."); //[cite: 10]
            return;
        }

        currentHp -= cost; //[cite: 10]
        isPopupActive = true; //[cite: 10]

        if (string.IsNullOrEmpty(currentSelectedScene))
        {
            Debug.LogError("목적지가 설정되지 않았습니다."); //[cite: 10]
            isPopupActive = false; //[cite: 10]
            return;
        }

        Debug.Log("확정된 목적지로 이동 : " + currentSelectedScene); //[cite: 10]

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(currentSelectedScene); //[cite: 10]
        }
        else
        {
            Debug.LogError("SceneController가 씬에 존재하지 않습니다!"); //[cite: 10]
        }
    }

    // 미니게임/상점 등의 씬에서 돌아올 때 호출[cite: 10]
    public void ReturnFromDestination()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("SceneController를 찾을 수 없습니다."); //[cite: 10]
            return;
        }

        SceneController.Instance.LoadScene("DrivingScene"); //[cite: 10]
        OnReturnedToDrivingScene(); //[cite: 10]
    }

    public void ReturnFromFarming()
    {
        ReturnFromDestination(); //[cite: 10]
    }

    private void OnReturnedToDrivingScene()
    {
        farmedCountToday++; //[cite: 10]
        timer = 0f; //[cite: 10]

        Debug.Log($"DrivingScene 복귀 / 오늘 내린 횟수 : {farmedCountToday} / {REQUIRED_FARM_COUNT}"); //[cite: 10]

        if (farmedCountToday >= REQUIRED_FARM_COUNT)
        {
            EndDay(); //[cite: 10]
        }
        else
        {
            isPopupActive = false; //[cite: 10]
        }
    }

    private void EndDay()
    {
        currentDay++; //[cite: 10]
        farmedCountToday = 0; //[cite: 10]
        timer = 0f; //[cite: 10]
        isPopupActive = false; //[cite: 10]

        Debug.Log("다음 날 시작! Day " + currentDay); //[cite: 10]
    }
}