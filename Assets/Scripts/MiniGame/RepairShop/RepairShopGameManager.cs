using TMPro;
using UnityEngine;

public class RepairShopGameManager : MonoBehaviour
{
    [Header("단계별 제한시간")]
    [SerializeField] private float stage1TimeLimit = 10f;
    [SerializeField] private float stage2TimeLimit = 10f;
    [SerializeField] private float stage3TimeLimit = 10f;

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    [Header("각 단계")]
    [SerializeField] private WheelRemovalMiniGame wheelMiniGame;
    [SerializeField] private BoltPhaseManager boltPhaseManager;
    [SerializeField] private TireInstallMiniGame tireInstallMiniGame;


    private int currentStage = 0;

    private float remainingTime = 0f;
    private float currentProgress = 0f;

    private bool timerRunning = false;


    // 각 단계 최종 완성도
    private float stage1Completion = 0f;
    private float stage2Completion = 0f;
    private float stage3Completion = 0f;


    // 최종 완성도
    public float FinalCompletion01 { get; private set; } = 0f;

    public float FinalCompletionPercent =>
        FinalCompletion01 * 100f;


    private void Update()
    {
        if (!timerRunning)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            UpdateStatusText();

            HandleTimeout();

            return;
        }

        UpdateStatusText();
    }


    // =====================================
    // 정비 전체 시작
    // =====================================
    public void BeginRepair()
    {
        stage1Completion = 0f;
        stage2Completion = 0f;
        stage3Completion = 0f;

        FinalCompletion01 = 0f;

        StartStage(1);
    }


    // =====================================
    // 단계 시작
    // =====================================
    public void StartStage(int stage)
    {
        currentStage = stage;
        currentProgress = 0f;

        switch (stage)
        {
            case 1:
                remainingTime = stage1TimeLimit;
                break;

            case 2:
                remainingTime = stage2TimeLimit;
                break;

            case 3:
                remainingTime = stage3TimeLimit;
                break;
        }

        timerRunning = true;

        UpdateStatusText();
    }


    // =====================================
    // 현재 단계 진행도 갱신
    // 0 ~ 1
    // =====================================
    public void UpdateCurrentProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);

        UpdateStatusText();
    }


    // =====================================
    // 시간 안에 단계 완료
    // =====================================
    public void CompleteStage(
        int stage,
        float completion)
    {
        completion =
            Mathf.Clamp01(completion);

        SaveStageCompletion(
            stage,
            completion
        );

        // 현재 진행 중인 단계가 완료됐으면
        // 타이머 정지
        if (currentStage == stage)
        {
            timerRunning = false;
        }
    }


    // =====================================
    // 제한시간 종료
    // =====================================
    private void HandleTimeout()
    {
        timerRunning = false;

        int timedOutStage = currentStage;

        // 그 순간까지의 진행도를 기록
        SaveStageCompletion(
            timedOutStage,
            currentProgress
        );

        Debug.Log(
            timedOutStage +
            "단계 시간 초과! 완성도: " +
            (currentProgress * 100f).ToString("F1") +
            "%"
        );


        // 다음 단계로 강제 이동
        switch (timedOutStage)
        {
            case 1:

                if (wheelMiniGame != null)
                    wheelMiniGame.ForceFinishByTimeout();

                break;


            case 2:

                if (boltPhaseManager != null)
                    boltPhaseManager.ForceFinishByTimeout();

                break;


            case 3:

                if (tireInstallMiniGame != null)
                    tireInstallMiniGame.ForceFinishByTimeout();

                FinishRepair();

                break;
        }
    }


    // =====================================
    // 단계 완성도 저장
    // =====================================
    private void SaveStageCompletion(
        int stage,
        float completion)
    {
        switch (stage)
        {
            case 1:
                stage1Completion = completion;
                break;

            case 2:
                stage2Completion = completion;
                break;

            case 3:
                stage3Completion = completion;
                break;
        }
    }


    public void FinishRepair()
    {
        timerRunning = false;

        // =====================================
        // 1~3단계 완성도의 평균
        // =====================================

        FinalCompletion01 =
            (
                stage1Completion +
                stage2Completion +
                stage3Completion
            ) / 3f;


        float finalPercent =
            FinalCompletion01 * 100f;


        Debug.Log(
            "1단계 완성도 : "
            + (stage1Completion * 100f).ToString("F1")
            + "%"
        );

        Debug.Log(
            "2단계 완성도 : "
            + (stage2Completion * 100f).ToString("F1")
            + "%"
        );

        Debug.Log(
            "3단계 완성도 : "
            + (stage3Completion * 100f).ToString("F1")
            + "%"
        );

        Debug.Log(
            "최종 완성도 : "
            + finalPercent.ToString("F1")
            + "%"
        );


        if (statusText != null)
        {
            statusText.text =
                "최종 완성도 : "
                + finalPercent.ToString("F1")
                + "%";
        }
    }


    // =====================================
    // 테스트용 화면 표시
    // =====================================
    private void UpdateStatusText()
    {
        if (statusText == null)
            return;

        statusText.text =
            currentStage +
            "단계 | 남은 시간 : " +
            remainingTime.ToString("F1") +
            "초 | 진행도 : " +
            (currentProgress * 100f).ToString("F0") +
            "%";
    }
}