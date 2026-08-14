using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; 

/// <summary>
///편의점 미니게임 스크립트
/// GameManager에 연결
/// 
/// 
/// 얼룩(Smudge)은 "누르고 있는 상태로 그 위에서 마우스를 움직인 누적 거리"가
/// wipeThreshold를 넘으면 지워진다.
///
/// 
/// - 드래그 진행 상태(isDragging, dragRectTransform)를 매니저 전역이 아니라
///   스팟(InteractableSpot)마다 따로 갖도록 변경 (여러 드래그 스팟을 동시에 다룰 수 있게).
/// - 기존 HandleDrag(mouseDelta)를 HandleDragForSpot(spot, mouseDelta)로 일반화해서
///   클로즈업 안에서 쓰던 것과 오버뷰에서 바로 쓰는 것 모두 같은 함수로 처리.
/// </summary>
/// 
/// 
public class FridgeMiniGameManager : MonoBehaviour
{
    public enum GameState { Overview, Closeup, Minigame } //게임 현재 화면상태(전체, 확대, 미니게임)
    public enum MinigameType { None, WipeSmudge, DragPile }//미니게임(x, 얼룩, 더미)

    [System.Serializable]
    public class FoodItem //게임 성공 시 얻을 수 있는 아이템
    {
        public string itemName;
        public Sprite icon;
        [Range(0.01f, 100f)] public float weight = 10f; //확률 계산을 위한 가중치
    }

    [System.Serializable]
    public class InteractableSpot
    {
        [Header("기본 정보")]
        public string spotName; //이름(냉장고or더미)
        public GameObject worldObject;     // Spot_Fridge
        public GameObject closeupView;     // Closeup_Fridge/X
        public MinigameType minigameType; //미니게임타입

        [Header("오버뷰에서 바로 진행 (DragPile 전용)")]
        [Tooltip("체크하면 클릭으로 화면을 전환하지 않고, 오버뷰 화면에 놓인 dragTarget을 바로 드래그해서 치울 수 있습니다.")]
        public bool playDirectlyInOverview = false;

        [Header("얼룩 닦기용 (WipeSmudge)")]
        public GameObject smudgePrefab;         // 프로젝트 창에서 만든 얼룩 프리팹
        public RectTransform spawnArea;         // 얼룩이 생성될 냉장고 안 UI 영역

        [HideInInspector] public List<GameObject> activeSmudgeList = new List<GameObject>(); // 동적 생성된 얼룩 관리용
        [HideInInspector] public List<GameObject> spawnedFoodObjects = new List<GameObject>(); // [신규] 바닥에 생성된 아이템 오브젝트 목록


        [Header("더미 치우기용 (DragPile)")]
        public GameObject dragTarget;      // Spot_Pile
        public GameObject hiddenReveal;    //HiddenFood
        public float dragClearDistance = 150f; //드래그하는 거리

        [Header("여기서 얻을 수 있는 식량")]
        public List<FoodItem> possibleFoods;

        [HideInInspector] public bool isCleared = false;//중복여부확인
        [HideInInspector] public int wipedCount;
        [HideInInspector] public Vector2 dragStartAnchoredPos;
        [HideInInspector] public Vector2 revealAnchoredPos;

        // 스팟별 드래그 진행 상태 
        [HideInInspector] public bool isDragging;
        [HideInInspector] public RectTransform dragRectTransform;
    }

    [Header("현재 상태 (읽기 전용)")]
    public GameState currentState = GameState.Overview;

    [Header("Canvas 참조 (드래그 스케일 보정용)")]
    public Canvas mainCanvas;

    [Header("전체 화면")]
    public GameObject overviewRoot;

    [Header("상호작용 지점 목록")]
    public List<InteractableSpot> spots = new List<InteractableSpot>();

    [Header("공용 UI")]
    public TextMeshProUGUI inventoryText;
    public GameObject closeButton; // 이건 그대로 Button + OnClick(ExitCloseup) 유지

    [Header("얼룩 닦기 민감도")]
    [Tooltip("마우스를 누른 채로 얼룩 위에서 움직여야 하는 누적 픽셀 거리. 값이 클수록 오래 문질러야 함")]
    public float wipeThreshold = 1000f; //인스펙터에서 조절가능

    private InteractableSpot currentSpot;
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    private Vector2 prevMousePos;
    private Dictionary<GameObject, float> wipeProgress = new Dictionary<GameObject, float>();

    private bool isMinigameCleared = false;//냉장고 미니게임 성공 여부 체크, 클리어 중복 실행 방지
    
    // [신규] 얼룩 오브젝트별로 어떤 FoodItem을 가지고 있는지 1:1 매칭하는 딕셔너리 (null이면 꽝)
    private Dictionary<GameObject, FoodItem> smudgeFoodMap = new Dictionary<GameObject, FoodItem>();

    void Start()
    {
        foreach (var spot in spots)
        {
            //모든 확대 화면 숨김
            if (spot.closeupView != null) spot.closeupView.SetActive(false);

            if (spot.minigameType == MinigameType.DragPile)
            {
                //드래그 스팟의 초기위치 저장
                if (spot.dragTarget != null)
                {
                    spot.dragRectTransform = spot.dragTarget.GetComponent<RectTransform>();
                    spot.dragStartAnchoredPos = spot.dragRectTransform.anchoredPosition;
                }
                if (spot.hiddenReveal != null)
                    spot.revealAnchoredPos = spot.hiddenReveal.GetComponent<RectTransform>().anchoredPosition;

                //확대없이 전체 화면에서 바로 치움
                if (spot.playDirectlyInOverview)
                {
                    // 오버뷰 화면에 바로 배치되어 있으므로 시작하자마자 활성화해둠
                    if (spot.dragTarget != null) spot.dragTarget.SetActive(true);
                    if (spot.hiddenReveal != null) spot.hiddenReveal.SetActive(false);
                }
            }
        }
        //기본 UI세팅(전체 화면 활성화, 마우스 위치기록)
        if (closeButton != null) closeButton.SetActive(false);
        if (overviewRoot != null) overviewRoot.SetActive(true);

        currentState = GameState.Overview;
        prevMousePos = Input.mousePosition;
        UpdateInventoryUI();
    }

    void Update()
    {
        Vector2 curMousePos = Input.mousePosition;
        Vector2 mouseDelta = curMousePos - prevMousePos; //프레임마다 마우스 이동량 계산

        if (Input.GetMouseButtonDown(0))
        {
            mouseDelta = Vector2.zero;
        }

        switch (currentState)
        {
            case GameState.Overview:
                HandleOverviewClick();//클릭시 클로즈업 진입 체크
                HandleDirectDragSpots(mouseDelta); // 오버뷰에서 바로 드래그하는 스팟들 처리
                break;
            case GameState.Minigame:
                if (currentSpot != null)
                {
                    if (currentSpot.minigameType == MinigameType.WipeSmudge)
                        HandleWipe(mouseDelta);//얼룩닦기
                    else if (currentSpot.minigameType == MinigameType.DragPile)
                        HandleDragForSpot(currentSpot, mouseDelta);//더미 치우기
                }
                break;
        }

        prevMousePos = curMousePos;
    }

    // ---------------------------------------------------
    // UI 레이캐스트 공용 함수 - 지정한 화면 좌표 아래에 있는 UI 오브젝트를 찾음
    // ---------------------------------------------------
    private GameObject RaycastUI(Vector2 screenPos)
    {
        PointerEventData ped = new PointerEventData(EventSystem.current) { position = screenPos };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);
        return results.Count > 0 ? results[0].gameObject : null;
    }

    private bool IsSameOrChild(GameObject hit, GameObject target)
    {
        if (hit == null || target == null) return false;
        if (hit == target) return true;
        return hit.transform.IsChildOf(target.transform);
    }

    // ---------------------------------------------------
    // 전체 화면 클릭 처리
    // ---------------------------------------------------
    void HandleOverviewClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        GameObject hit = RaycastUI(Input.mousePosition);
        if (hit == null) return;

        foreach (var spot in spots)
        {
            if (spot.playDirectlyInOverview) continue; // 바로 진행하는 스팟은 클릭으로 열지 않음

            if (IsSameOrChild(hit, spot.worldObject))//확대화면진입
            {
                EnterCloseup(spot);
                return;
            }
        }
    }

    // ---------------------------------------------------
    // 오버뷰 화면에서 바로 진행되는 DragPile 스팟들을 매 프레임 확인
    // ---------------------------------------------------
    void HandleDirectDragSpots(Vector2 mouseDelta)
    {
        foreach (var spot in spots)
        {
            if (!spot.playDirectlyInOverview) continue;
            if (spot.minigameType != MinigameType.DragPile) continue;
            if (spot.dragTarget == null || !spot.dragTarget.activeSelf) continue; // 이미 치웠으면 스킵

            HandleDragForSpot(spot, mouseDelta);
        }
    }

    void EnterCloseup(InteractableSpot spot)
    {
        currentSpot = spot;
        currentState = GameState.Closeup;

        if (overviewRoot != null) overviewRoot.SetActive(false);
        if (spot.closeupView != null) spot.closeupView.SetActive(true);
        if (closeButton != null) closeButton.SetActive(true);

        StartMinigame(spot);
    }

    void StartMinigame(InteractableSpot spot)
    {
        currentState = GameState.Minigame;
        spot.wipedCount = 0;
        spot.isCleared = false;
        wipeProgress.Clear();
        smudgeFoodMap.Clear();
        isMinigameCleared = false; // 클리어 플래그 초기화

        if (spot.minigameType == MinigameType.WipeSmudge)
        {
            // 1. 기존 얼룩 및 잔여 아이템 제거
            foreach (var oldSmudge in spot.activeSmudgeList)
            {
                if (oldSmudge != null) Destroy(oldSmudge);
            }
            spot.activeSmudgeList.Clear();

            foreach (var oldFood in spot.spawnedFoodObjects)
            {
                if (oldFood != null) Destroy(oldFood);
            }
            spot.spawnedFoodObjects.Clear();

            if (spot.smudgePrefab != null && spot.spawnArea != null)
            {
                Rect areaRect = spot.spawnArea.rect;
                int count = Random.Range(2, 4); // 얼룩 개수 (2~3개)

                RectTransform smudgeRectTransform = spot.smudgePrefab.GetComponent<RectTransform>();
                float paddingX = (smudgeRectTransform != null) ? smudgeRectTransform.rect.width / 2f : 50f;
                float paddingY = (smudgeRectTransform != null) ? smudgeRectTransform.rect.height / 2f : 50f;

                for (int i = 0; i < count; i++)
                {
                    float randomX = Random.Range(areaRect.xMin + paddingX, areaRect.xMax - paddingX);
                    float randomY = Random.Range(areaRect.yMin + paddingY, areaRect.yMax - paddingY);
                    Vector2 spawnPos = new Vector2(randomX, randomY);

                    // -----------------------------------------------------------
                    // [1] 얼룩 뒤에 들어갈 아이템 확률 판단 (50% 확률로 등장, 50%는 꽝)
                    // -----------------------------------------------------------
                    FoodItem assignedFood = null;
                    if (Random.value < 0.7f) // 70% 확률로 아이템 등장 (원하는대로 확률 조정가능)
                    {
                        assignedFood = GetRandomFoodItem(spot);
                    }

                    // -----------------------------------------------------------
                    // [2] 아이템이 배치되는 경우, 얼룩 바로 아래(바닥)에 이미지 생성
                    // -----------------------------------------------------------
                    if (assignedFood != null)
                    {
                        GameObject foodObj = new GameObject("HiddenItem", typeof(RectTransform), typeof(Image));
                        foodObj.transform.SetParent(spot.spawnArea, false);

                        Image foodImg = foodObj.GetComponent<Image>();
                        foodImg.sprite = assignedFood.icon;
                        foodImg.preserveAspect = true; // 비율 유지

                        RectTransform foodRect = foodObj.GetComponent<RectTransform>();
                        foodRect.anchoredPosition = spawnPos; // 얼룩 위치와 동일하게 지정!
                        foodRect.sizeDelta = new Vector2(120f, 120f); // 얼룩 크기에 맞춘 적절한 크기

                        spot.spawnedFoodObjects.Add(foodObj);
                    }

                    // -----------------------------------------------------------
                    // [3] 그 위에 얼룩 생성 (Hierarchy상 아래에 위치하므로 아이템을 덮음)
                    // -----------------------------------------------------------
                    GameObject newSmudge = Instantiate(spot.smudgePrefab, spot.spawnArea);
                    RectTransform rect = newSmudge.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchoredPosition = spawnPos;
                    }

                    newSmudge.SetActive(true);
                    Image maskImage = newSmudge.GetComponent<Image>();
                    if (maskImage != null)
                    {
                        Color c = maskImage.color;
                        c.a = 1f;
                        maskImage.color = c;
                    }

                    spot.activeSmudgeList.Add(newSmudge);
                    wipeProgress[newSmudge] = 0f;

                    // 얼룩과 아이템 매칭 기록 (assignedFood가 null이면 꽝)
                    smudgeFoodMap[newSmudge] = assignedFood;
                }
            }
        }
        else if (spot.minigameType == MinigameType.DragPile)
        {
            if (spot.dragTarget != null)
            {
                spot.dragTarget.SetActive(true);
                spot.dragTarget.GetComponent<RectTransform>().anchoredPosition = spot.dragStartAnchoredPos;
            }
            if (spot.hiddenReveal != null) spot.hiddenReveal.SetActive(false);
        }
    }

    // ---------------------------------------------------
    // 얼룩 닦기 - 마우스를 누른 채로 얼룩 위에서 움직인 거리를 누적
    // ---------------------------------------------------
    void HandleWipe(Vector2 mouseDelta)
    {
        if (isMinigameCleared) return;

        if (Input.GetMouseButton(0))
        {
            GameObject hit = RaycastUI(Input.mousePosition);

            // 동적으로 생성된 activeSmudgeList를 순회
            foreach (var mask in currentSpot.activeSmudgeList)
            {
                if (mask == null || !mask.activeSelf) continue;
                if (!IsSameOrChild(hit, mask)) continue;
                //마우스 누를 채 움직인 거리를 누적
                if (!wipeProgress.ContainsKey(mask)) wipeProgress[mask] = 0f;
                wipeProgress[mask] += mouseDelta.magnitude;

                // 누적 진행도(0.0~1.0) 에 따라 알파값(투명도) 감소
                float progressRatio = Mathf.Clamp01(wipeProgress[mask] / wipeThreshold);
                Image maskImage = mask.GetComponent<Image>();
                if (maskImage != null)
                {
                    Color color = maskImage.color;
                    color.a = 1f - progressRatio; //문지를수록 옅어짐
                    maskImage.color = color;
                }

                // 다 문질렀으면 비활성화 및 카운트 증가
                if (wipeProgress[mask] >= wipeThreshold)
                {
                    mask.SetActive(false);
                    currentSpot.wipedCount++;

                    // [신규] 이 얼룩 뒤에 아이템이 지정되어 있었다면, 인벤토리에 추가!
                    if (smudgeFoodMap.ContainsKey(mask) && smudgeFoodMap[mask] != null)
                    {
                        AddToInventory(smudgeFoodMap[mask].itemName);
                    }
                }
                break;
            }
        }

        // 모든 얼룩이 다 지워졌는지 체크, 화면 닫기
        // 한번 실행하면 다시 얼룩 지울 수 없음. 1번만 할 수 있음
        if (!isMinigameCleared && currentSpot.activeSmudgeList.Count > 0 && currentSpot.wipedCount >= currentSpot.activeSmudgeList.Count)
        {
            isMinigameCleared = true; // [중요] 중복 실행 방지
            Invoke(nameof(ExitCloseup), 1.2f); // 마지막 드러난 아이템을 확인하고 닫히도록 1.2초 대기
        }
    }

    // ---------------------------------------------------
    // 더미 드래그 (특정 스팟 기준으로 동작, 클로즈업/오버뷰 어디서든 재사용)
    // ---------------------------------------------------
    void HandleDragForSpot(InteractableSpot spot, Vector2 mouseDelta)
    {
        // 이미 치워진 스팟이면 더 이상 드래그 및 보상 체크를 하지 않음
        if (spot.isCleared) return;

        // 마우스 누르는 순간 대상 잡기
        if (Input.GetMouseButtonDown(0))
        {
            GameObject hit = RaycastUI(Input.mousePosition);
            if (IsSameOrChild(hit, spot.dragTarget))
            {
                spot.isDragging = true;
            }
        }
        // 드래그 중 위치 이동
        else if (Input.GetMouseButton(0) && spot.isDragging && spot.dragRectTransform != null)
        {
            float scale = (mainCanvas != null) ? mainCanvas.scaleFactor : 1f;
            spot.dragRectTransform.anchoredPosition += mouseDelta / scale;
        }
        // 마우스를 뗐을 때 치운 거리 판정
        else if (Input.GetMouseButtonUp(0) && spot.isDragging)
        {
            spot.isDragging = false;

            if (spot.dragRectTransform != null)
            {
                // 원래 위치와의 거리가 dragClearDistance를 넘었는지 확인
                float distance = Vector2.Distance(spot.dragRectTransform.anchoredPosition, spot.revealAnchoredPos);
                if (distance > spot.dragClearDistance && spot.hiddenReveal != null)
                {
                    spot.isCleared = true; // [중요] 최초 1회 성공 처리 (중복 지급 방지)

                    spot.hiddenReveal.SetActive(true); // 숨겨진 아이템 등장
                    GiveRandomFood(spot); // 보상 지급 (이제 딱 1번만 실행됩니다!)

                    // 클로즈업 화면을 거쳐 들어온 경우에만 화면을 닫는다.
                    if (!spot.playDirectlyInOverview)
                    {
                        Invoke(nameof(ExitCloseup), 0.8f);
                    }
                }
            }
        }
    }

    // ---------------------------------------------------
    // 확률 계산 후 FoodItem 객체 반환하는 함수
    // ---------------------------------------------------
    FoodItem GetRandomFoodItem(InteractableSpot spot)
    {
        if (spot.possibleFoods == null || spot.possibleFoods.Count == 0) return null;

        // 1. 등록된 모든 음식 가중치 총합 계산
        float totalWeight = 0f;
        foreach (var f in spot.possibleFoods) totalWeight += f.weight;

        // 2. 0 ~ totalWeight 사이의 랜덤값 가챠
        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        // 3. 누적 가중치 범위 안에 드는 아이템 선택
        foreach (var f in spot.possibleFoods)
        {
            cumulative += f.weight;
            if (rand <= cumulative)
            {
                return f;
            }
        }
        return null;
    }

    // ---------------------------------------------------
    // 랜덤 확률로 식량 지급
    // ---------------------------------------------------
    void GiveRandomFood(InteractableSpot spot)
    {
        FoodItem item = GetRandomFoodItem(spot);
        if (item != null)
        {
            AddToInventory(item.itemName);
        }
    }

    void AddToInventory(string itemName)
    {
        if (inventory.ContainsKey(itemName)) inventory[itemName]++;
        else inventory[itemName] = 1;

        UpdateInventoryUI();
    }

    void UpdateInventoryUI()
    {
        if (inventoryText == null) return;

        string result = "[획득한 식량]\n";
        foreach (var kv in inventory)
            result += $"{kv.Key} x{kv.Value}\n";

        inventoryText.text = result;
    }

    // 닫기 버튼의 OnClick()에 연결 (파라미터 없음)
    public void ExitCloseup()
    {
        if (currentSpot != null)
        {
            if (currentSpot.closeupView != null)
                currentSpot.closeupView.SetActive(false);

            // 클리어 시 생성했던 아이템들 깔끔하게 삭제
            foreach (var foodObj in currentSpot.spawnedFoodObjects)
            {
                if (foodObj != null) Destroy(foodObj);
            }
            currentSpot.spawnedFoodObjects.Clear();
        }

        currentSpot = null;

        if (closeButton != null) closeButton.SetActive(false);
        if (overviewRoot != null) overviewRoot.SetActive(true);

        currentState = GameState.Overview;
    }
}