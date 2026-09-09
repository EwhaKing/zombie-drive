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

    private List<DraggableBolt> spawnedBolts
        = new List<DraggableBolt>();

    // =====================================
    // 나사 단계 시작
    // =====================================
    public void StartBoltPhase()
    {
        ClearLooseBolts();

        RandomizeSlots();
    }

    // =====================================
    // 구멍 상태 랜덤 결정
    // =====================================
    private void RandomizeSlots()
    {
        List<int> randomIndexes = new List<int>();

        for (int i = 0; i < boltSlots.Count; i++)
        {
            randomIndexes.Add(i);
        }

        // Fisher-Yates Shuffle
        for (int i = 0; i < randomIndexes.Count; i++)
        {
            int randomIndex =
                Random.Range(i, randomIndexes.Count);

            int temp = randomIndexes[i];

            randomIndexes[i] =
                randomIndexes[randomIndex];

            randomIndexes[randomIndex] = temp;
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

        // 일단 전부 정상 나사로 설정
        foreach (BoltSlot slot in boltSlots)
        {
            slot.Setup(
                BoltState.Inserted,
                this
            );
        }

        // -------------------------
        // 반쯤 나온 나사
        // -------------------------
        for (int i = 0; i < halfCount; i++)
        {
            int index = randomIndexes[i];

            boltSlots[index].Setup(
                BoltState.HalfInserted,
                this
            );
        }

        // -------------------------
        // 빈 구멍
        // -------------------------
        for (
            int i = halfCount;
            i < halfCount + empty;
            i++
        )
        {
            int index = randomIndexes[i];

            boltSlots[index].Setup(
                BoltState.Empty,
                this
            );
        }

        // 빈 구멍 개수만큼 바닥 나사 생성
        SpawnLooseBolts(empty);
    }

    // =====================================
    // 바닥 나사 생성
    // =====================================
    private void SpawnLooseBolts(int count)
    {
        for (int i = 0; i < count; i++)
        {
            DraggableBolt bolt =
                Instantiate(
                    looseBoltPrefab,
                    floorRail
                );

            RectTransform boltRect =
                bolt.GetComponent<RectTransform>();

            Rect railRect = floorRail.rect;

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
    // 기존 바닥 나사 제거
    // =====================================
    private void ClearLooseBolts()
    {
        foreach (DraggableBolt bolt in spawnedBolts)
        {
            if (bolt != null)
            {
                Destroy(bolt.gameObject);
            }
        }

        spawnedBolts.Clear();
    }

    // =====================================
    // 나사가 변경될 때마다 호출
    // =====================================
    public void NotifyBoltChanged()
    {
        foreach (BoltSlot slot in boltSlots)
        {
            if (slot.CurrentState
                != BoltState.Inserted)
            {
                return;
            }
        }

        CompleteBoltPhase();
    }

    // =====================================
    // 모든 나사 정상적으로 끼움
    // =====================================
    private void CompleteBoltPhase()
    {
        Debug.Log("모든 나사를 정상적으로 끼웠습니다!");

        // TODO:
        // 다음 정비 단계 시작
    }
}