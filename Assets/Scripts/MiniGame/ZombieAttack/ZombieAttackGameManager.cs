using UnityEngine;
using TMPro;

public class ZombieAttackGameManager : MonoBehaviour
{
    // =========================
    // 타이머
    // =========================

    [Header("게임 시간")]
    [SerializeField] private float gameDuration = 30f;

    [Header("타이머 UI")]
    [SerializeField] private TMP_Text timerText;

    private float remainingTime;


    // =========================
    // 목숨
    // =========================

    [Header("목숨 설정")]
    [SerializeField] private int maxLives = 3;

    [SerializeField] private GameObject[] heartObjects;

    public int CurrentLives { get; private set; }


    // =========================
    // 게임 상태
    // =========================

    public bool IsGameRunning { get; private set; }


    private void Start()
    {
        StartGame();
    }


    private void Update()
    {
        if (!IsGameRunning)
            return;
        
        // =========================
        // 타이머 감소
        // =========================

        remainingTime -= Time.deltaTime;


        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            UpdateTimerText();

            IsGameRunning = false;

            OnTimerEnd();

            return;
        }


        UpdateTimerText();
    }


    // =========================
    // 게임 시작
    // =========================

    private void StartGame()
    {
        remainingTime = gameDuration;

        CurrentLives = maxLives;

        IsGameRunning = true;


        UpdateTimerText();
        UpdateHeartUI();
    }


    // =========================
    // 타이머 UI
    // =========================

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;


        int totalSeconds =
            Mathf.CeilToInt(remainingTime);


        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;


        timerText.text =
            $"{minutes:00}:{seconds:00}";
    }


    // =========================
    // 목숨 감소
    // =========================

    public void LoseLife()
    {
        // 이미 게임이 끝났으면
        // 더 이상 목숨 감소하지 않음
        if (!IsGameRunning)
            return;


        // 목숨 감소
        CurrentLives--;


        // 0 아래로 내려가는 것 방지
        CurrentLives =
            Mathf.Max(CurrentLives, 0);


        Debug.Log(
            "목숨 감소! 남은 목숨 : "
            + CurrentLives
        );


        // UI 갱신
        UpdateHeartUI();


        // 목숨이 전부 사라졌다면
        if (CurrentLives <= 0)
        {
            IsGameRunning = false;

            OnGameOver();
        }
    }


    // =========================
    // 하트 UI 갱신
    // =========================

    private void UpdateHeartUI()
    {
        if (heartObjects == null)
            return;


        for (int i = 0;
             i < heartObjects.Length;
             i++)
        {
            if (heartObjects[i] == null)
                continue;


            // 현재 목숨보다
            // 작은 번호의 하트만 표시
            heartObjects[i].SetActive(
                i < CurrentLives
            );
        }
    }


    // =========================
    // 30초 종료
    // =========================

    private void OnTimerEnd()
    {
        Debug.Log(
            "30초 생존 성공!"
        );

        // 나중에
        // 승리 처리 연결
    }


    // =========================
    // 게임 오버
    // =========================

    private void OnGameOver()
    {
        Debug.Log(
            "목숨을 모두 잃었습니다. GAME OVER!"
        );

        // 나중에
        // Game Over UI
        // 다른 기능 연동
        // 등을 여기서 처리
    }
}