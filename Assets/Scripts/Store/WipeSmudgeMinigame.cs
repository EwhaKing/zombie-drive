using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WipeSmudgeMinigame : MonoBehaviour
{
    public FridgeMiniGameManager gameManager; // 게임 전체 상태 및 인벤토리를 관리하는 매니저
    public GameObject smudgePrefab;            // 생성할 얼룩 프리팹
    public RectTransform spawnArea;            // 얼룩과 아이템이 스폰될 UI 영역
    public float wipeThreshold = 1000f;        // 얼룩이 완전히 지워지기 위해 필요한 총 마우스 이동량
    public List<ItemData> possibleFoods;       // 얼룩 밑에서 스폰될 수 있는 아이템 리스트

    [Header("아이템 크기 설정")]
    public Vector2 itemSize = new Vector2(130f, 130f); // 스폰되는 아이템의 UI 크기

    [Header("얼룩 배치 설정")]
    public float minDistanceBetweenSmudges = 150f;     // 얼룩 간의 최소 간격 (겹침 방지용)

    [Header("아이템 생성 확률 설정")]
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.3f; // [핵심] 얼룩 밑에 아이템이 생성될 확률 (0.3f = 30%)

    private List<GameObject> activeSmudges = new List<GameObject>();            // 현재 생성된 얼룩 리스트
    private Dictionary<GameObject, float> wipeProgress = new Dictionary<GameObject, float>(); // 얼룩별 닦은 진척도 저장
    private bool isInitialized = false;                                          // 초기 스폰 수행 여부 플래그

    void OnEnable()
    {
        // 오브젝트가 활성화될 때 한 번만 얼룩을 스폰 (재진입 시 리셋 방지)
        if (isInitialized) return;

        ResetAndSpawnSmudges();
        isInitialized = true;
    }

    // 얼룩 및 아이템을 생성하고 배치하는 핵심 메서드
    public void ResetAndSpawnSmudges()
    {
        // 기존 얼룩 오브젝트 및 데이터 초기화
        foreach (var s in activeSmudges) if (s != null) Destroy(s);
        activeSmudges.Clear();
        wipeProgress.Clear();

        if (smudgePrefab == null || spawnArea == null) return;

        Rect areaRect = spawnArea.rect;
        int count = Random.Range(15, 21); // 얼룩 생성 개수 (15개 ~ 20개)

        List<Vector2> spawnedPositions = new List<Vector2>(); // 생성된 얼룩 위치 기록용 리스트

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = Vector2.zero;
            bool validPositionFound = false;

            // 얼룩끼리 너무 겹치지 않도록 최대 30번 위치를 재선정
            for (int attempt = 0; attempt < 30; attempt++)
            {
                Vector2 tempPos = new Vector2(
                    Random.Range(areaRect.xMin + 60f, areaRect.xMax - 60f),
                    Random.Range(areaRect.yMin + 60f, areaRect.yMax - 60f)
                );

                bool isTooClose = false;
                foreach (Vector2 existingPos in spawnedPositions)
                {
                    // 기존에 배치된 얼룩과의 거리가 지정한 최소 간격보다 작은지 검사
                    if (Vector2.Distance(tempPos, existingPos) < minDistanceBetweenSmudges)
                    {
                        isTooClose = true;
                        break;
                    }
                }

                if (!isTooClose)
                {
                    spawnPos = tempPos;
                    validPositionFound = true;
                    break;
                }
            }

            // 30번 시도 후에도 적절한 위치를 못 찾으면 임의 위치 지정
            if (!validPositionFound)
            {
                spawnPos = new Vector2(
                    Random.Range(areaRect.xMin + 60f, areaRect.xMax - 60f),
                    Random.Range(areaRect.yMin + 60f, areaRect.yMax - 60f)
                );
            }

            spawnedPositions.Add(spawnPos);

            // =========================================================================
            // [아이템 생성 확률 처리 부분]
            // Random.value가 itemSpawnChance(예: 0.3)보다 작을 때만 아이템 할당 (30% 확률)
            // =========================================================================
            ItemData assignedFood = (Random.value < itemSpawnChance) ? gameManager.GetRandomItem(possibleFoods) : null;

            // 1. 아이템 할당된 경우, 얼룩보다 먼저 생성하여 레이어상 얼룩 아래에 깔리도록 함
            if (assignedFood != null)
            {
                SpawnItemUnderSmudge(assignedFood, spawnPos);
            }

            // 2. 아이템 위에 덮어씌울 얼룩 생성
            GameObject smudge = Instantiate(smudgePrefab, spawnArea);
            smudge.GetComponent<RectTransform>().anchoredPosition = spawnPos;
            smudge.SetActive(true);

            activeSmudges.Add(smudge);
            wipeProgress[smudge] = 0f;
        }
    }

    // 얼룩 밑에 클릭 가능한 아이템 오브젝트 생성
    void SpawnItemUnderSmudge(ItemData item, Vector2 pos)
    {
        GameObject foodObj = new GameObject("HiddenItemUnderSmudge", typeof(RectTransform), typeof(Image), typeof(Button));
        foodObj.transform.SetParent(spawnArea, false);

        Image img = foodObj.GetComponent<Image>();
        img.sprite = item.icon;
        img.preserveAspect = true;

        RectTransform rect = foodObj.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = itemSize;

        // 아이템 클릭 시 인벤토리에 추가하고 필드에서 제거
        Button btn = foodObj.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            gameManager.AddToInventory(item);
            Destroy(foodObj);
        });
    }

    // 마우스 드래그를 감지하여 얼룩을 지우는 로직
    void Update()
    {
        if (!Input.GetMouseButton(0)) return;

        // 마우스 커서 위치에 있는 UI 요소들을 레이캐스트로 탐색
        PointerEventData ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        if (results.Count == 0) return;

        GameObject hit = results[0].gameObject;

        foreach (var smudge in activeSmudges)
        {
            if (smudge == null || !smudge.activeSelf) continue;
            if (hit != smudge && !hit.transform.IsChildOf(smudge.transform)) continue;

            // 마우스 이동량 계산 및 닦기 진척도 누적
            float mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).magnitude * 100f;
            wipeProgress[smudge] += mouseDelta;

            // 진척도에 따라 얼룩의 알파(투명도) 값을 줄여 서서히 투명해지게 함
            Image img = smudge.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 1f - Mathf.Clamp01(wipeProgress[smudge] / wipeThreshold);
                img.color = c;
            }

            // 목표 수치 이상 닦으면 얼룩 비활성화 (아래에 있던 아이템이 드러남)
            if (wipeProgress[smudge] >= wipeThreshold)
            {
                smudge.SetActive(false);
            }
            break;
        }
    }
}