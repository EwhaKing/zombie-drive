using System;
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


    // =========================
    // 게임 종료 이벤트
    // true  = 생존 성공
    // false = 생존 실패
    // =========================

    public event Action<bool> GameEnded;


    // =========================
    // Unity 기본 함수
    // =========================

    private void Start()
    {
        StartGame();
    }


    private void Update()
    {
        if (!IsGameRunning)
            return;


        // 타이머 감소
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
        // 목숨 감소하지 않음
        if (!IsGameRunning)
            return;


        CurrentLives--;


        // 0 아래로 내려가지 않도록
        CurrentLives =
            Mathf.Max(CurrentLives, 0);


        Debug.Log(
            "목숨 감소! 남은 목숨 : "
            + CurrentLives
        );


        UpdateHeartUI();


        // 목숨이 모두 사라짐
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


            heartObjects[i].SetActive(
                i < CurrentLives
            );
        }
    }


    // =========================
    // 30초 생존 성공
    // =========================

    private void OnTimerEnd()
    {
        Debug.Log(
            "30초 생존 성공!"
        );


        // true = 생존 성공
        GameEnded?.Invoke(true);
    }


    // =========================
    // 목숨 0 → 게임 오버
    // =========================

    private void OnGameOver()
    {
        Debug.Log(
            "생존 실패! GAME OVER!"
        );


        // false = 생존 실패
        GameEnded?.Invoke(false);
    }
}