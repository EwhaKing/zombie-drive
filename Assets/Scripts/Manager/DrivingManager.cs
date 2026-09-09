using UnityEngine;

// 주행 화면의 전체 상태
// 타이머, 체력, 오늘 내린 횟수, 랜덤 목적지 이동 관리
public class DrivingManager : MonoBehaviour
{
    public static DrivingManager Instance;

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

    // 팝업 또는 다른 장소에 있는 동안
    // DrivingScene 타이머가 작동하지 않게 함
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
        // 1. Instance가 비어있다면 자기 자신을 static 변수에 할당
        if (Instance == null)
        {
            Instance = this;
            // GameManager가 최상위 오브젝트가 아니라면 부모를 해제하여 Root로 생성
            transform.SetParent(null); 
            DontDestroyOnLoad(gameObject);
        }
        // 2. 이미 Instance가 존재하고, 그게 자기 자신이 아니라면 (중복 생성된 경우)
        else if (Instance != this)
        {
            // 오브젝트(gameObject) 전체를 지우지 않고, 중복 추가된 '스크립트 컴포넌트'만 삭제
            Destroy(this);
            return;
        }
    }


    private void Update()
    {
        // 팝업이 떠 있거나 다른 장소에 있는 동안에는
        // 타이머 정지
        if (isPopupActive)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= popupInterval)
        {
            timer = 0f;

            ShowNavigationPopup();
        }
    }


    // 일정 시간이 지나면 네비게이션 팝업 표시
    private void ShowNavigationPopup()
    {
        if (NavigationPopupUI.Instance == null)
        {
            Debug.LogWarning("NavigationPopupUI를 찾을 수 없습니다.");
            return;
        }

        isPopupActive = true;

        NavigationPopupUI.Instance.Show(hpCostPerFarm);
    }


    // -------------------------------------------------------
    // 내리지 않기
    // -------------------------------------------------------

    public void OnStayInCar()
    {
        isPopupActive = false;

        // 다시 처음부터 시간 측정
        timer = 0f;

        Debug.Log("차에서 내리지 않았습니다.");
    }


    // -------------------------------------------------------
    // 내리기
    // NavigationPopupUI에서 기존처럼 StartFarming(cost)를 호출하면 됨
    // -------------------------------------------------------

    public void StartFarming(int cost)
    {
        // 혹시 체력 확인이 NavigationPopupUI에 없을 경우를 대비
        if (currentHp < cost)
        {
            Debug.LogWarning("체력이 부족합니다.");
            return;
        }

        // 체력 감소
        currentHp -= cost;

        // 다른 씬에 있는 동안 Driving 타이머 정지
        isPopupActive = true;


        // 등록된 목적지가 없는 경우
        if (destinationScenes == null || destinationScenes.Length == 0)
        {
            Debug.LogError("이동 가능한 씬이 등록되어 있지 않습니다.");

            isPopupActive = false;

            return;
        }


        // 0 ~ destinationScenes.Length - 1 중 랜덤 선택
        int randomIndex = Random.Range(0, destinationScenes.Length);

        string selectedScene = destinationScenes[randomIndex];


        Debug.Log("랜덤 목적지 선택 : " + selectedScene);


        // [수정됨] SceneController를 이용해 선택된 씬으로 이동
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(selectedScene);
        }
        else
        {
            Debug.LogError("SceneController가 씬에 존재하지 않습니다!");
        }
    }


    // -------------------------------------------------------
    // StoreGame / ChargingMinigame / RepairShop에서
    // 돌아가기 버튼을 누르면 호출
    // -------------------------------------------------------

    public void ReturnFromDestination()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("SceneController를 찾을 수 없습니다.");
            return;
        }

        // [수정됨] SceneController를 이용해 DrivingScene으로 이동 후 복귀 메서드 호출
        SceneController.Instance.LoadScene("DrivingScene");
        OnReturnedToDrivingScene();
    }


    // -------------------------------------------------------
    // 예전에 Farming 씬에서 사용하던 코드가 남아있을 경우
    // 깨지지 않게 하기 위한 함수
    // -------------------------------------------------------

    public void ReturnFromFarming()
    {
        ReturnFromDestination();
    }


    // -------------------------------------------------------
    // DrivingScene으로 복귀한 직후 실행
    // -------------------------------------------------------

    private void OnReturnedToDrivingScene()
    {
        // 오늘 실제로 내린 횟수 증가
        farmedCountToday++;

        // 다시 타이머 처음부터 시작
        timer = 0f;


        Debug.Log(
            "DrivingScene 복귀 / 오늘 내린 횟수 : "
            + farmedCountToday
            + " / "
            + REQUIRED_FARM_COUNT
        );


        // 3번 완료했다면 하루 종료
        if (farmedCountToday >= REQUIRED_FARM_COUNT)
        {
            EndDay();
        }
        else
        {
            // 아직 3번이 아니라면 다시 주행 시작
            isPopupActive = false;
        }
    }


    // -------------------------------------------------------
    // 하루 종료
    // -------------------------------------------------------

    private void EndDay()
    {
        currentDay++;

        farmedCountToday = 0;

        timer = 0f;

        isPopupActive = false;


        Debug.Log("다음 날 시작! Day " + currentDay);
    }
}