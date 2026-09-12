using System.Collections.Generic;
using UnityEngine;

public class BoltPhaseManager : MonoBehaviour
{
    // =====================================
    // Inspector 설정
    // =====================================

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


    // =====================================
    // 내부 변수
    // =====================================

    // 바닥에 생성된 나사들
    private List<DraggableBolt> spawnedBolts =
        new List<DraggableBolt>();


    // 이번 판에서 플레이어가 직접 고쳐야 하는
    // 문제 볼트의 총 개수
    private int initialProblemCount = 0;


    // 현재 2단계가 진행 중인지
    private bool phaseActive = false;


    // BoltSlot에서 확인할 수 있도록 공개
    public bool IsPhaseActive => phaseActive;


    // =====================================
    // 2단계 시작
    // =====================================

    public void StartBoltPhase()
    {
        Debug.Log("2단계 나사 수리 시작!");

        phaseActive = true;


        // 이전에 남아 있던 바닥 나사 제거
        ClearLooseBolts();


        // 나사 상태 랜덤 생성
        RandomizeSlots();


        // =================================
        // 이번 판에서 실제로 고쳐야 할
        // 문제 볼트 개수 계산
        // =================================

        initialProblemCount = 0;


        foreach (BoltSlot slot in boltSlots)
        {
            // 처음부터 정상인 볼트는 계산하지 않음
            if (slot.CurrentState != BoltState.Inserted)
            {
                initialProblemCount++;
            }
        }


        Debug.Log(
            "이번에 고쳐야 할 볼트 수 : "
            + initialProblemCount
        );


        // =================================
        // 2단계 타이머 시작
        // =================================

        if (gameManager != null)
        {
            gameManager.StartStage(2);

            // 플레이어가 아직 아무것도 안 고쳤으므로
            // 시작 진행도는 0%
            gameManager.UpdateCurrentProgress(0f);
        }


        // 혹시 Inspector에서 문제 볼트 수를
        // 0개로 설정한 경우 바로 완료
        if (initialProblemCount <= 0)
        {
            CompleteBoltPhase();
        }
    }


    // =====================================
    // 각 볼트의 초기 상태 랜덤 설정
    // =====================================

    private void RandomizeSlots()
    {
        // 볼트가 하나도 없으면 종료
        if (boltSlots == null ||
            boltSlots.Count == 0)
        {
            Debug.LogWarning(
                "BoltSlot이 하나도 등록되어 있지 않습니다."
            );

            return;
        }


        // ---------------------------------
        // 랜덤 인덱스 배열 만들기
        // ---------------------------------

        List<int> randomIndexes =
            new List<int>();


        for (int i = 0;
             i < boltSlots.Count;
             i++)
        {
            randomIndexes.Add(i);
        }


        // ---------------------------------
        // Fisher-Yates Shuffle
        // ---------------------------------

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


        // ---------------------------------
        // 반쯤 끼워진 볼트 개수
        // ---------------------------------

        int halfCount =
            Mathf.Clamp(
                halfInsertedCount,
                0,
                boltSlots.Count
            );


        // ---------------------------------
        // 빈 구멍 개수
        // ---------------------------------

        int empty =
            Mathf.Clamp(
                emptyCount,
                0,
                boltSlots.Count - halfCount
            );


        // =================================
        // 일단 모든 볼트를 정상 상태로 설정
        // =================================

        foreach (BoltSlot slot in boltSlots)
        {
            slot.Setup(
                BoltState.Inserted,
                this
            );
        }


        // =================================
        // 일부를 HalfInserted로 변경
        // =================================

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


        // =================================
        // 일부를 Empty로 변경
        // =================================

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


        // =================================
        // 빈 구멍 수만큼
        // 바닥 나사 생성
        // =================================

        SpawnLooseBolts(empty);
    }


    // =====================================
    // 바닥에 드래그 가능한 나사 생성
    // =====================================

    private void SpawnLooseBolts(int count)
    {
        if (looseBoltPrefab == null)
        {
            Debug.LogWarning(
                "Loose Bolt Prefab이 연결되어 있지 않습니다."
            );

            return;
        }


        if (floorRail == null)
        {
            Debug.LogWarning(
                "Floor Rail이 연결되어 있지 않습니다."
            );

            return;
        }


        for (int i = 0;
             i < count;
             i++)
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


            if (boltRect != null)
            {
                boltRect.anchoredPosition =
                    new Vector2(x, y);
            }


            spawnedBolts.Add(bolt);
        }
    }


    // =====================================
    // 기존 바닥 나사 제거
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
    // 플레이어가 볼트 하나를 고칠 때마다 호출
    //
    // BoltSlot에서 호출함
    // =====================================

    public void NotifyBoltChanged()
    {
        // 이미 2단계가 끝난 상태면 무시
        if (!phaseActive)
        {
            return;
        }


        // =================================
        // 아직 고쳐지지 않은 문제 볼트 개수
        // =================================

        int remainingProblems = 0;


        foreach (BoltSlot slot in boltSlots)
        {
            if (slot.CurrentState != BoltState.Inserted)
            {
                remainingProblems++;
            }
        }


        // =================================
        // 플레이어가 직접 고친 볼트 개수
        // =================================

        int repairedCount =
            initialProblemCount -
            remainingProblems;


        repairedCount =
            Mathf.Clamp(
                repairedCount,
                0,
                initialProblemCount
            );


        // =================================
        // 2단계 진행도 계산
        //
        // 직접 고친 볼트 /
        // 처음 고쳐야 했던 볼트
        // =================================

        float progress = 0f;


        if (initialProblemCount > 0)
        {
            progress =
                (float)repairedCount /
                initialProblemCount;
        }
        else
        {
            progress = 1f;
        }


        progress =
            Mathf.Clamp01(progress);


        // =================================
        // RepairShopGameManager에
        // 현재 진행도 전달
        // =================================

        if (gameManager != null)
        {
            gameManager.UpdateCurrentProgress(
                progress
            );
        }


        Debug.Log(
            "2단계 진행도 : "
            + repairedCount
            + " / "
            + initialProblemCount
            + " = "
            + (progress * 100f)
                .ToString("F0")
            + "%"
        );


        // =================================
        // 문제 볼트를 전부 고쳤으면 완료
        // =================================

        if (remainingProblems <= 0)
        {
            CompleteBoltPhase();
        }
    }


    // =====================================
    // 2단계 정상 완료
    // =====================================

    private void CompleteBoltPhase()
    {
        if (!phaseActive)
        {
            return;
        }


        phaseActive = false;


        Debug.Log(
            "모든 문제 볼트를 정상적으로 고쳤습니다!"
        );


        // 혹시 남은 바닥 나사가 있다면 제거
        ClearLooseBolts();


        // =================================
        // 진행도 100% 확정
        // =================================

        if (gameManager != null)
        {
            gameManager.UpdateCurrentProgress(
                1f
            );


            // 2단계 완성도 100% 저장
            gameManager.CompleteStage(
                2,
                1f
            );
        }


        // =================================
        // 3단계 시작
        // =================================

        if (tireInstallMiniGame != null)
        {
            tireInstallMiniGame
                .StartTireInstallPhase();
        }
        else
        {
            Debug.LogWarning(
                "TireInstallMiniGame이 연결되어 있지 않습니다."
            );
        }
    }


    // =====================================
    // 2단계 제한시간 초과
    //
    // RepairShopGameManager가 호출함
    // =====================================

    public void ForceFinishByTimeout()
    {
        if (!phaseActive)
        {
            return;
        }


        phaseActive = false;


        Debug.Log(
            "2단계 제한시간 종료!"
        );


        // ---------------------------------
        // 여기서는 CompleteStage를
        // 호출하지 않음!
        //
        // 시간 초과 순간의 진행도 저장은
        // RepairShopGameManager가 담당함.
        // ---------------------------------


        // 남아 있는 바닥 나사 제거
        ClearLooseBolts();


        // =================================
        // 3단계로 이동
        // =================================

        if (tireInstallMiniGame != null)
        {
            tireInstallMiniGame
                .StartTireInstallPhase();
        }
        else
        {
            Debug.LogWarning(
                "TireInstallMiniGame이 연결되어 있지 않습니다."
            );
        }
    }
}