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
    // [추가] 하드코딩돼 있던 130f를 인스펙터에서 조절 가능하게 변경
    public float itemTargetSize = 130f;

    [Header("얼룩 배치 설정")]
    public float minDistanceBetweenSmudges = 150f;     // 얼룩 간의 최소 간격 (겹침 방지용)

    [Header("아이템 생성 확률 설정")]
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.3f; // [변경] 이제 '칸 안의 각 셀'마다 적용되는 확률

    // [추가] 냉장고 칸(선반) 설정
    [Header("냉장고 칸(선반) 설정")]
    // (아래쪽 비율, 위쪽 비율): spawnArea 높이 기준 0(맨 아래)~1(맨 위).
    // 냉장고 이미지의 선반 위치에 맞춰 인스펙터에서 조절하세요.
    public Vector2[] shelfBands = new Vector2[]
    {
        new Vector2(0.05f, 0.30f), // 맨 아래 칸
        new Vector2(0.38f, 0.63f), // 가운데 칸
        new Vector2(0.70f, 0.95f)  // 맨 위 칸
    };
    // [추가] 한 칸을 가로로 몇 개 셀로 나눌지. 셀 하나에 아이템 최대 1개 → 겹침 방지
    public int slotsPerShelf = 4;
    // [추가] 좌우 여백
    public float sidePadding = 60f;

    private List<GameObject> activeSmudges = new List<GameObject>();
    // [추가] 생성된 아이템도 추적해서 리셋 시 함께 삭제
    private List<GameObject> spawnedItems = new List<GameObject>();
    private Dictionary<GameObject, float> wipeProgress = new Dictionary<GameObject, float>();
    private bool isInitialized = false;

    // 드래그 입력 처리를 위한 변수
    private Vector2 lastMousePosition;
    private bool isDragging = false;


    void OnEnable()
    {
        // 오브젝트가 활성화될 때 한 번만 얼룩을 스폰 (재진입 시 리셋 방지)
        if (isInitialized) return;

        ResetAndSpawnSmudges();
        isInitialized = true;
    }

    // 얼룩 및 아이템을 생성하고 배치
    public void ResetAndSpawnSmudges()
    {
        foreach (var s in activeSmudges) if (s != null) Destroy(s);
        activeSmudges.Clear();
        wipeProgress.Clear();

        // [추가] 이전에 생성한 아이템도 정리
        foreach (var it in spawnedItems) if (it != null) Destroy(it);
        spawnedItems.Clear();

        if (smudgePrefab == null || spawnArea == null) return;

        Rect areaRect = spawnArea.rect;
        Vector2 center = areaRect.center; // [추가] anchoredPosition은 영역 '중심' 기준이므로 변환용

        List<Vector2> spawnedPositions = new List<Vector2>(); // 얼룩 간격 체크용
        List<Vector2> itemPositions = new List<Vector2>();    // 아이템 위치 (얼룩을 덮어줄 용도)

        // ===== [변경] 1단계: 칸(선반)별 아이템 생성 =====
        // 아이템을 얼룩보다 '먼저' 만들어야 모든 얼룩이 아이템 위에 그려지고(가려지고)
        // 얼룩을 지우기 전에는 클릭도 안 됨 (기존엔 나중에 생긴 아이템이 얼룩 위로 올라오는 버그가 있었음)
        float cellW = (areaRect.width - sidePadding * 2f) / Mathf.Max(1, slotsPerShelf);

        foreach (Vector2 band in shelfBands)
        {
            float bandMinY = areaRect.yMin + areaRect.height * Mathf.Min(band.x, band.y);
            float bandMaxY = areaRect.yMin + areaRect.height * Mathf.Max(band.x, band.y);

            for (int c = 0; c < slotsPerShelf; c++)
            {
                if (Random.value >= itemSpawnChance) continue;

                ItemData food = gameManager.GetRandomItem(possibleFoods);
                if (food == null) continue;

                float size = itemTargetSize * food.itemScale; // 실제 화면상 크기(스케일 포함)

                // 셀 안에서만 좌우 흔들림 → 옆 셀 아이템과 절대 겹치지 않음
                float cellCenterX = areaRect.xMin + sidePadding + cellW * (c + 0.5f);
                float jitterX = Mathf.Max(0f, (cellW - size) * 0.5f);
                float x = cellCenterX + Random.Range(-jitterX, jitterX);

                // 칸(선반) 띠 안에서만 상하 위치 결정
                float yMin = bandMinY + size * 0.5f;
                float yMax = bandMaxY - size * 0.5f;
                float y = (yMin < yMax) ? Random.Range(yMin, yMax) : (bandMinY + bandMaxY) * 0.5f;

                Vector2 pos = new Vector2(x, y) - center; // 중심 기준 좌표로 변환
                SpawnItemUnderSmudge(food, pos);
                itemPositions.Add(pos);
            }
        }

        // ===== [변경] 2단계: 아이템 위에 얼룩 덮기 =====
        foreach (Vector2 pos in itemPositions)
        {
            CreateSmudge(pos);
            spawnedPositions.Add(pos);
        }

        // ===== 3단계: 남은 개수만큼 랜덤 얼룩 (기존 로직, 칸 제한 없음) =====
        int totalCount = Random.Range(15, 21);
        while (activeSmudges.Count < totalCount)
        {
            Vector2 spawnPos = Vector2.zero;
            bool found = false;

            for (int attempt = 0; attempt < 30; attempt++)
            {
                Vector2 temp = new Vector2(
                    Random.Range(areaRect.xMin + 60f, areaRect.xMax - 60f),
                    Random.Range(areaRect.yMin + 60f, areaRect.yMax - 60f)) - center;

                bool tooClose = false;
                foreach (Vector2 p in spawnedPositions)
                {
                    if (Vector2.Distance(temp, p) < minDistanceBetweenSmudges) { tooClose = true; break; }
                }
                if (!tooClose) { spawnPos = temp; found = true; break; }
            }

            if (!found)
            {
                spawnPos = new Vector2(
                    Random.Range(areaRect.xMin + 60f, areaRect.xMax - 60f),
                    Random.Range(areaRect.yMin + 60f, areaRect.yMax - 60f)) - center;
            }

            spawnedPositions.Add(spawnPos);
            CreateSmudge(spawnPos);
        }
    }

    // [추가] 얼룩 생성 코드를 함수로 분리 (아이템용/랜덤용 공통 사용)
    void CreateSmudge(Vector2 anchoredPos)
    {
        GameObject smudge = Instantiate(smudgePrefab, spawnArea);
        smudge.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
        smudge.SetActive(true);

        activeSmudges.Add(smudge);
        wipeProgress[smudge] = 0f;
    }

    void SpawnItemUnderSmudge(ItemData item, Vector2 pos)
    {
        GameObject foodObj = new GameObject("HiddenItemUnderSmudge", typeof(RectTransform), typeof(Image), typeof(Button));
        foodObj.transform.SetParent(spawnArea, false);
        spawnedItems.Add(foodObj); // [추가] 리셋 시 삭제할 수 있도록 등록

        Image img = foodObj.GetComponent<Image>();
        img.sprite = item.icon;
        img.preserveAspect = true;

        RectTransform rect = foodObj.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;

        if (item.icon != null)
        {
            float w = item.icon.rect.width;
            float h = item.icon.rect.height;
            float targetSize = itemTargetSize; // [변경] 130f 하드코딩 → 변수

            if (w > h) rect.sizeDelta = new Vector2(targetSize, targetSize * (h / w));
            else       rect.sizeDelta = new Vector2(targetSize * (w / h), targetSize);
        }
        else
        {
            rect.sizeDelta = itemSize;
        }

        float scale = item.itemScale;
        rect.localScale = new Vector3(scale, scale, 1.0f);

        Button btn = foodObj.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            gameManager.AddToInventory(item);
            Destroy(foodObj);
        });
    }

    void Update()
    {
        // 1. 드래그 시작 시점 처리
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
            return;
        }

        // 2. 드래그 종료 처리
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            return;
        }

        if (!isDragging || !Input.GetMouseButton(0)) return;

        Vector2 currentPos = Input.mousePosition;

        // 3. 레이캐스트 실행
        PointerEventData ped = new PointerEventData(EventSystem.current) { position = currentPos };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        if (results.Count == 0) return;

        // 4. 감지된 UI 중 얼룩 검사
        foreach (var result in results)
        {
            GameObject hit = result.gameObject;

            foreach (var smudge in activeSmudges)
            {
                if (smudge == null || !smudge.activeSelf) continue;
                if (hit != smudge && !hit.transform.IsChildOf(smudge.transform)) continue;

                // 마우스 이동 거리 계산 (Delta)
                float mouseDelta = Vector2.Distance(currentPos, lastMousePosition);
                wipeProgress[smudge] += mouseDelta;

                // 알파 값 조절
                Image img = smudge.GetComponent<Image>();
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 1f - Mathf.Clamp01(wipeProgress[smudge] / wipeThreshold);
                    img.color = c;
                }

                // 목표치 도달 시 얼룩 비활성화
                if (wipeProgress[smudge] >= wipeThreshold)
                {
                    smudge.SetActive(false);
                }

                lastMousePosition = currentPos;
                return;
            }
        }

        lastMousePosition = currentPos;
    }
}