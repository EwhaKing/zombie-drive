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
    public float itemSpawnChance = 0.3f; // 얼룩 밑에 아이템이 생성될 확률 (0.3f = 30%)

    private List<GameObject> activeSmudges = new List<GameObject>(); 
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

        List<Vector2> spawnedPositions = new List<Vector2>();

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = Vector2.zero;
            bool validPositionFound = false;

            for (int attempt = 0; attempt < 30; attempt++)
            {
                Vector2 tempPos = new Vector2(
                    Random.Range(areaRect.xMin + 60f, areaRect.xMax - 60f),
                    Random.Range(areaRect.yMin + 60f, areaRect.yMax - 60f)
                );

                bool isTooClose = false;
                foreach (Vector2 existingPos in spawnedPositions)
                {
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

            if (!validPositionFound)
            {
                spawnPos = new Vector2(
                    Random.Range(areaRect.xMin + 60f, areaRect.xMax - 60f),
                    Random.Range(areaRect.yMin + 60f, areaRect.yMax - 60f)
                );
            }

            spawnedPositions.Add(spawnPos);

            ItemData assignedFood = (Random.value < itemSpawnChance) ? gameManager.GetRandomItem(possibleFoods) : null;

            if (assignedFood != null)
            {
                SpawnItemUnderSmudge(assignedFood, spawnPos);
            }

            GameObject smudge = Instantiate(smudgePrefab, spawnArea);
            smudge.GetComponent<RectTransform>().anchoredPosition = spawnPos;
            smudge.SetActive(true);

            activeSmudges.Add(smudge);
            wipeProgress[smudge] = 0f;
        }
    }

    void SpawnItemUnderSmudge(ItemData item, Vector2 pos)
    {
        GameObject foodObj = new GameObject("HiddenItemUnderSmudge", typeof(RectTransform), typeof(Image), typeof(Button));
        foodObj.transform.SetParent(spawnArea, false);

        Image img = foodObj.GetComponent<Image>();
        img.sprite = item.icon;
        img.preserveAspect = true;

        RectTransform rect = foodObj.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;

        if (item.icon != null)
        {
            float spriteWidth = item.icon.rect.width;
            float spriteHeight = item.icon.rect.height;
            float targetSize = 130f;

            if (spriteWidth > spriteHeight)
            {
                rect.sizeDelta = new Vector2(targetSize, targetSize * (spriteHeight / spriteWidth));
            }
            else
            {
                rect.sizeDelta = new Vector2(targetSize * (spriteWidth / spriteHeight), targetSize);
            }
        }
        else
        {
            rect.sizeDelta = itemSize;
        }

        float scale = (item != null) ? item.itemScale : 1.0f;
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