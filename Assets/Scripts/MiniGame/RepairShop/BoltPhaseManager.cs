using System.Collections.Generic;
using UnityEngine;

public class BoltPhaseManager : MonoBehaviour
{
    [Header("림의 나사 구멍")]
    [SerializeField] private List<BoltSlot> boltSlots;

    [Header("랜덤 상태 개수")]
    [SerializeField] private int halfInsertedCount = 2;
    [SerializeField] private int emptyCount = 2;

    [Header("바닥 나사")]
    [SerializeField] private DraggableBolt looseBoltPrefab;
    [SerializeField] private RectTransform floorRail;

    [Header("3단계 타이어 부착")]
    [SerializeField] private TireInstallMiniGame tireInstallMiniGame;

    [Header("전체 정비 관리자")]
    [SerializeField] private RepairShopGameManager gameManager;


    private List<DraggableBolt> spawnedBolts =
        new List<DraggableBolt>();


    private int initialProblemCount = 0;

    private bool phaseActive = false;


    public bool IsPhaseActive => phaseActive;


    // =====================================
    // 나사 단계 시작
    // =====================================

    public void StartBoltPhase()
    {
        phaseActive = true;

        ClearLooseBolts();

        RandomizeSlots();


        // 처음 고쳐야 할 나사 개수 계산
        initialProblemCount = 0;

        foreach (BoltSlot slot in boltSlots)
        {
            if (slot.CurrentState
                != BoltState.Inserted)
            {
                initialProblemCount++;
            }
        }


        // 2단계 제한시간 시작
        if (gameManager != null)
        {
            gameManager.StartStage(2);

            gameManager.UpdateCurrentProgress(
                0f
            );
        }
    }


    // =====================================
    // 나사 상태 랜덤 결정
    // =====================================

    private void RandomizeSlots()
    {
        List<int> randomIndexes =
            new List<int>();


        for (int i = 0;
             i < boltSlots.Count;
             i++)
        {
            randomIndexes.Add(i);
        }


        // Fisher-Yates Shuffle
        for (int i = 0;
             i < randomIndexes.Count;
             i++)
        {
            int randomIndex =
                Random.Range(
                    i,
                    randomIndexes.Count
                );


            int temp =
                randomIndexes[i];


            randomIndexes[i] =
                randomIndexes[randomIndex];


            randomIndexes[randomIndex] =
                temp;
        }


        int halfCount =
            Mathf.Clamp(
                halfInsertedCount,
                0,
                boltSlots.Count
            );


        int empty =
            Mathf.Clamp(
                emptyCount,
                0,
                boltSlots.Count - halfCount
            );


        // 일단 전부 정상
        foreach (BoltSlot slot in boltSlots)
        {
            slot.Setup(
                BoltState.Inserted,
                this
            );
        }


        // 반쯤 나온 나사
        for (int i = 0;
             i < halfCount;
             i++)
        {
            int index =
                randomIndexes[i];


            boltSlots[index].Setup(
                BoltState.HalfInserted,
                this
            );
        }


        // 빈 구멍
        for (int i = halfCount;
             i < halfCount + empty;
             i++)
        {
            int index =
                randomIndexes[i];


            boltSlots[index].Setup(
                BoltState.Empty,
                this
            );
        }


        SpawnLooseBolts(empty);
    }


    // =====================================
    // 바닥 나사 생성
    // =====================================

    private void SpawnLooseBolts(int count)
    {
        if (looseBoltPrefab == null ||
            floorRail == null)
        {
            return;
        }


        for (int i = 0; i < count; i++)
        {
            DraggableBolt bolt =
                Instantiate(
                    looseBoltPrefab,
                    floorRail
                );


            RectTransform boltRect =
                bolt.GetComponent<RectTransform>();


            Rect railRect =
                floorRail.rect;


            float x =
                Random.Range(
                    railRect.xMin + 50f,
                    railRect.xMax - 50f
                );


            float y =
                Random.Range(
                    railRect.yMin + 30f,
                    railRect.yMax - 30f
                );


            boltRect.anchoredPosition =
                new Vector2(x, y);


            spawnedBolts.Add(bolt);
        }
    }


    // =====================================
    // 바닥 나사 제거
    // =====================================

    private void ClearLooseBolts()
    {
        foreach (DraggableBolt bolt
                 in spawnedBolts)
        {
            if (bolt != null)
            {
                Destroy(
                    bolt.gameObject
                );
            }
        }


        spawnedBolts.Clear();
    }


    // =====================================
    // 나사 변경될 때 호출
    // =====================================

    public void NotifyBoltChanged()
    {
        if (!phaseActive)
            return;


        int remainingProblems = 0;


        foreach (BoltSlot slot in boltSlots)
        {
            if (slot.CurrentState
                != BoltState.Inserted)
            {
                remainingProblems++;
            }
        }


        float progress;


        if (initialProblemCount <= 0)
        {
            progress = 1f;
        }
        else
        {
            progress =
                1f -
                (
                    (float)remainingProblems /
                    initialProblemCount
                );
        }


        // 현재 나사 진행도 전달
        if (gameManager != null)
        {
            gameManager.UpdateCurrentProgress(
                progress
            );
        }


        // 아직 안 고친 나사가 남아있음
        if (remainingProblems > 0)
            return;


        CompleteBoltPhase();
    }


    // =====================================
    // 모든 나사 정상 완료
    // =====================================

    private void CompleteBoltPhase()
    {
        if (!phaseActive)
            return;


        phaseActive = false;


        Debug.Log(
            "모든 나사를 정상적으로 끼웠습니다!"
        );


        // 2단계 100%
        if (gameManager != null)
        {
            gameManager.CompleteStage(
                2,
                1f
            );
        }


        // 3단계 시작
        if (tireInstallMiniGame != null)
        {
            tireInstallMiniGame
                .StartTireInstallPhase();
        }
    }


    // =====================================
    // 2단계 시간 초과
    // =====================================

    public void ForceFinishByTimeout()
    {
        if (!phaseActive)
            return;


        phaseActive = false;


        Debug.Log(
            "2단계 제한시간 종료!"
        );


        // 그대로 3단계 진행
        if (tireInstallMiniGame != null)
        {
            tireInstallMiniGame
                .StartTireInstallPhase();
        }
    }
}