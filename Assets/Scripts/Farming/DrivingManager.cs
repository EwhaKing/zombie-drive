using UnityEngine;

// 주행 화면의 전체 상태(타이머, 체력, 오늘 내린 횟수)를 관리하는 매니저
public class DrivingManager : MonoBehaviour
{
    public static DrivingManager Instance;

    [Header("날짜 설정")]
    public int currentDay = 1;

    [Header("타이머 설정")]
    public float popupInterval = 5f;   // 알림창이 뜨는 간격 (5분 = 300초). 테스트 땐 5~10으로 줄여서 확인하세요.
    private float timer;                 // 현재까지 흐른 시간

    [Header("체력 설정")]
    public int maxHp = 100;
    public int currentHp = 100;          // 현재 체력
    public int hpCostPerFarm = 20;       // 이번 알림창에서 소모될 체력 (알림창마다 다르면 여기 값을 바꿔주면 됨)

    [Header("파밍 진행도")]
    public int farmedCountToday = 0;             // 오늘 내린 횟수 (화면에는 표시 안 함, 내부 카운트용)
    public const int REQUIRED_FARM_COUNT = 3;    // 3번 채우면 하루 종료

    // 지금 알림창이 화면에 떠 있는 상태인지 여부. 떠 있는 동안엔 타이머가 멈춰야 함.
    public bool isPopupActive = false;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 알림창이 떠 있는 동안엔 시간이 흐르지 않게 함
        if (isPopupActive) return;

        timer += Time.deltaTime;

        if (timer >= popupInterval)
        {
            timer = 0f;
            ShowNavigationPopup();
        }
    }

    // 5분이 지나서 알림창을 띄우는 함수
    private void ShowNavigationPopup()
    {
        if (NavigationPopupUI.Instance == null)
        {
            return;
        }

        isPopupActive = true;
        NavigationPopupUI.Instance.Show(hpCostPerFarm);
    }

    // ----------------------------------------------------------------
    // 아래부터는 NavigationPopupUI.cs 에서 버튼 클릭 시 호출해주는 함수들
    // ----------------------------------------------------------------

    // "내리지 않기"를 눌렀을 때 호출 (NavigationPopupUI에서 호출)
    public void OnStayInCar()
    {
        isPopupActive = false;
        // timer는 이미 0으로 리셋되어 있으므로(ShowNavigationPopup 진입 전), 여기서 다시 0으로 안 해도 됨
        // 만약 "닫자마자 즉시 5분 리셋"을 강조하고 싶다면 아래 줄을 추가해도 무방
        timer = 0f;
    }

    // "차에서 내리기"를 누르고, 체력도 충분해서 실제로 파밍을 시작할 때 호출
    public void StartFarming(int cost)
    {
        currentHp -= cost;          // 체력 소모
        isPopupActive = true;       // 파밍 씬에 있는 동안에는 주행 타이머가 작동하지 않도록 유지

        // 파밍 씬으로 전환 (콜백 필요 없음, 그냥 이동만 하면 됨)
        SceneTransition.Instance.GoToScene("Farming");
    }

    // 파밍 씬의 "복귀 버튼"을 눌렀을 때 호출 (FarmingSceneController에서 호출)
    public void ReturnFromFarming()
    {
        // 씬 전환이 끝난 "직후" OnReturnedToDrivingScene을 실행해달라고 콜백으로 넘김
        SceneTransition.Instance.GoToSceneWithCallback("DrivingScene", OnReturnedToDrivingScene);
    }

    // 주행 씬으로 복귀 완료된 직후 실행되는 함수
    private void OnReturnedToDrivingScene()
    {
        farmedCountToday += 1;   // 오늘 내린 횟수 +1
        timer = 0f;              // 복귀 완료 시점부터 5분 타이머 재시작

        // 파밍 3회 완료 시 다음 날로 전환
        if (farmedCountToday >= REQUIRED_FARM_COUNT)
        {
            EndDay();
        }
        else
        {
            // 아직 3회를 채우지 않았다면 주행 재개
            isPopupActive = false;
        }

        // DrivingScene으로 돌아왔으므로 다시 타이머 작동
        //isPopupActive = false;

        if (farmedCountToday >= REQUIRED_FARM_COUNT)
        {
            EndDay();
        }
    }

    // 하루치 파밍(3회)이 다 끝났을 때 호출
    private void EndDay()
    {
        // 날짜 증가
        currentDay++;

        // 오늘 파밍 횟수 초기화
        farmedCountToday = 0;

        // 팝업 타이머도 처음부터 다시 시작
        timer = 0f;

        // 다시 주행 가능 상태로 변경
        isPopupActive = false;

        Debug.Log("다음 날 시작! Day " + currentDay);

        //enabled = false; // 이 스크립트의 Update()를 멈춰서 타이머 정지
        //Debug.Log("하루 끝! 다음 날로 전환 처리 필요");
        // TODO: 다음 날로 넘어가는 로직(DayManager 등)을 여기서 호출하면 됨
    }
}