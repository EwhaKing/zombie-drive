using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RepairShopGameManager : MonoBehaviour
{
    public enum RepairStage
    {
        Shop,
        RemoveWheel,
        Bolts,
        AttachWheel,
        Result
    }

    [Header("화면")]
    public GameObject shopView;
    public GameObject wheelGameView;
    public GameObject resultPanel;

    [Header("상단 UI")]
    public TMP_Text stageText;
    public TMP_Text timerText;

    [Header("게이지")]
    public Image gaugeFill;

    [Header("바퀴")]
    public Image oldTireImage;
    public Image newTireImage;
    public WheelRotateInput wheelRotateInput;

    [Header("나사")]
    public GameObject boltsRoot;
    public List<BoltSlot> boltSlots;

    [Header("시간")]
    public float stage1TimeLimit = 10f;
    public float stage2TimeLimit = 10f;
    public float stage3TimeLimit = 10f;

    [Header("필요 회전")]
    public float stage1RequiredTurns = 30f;
    public float stage3RequiredTurns = 30f;

    [Header("결과")]
    public TMP_Text resultText;

    private RepairStage currentStage;

    private float remainingTime;

    private float stage1RotatedDegrees;
    private float stage3RotatedDegrees;

    private float stage1Score;
    private float stage2Score;
    private float stage3Score;

    private bool stage1TimedOut;
    private bool stage2TimedOut;
    private bool stage3TimedOut;

    public float FinalRepairRatio { get; private set; }

    public bool IsBoltStageActive
    {
        get { return currentStage == RepairStage.Bolts; }
    }


    private void Start()
    {
        ShowShop();
    }


    private void Update()
    {
        if (currentStage != RepairStage.RemoveWheel &&
            currentStage != RepairStage.Bolts &&
            currentStage != RepairStage.AttachWheel)
        {
            return;
        }

        remainingTime -= Time.unscaledDeltaTime;

        if (remainingTime < 0f)
            remainingTime = 0f;

        UpdateTimerText();

        if (remainingTime <= 0f)
        {
            HandleTimeout();
        }
    }


    // =========================
    // 처음 정비소 화면
    // =========================

    public void ShowShop()
    {
        currentStage = RepairStage.Shop;

        shopView.SetActive(true);
        wheelGameView.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }


    // =========================
    // 캠핑카 터치
    // =========================

    public void BeginMiniGame()
    {
        shopView.SetActive(false);
        wheelGameView.SetActive(true);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        ResetGame();

        StartStage1();
    }


    private void ResetGame()
    {
        stage1RotatedDegrees = 0f;
        stage3RotatedDegrees = 0f;

        stage1Score = 0f;
        stage2Score = 0f;
        stage3Score = 0f;

        stage1TimedOut = false;
        stage2TimedOut = false;
        stage3TimedOut = false;

        FinalRepairRatio = 0f;

        gaugeFill.fillAmount = 0f;

        oldTireImage.gameObject.SetActive(true);
        newTireImage.gameObject.SetActive(false);

        SetImageAlpha(oldTireImage, 1f);
        SetImageAlpha(newTireImage, 0.25f);

        foreach (BoltSlot slot in boltSlots)
        {
            slot.ResetSlot();
        }
    }


    // =========================
    // 1단계 : 기존 타이어 탈거
    // =========================

    private void StartStage1()
    {
        currentStage = RepairStage.RemoveWheel;

        remainingTime = stage1TimeLimit;

        stageText.text =
            "1단계 : 바퀴 탈거\n시계 방향으로 돌리세요";

        gaugeFill.fillAmount = 0f;
        gaugeFill.fillClockwise = true;

        boltsRoot.SetActive(false);

        oldTireImage.gameObject.SetActive(true);
        newTireImage.gameObject.SetActive(false);

        wheelRotateInput.SetRotationTarget(
            oldTireImage.rectTransform
        );

        wheelRotateInput.SetInputEnabled(true);

        UpdateTimerText();
    }


    // WheelRotateInput에서 계속 호출
    public void OnWheelDragged(float deltaAngle)
    {
        if (currentStage == RepairStage.RemoveWheel)
        {
            HandleStage1Rotation(deltaAngle);
        }
        else if (currentStage == RepairStage.AttachWheel)
        {
            HandleStage3Rotation(deltaAngle);
        }
    }


    private void HandleStage1Rotation(float deltaAngle)
    {
        // 음수 = 시계 방향
        if (deltaAngle >= 0f)
            return;

        stage1RotatedDegrees += -deltaAngle;

        float requiredDegrees =
            stage1RequiredTurns * 360f;

        float progress =
            Mathf.Clamp01(
                stage1RotatedDegrees /
                requiredDegrees
            );

        gaugeFill.fillAmount = progress;

        if (progress >= 1f)
        {
            FinishStage1(false);
        }
    }


    private void FinishStage1(bool timedOut)
    {
        wheelRotateInput.SetInputEnabled(false);

        stage1TimedOut = timedOut;

        float requiredDegrees =
            stage1RequiredTurns * 360f;

        stage1Score =
            Mathf.Clamp01(
                stage1RotatedDegrees /
                requiredDegrees
            );

        // 성공 여부와 관계없이 다음 단계 진행을 위해
        // 기존 타이어는 제거
        oldTireImage.gameObject.SetActive(false);

        gaugeFill.fillAmount = 0f;

        StartStage2();
    }


    // =========================
    // 2단계 : 나사 체결
    // =========================

    private void StartStage2()
    {
        currentStage = RepairStage.Bolts;

        remainingTime = stage2TimeLimit;

        stageText.text =
            "2단계 : 나사 체결\n모든 나사를 끼우세요";

        boltsRoot.SetActive(true);

        wheelRotateInput.SetInputEnabled(false);

        UpdateTimerText();

        // 혹시 처음부터 전부 Full인 경우
        if (GetBoltCompletion() >= 1f)
        {
            FinishStage2(false);
        }
    }


    public void NotifyBoltChanged()
    {
        if (currentStage != RepairStage.Bolts)
            return;

        if (GetBoltCompletion() >= 1f)
        {
            FinishStage2(false);
        }
    }


    private float GetBoltCompletion()
    {
        if (boltSlots.Count == 0)
            return 0f;

        int completed = 0;

        foreach (BoltSlot slot in boltSlots)
        {
            if (slot.IsCompleted)
                completed++;
        }

        return (float)completed /
               boltSlots.Count;
    }


    private void FinishStage2(bool timedOut)
    {
        stage2TimedOut = timedOut;

        stage2Score = GetBoltCompletion();

        boltsRoot.SetActive(false);

        StartStage3();
    }


    // =========================
    // 3단계 : 새 타이어 장착
    // =========================

    private void StartStage3()
    {
        currentStage = RepairStage.AttachWheel;

        remainingTime = stage3TimeLimit;

        stageText.text =
            "3단계 : 타이어 부착\n시계 반대 방향으로 돌리세요";

        gaugeFill.fillAmount = 0f;
        gaugeFill.fillClockwise = false;

        newTireImage.gameObject.SetActive(true);

        SetImageAlpha(newTireImage, 0.25f);

        wheelRotateInput.SetRotationTarget(
            newTireImage.rectTransform
        );

        wheelRotateInput.SetInputEnabled(true);

        UpdateTimerText();
    }


    private void HandleStage3Rotation(float deltaAngle)
    {
        // 양수 = 시계 반대 방향
        if (deltaAngle <= 0f)
            return;

        stage3RotatedDegrees += deltaAngle;

        float requiredDegrees =
            stage3RequiredTurns * 360f;

        float progress =
            Mathf.Clamp01(
                stage3RotatedDegrees /
                requiredDegrees
            );

        gaugeFill.fillAmount = progress;

        // 25% 투명도에서 시작해
        // 진행도에 따라 100% 불투명
        float alpha =
            Mathf.Lerp(0.25f, 1f, progress);

        SetImageAlpha(
            newTireImage,
            alpha
        );

        if (progress >= 1f)
        {
            FinishStage3(false);
        }
    }


    private void FinishStage3(bool timedOut)
    {
        wheelRotateInput.SetInputEnabled(false);

        stage3TimedOut = timedOut;

        float requiredDegrees =
            stage3RequiredTurns * 360f;

        stage3Score =
            Mathf.Clamp01(
                stage3RotatedDegrees /
                requiredDegrees
            );

        ShowResult();
    }


    // =========================
    // 시간 초과
    // =========================

    private void HandleTimeout()
    {
        switch (currentStage)
        {
            case RepairStage.RemoveWheel:
                FinishStage1(true);
                break;

            case RepairStage.Bolts:
                FinishStage2(true);
                break;

            case RepairStage.AttachWheel:
                FinishStage3(true);
                break;
        }
    }


    // =========================
    // 결과
    // =========================

    private void ShowResult()
    {
        currentStage = RepairStage.Result;

        FinalRepairRatio =
            (stage1Score +
             stage2Score +
             stage3Score) / 3f;

        bool perfect =
            !stage1TimedOut &&
            !stage2TimedOut &&
            !stage3TimedOut &&
            stage1Score >= 0.999f &&
            stage2Score >= 0.999f &&
            stage3Score >= 0.999f;

        if (perfect)
        {
            FinalRepairRatio = 1f;

            resultText.text =
                "수리 완료!\n" +
                "타이어 수리 완성도 100%";
        }
        else
        {
            resultText.text =
                "수리 종료\n" +
                $"1단계 : {stage1Score * 100f:0}%\n" +
                $"2단계 : {stage2Score * 100f:0}%\n" +
                $"3단계 : {stage3Score * 100f:0}%\n\n" +
                $"최종 완성도 : {FinalRepairRatio * 100f:0}%";
        }

        Debug.Log(
            $"RepairShop 최종 수리 완성도 = " +
            $"{FinalRepairRatio * 100f:0}%"
        );

        resultPanel.SetActive(true);
    }


    // =========================
    // 다시 하기
    // =========================

    public void Retry()
    {
        BeginMiniGame();
    }


    // =========================
    // 정비소 처음 화면으로
    // =========================

    public void BackToShop()
    {
        ShowShop();
    }


    private void UpdateTimerText()
    {
        timerText.text =
            $"남은 시간 : {remainingTime:0.0}초";
    }


    private void SetImageAlpha(
        Image image,
        float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}