using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    public enum GameState { Overview, Closeup, Minigame }
    public enum MinigameType { None, WipeSmudge, DragPile }

    [System.Serializable]
    public class FoodItem
    {
        public string itemName;
        public Sprite icon;
        [Range(0.01f, 100f)] public float weight = 10f;
    }

    [System.Serializable]
    public class InteractableSpot
    {
        [Header("기본 정보")]
        public string spotName;
        public GameObject worldObject;     // 전체 화면에서 클릭할 대상 (playDirectlyInOverview면 안 씀)
        public GameObject closeupView;     // (playDirectlyInOverview면 안 씀)
        public MinigameType minigameType;

        [Header("오버뷰에서 바로 진행 (DragPile 전용)")]
        [Tooltip("체크하면 클릭으로 화면을 전환하지 않고, 오버뷰 화면에 놓인 dragTarget을 바로 드래그해서 치울 수 있습니다.")]
        public bool playDirectlyInOverview = false;

        [Header("얼룩 닦기용 (WipeSmudge)")]
        public GameObject[] smudgeMasks;   // 각각 Image만 있으면 됨 

        [Header("더미 치우기용 (DragPile)")]
        public GameObject dragTarget;      // Image만 있으면 됨 
        public GameObject hiddenReveal;
        public float dragClearDistance = 150f;

        [Header("여기서 얻을 수 있는 식량")]
        public List<FoodItem> possibleFoods;

        [HideInInspector] public int wipedCount;
        [HideInInspector] public Vector2 dragStartAnchoredPos;
        [HideInInspector] public Vector2 revealAnchoredPos;

        // 스팟별 드래그 진행 상태 (기존엔 매니저에 하나뿐이라 동시 처리가 안 됐음)
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
    public Text inventoryText;
    public GameObject closeButton; // 이건 그대로 Button + OnClick(ExitCloseup) 유지

    [Header("얼룩 닦기 민감도")]
    [Tooltip("마우스를 누른 채로 얼룩 위에서 움직여야 하는 누적 픽셀 거리. 값이 클수록 오래 문질러야 함")]
    public float wipeThreshold = 1000f; //인스펙터에서 조절가능

    private InteractableSpot currentSpot;
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    private Vector2 prevMousePos;
    private Dictionary<GameObject, float> wipeProgress = new Dictionary<GameObject, float>();

    void Start()
    {
        foreach (var spot in spots)
        {
            if (spot.closeupView != null) spot.closeupView.SetActive(false);

            if (spot.minigameType == MinigameType.DragPile)
            {
                if (spot.dragTarget != null)
                {
                    spot.dragRectTransform = spot.dragTarget.GetComponent<RectTransform>();
                    spot.dragStartAnchoredPos = spot.dragRectTransform.anchoredPosition;
                }
                if (spot.hiddenReveal != null)
                    spot.revealAnchoredPos = spot.hiddenReveal.GetComponent<RectTransform>().anchoredPosition;

                if (spot.playDirectlyInOverview)
                {
                    // 오버뷰 화면에 바로 배치되어 있으므로 시작하자마자 활성화해둠
                    if (spot.dragTarget != null) spot.dragTarget.SetActive(true);
                    if (spot.hiddenReveal != null) spot.hiddenReveal.SetActive(false);
                }
            }
        }

        if (closeButton != null) closeButton.SetActive(false);
        if (overviewRoot != null) overviewRoot.SetActive(true);

        currentState = GameState.Overview;
        prevMousePos = Input.mousePosition;
        UpdateInventoryUI();
    }

    void Update()
    {
        Vector2 curMousePos = Input.mousePosition;
        Vector2 mouseDelta = curMousePos - prevMousePos;

        if (Input.GetMouseButtonDown(0))
    {
        mouseDelta = Vector2.zero;
    }

        switch (currentState)
        {
            case GameState.Overview:
                HandleOverviewClick();
                HandleDirectDragSpots(mouseDelta); // 오버뷰에서 바로 드래그하는 스팟들 처리
                break;
            case GameState.Minigame:
                if (currentSpot != null)
                {
                    if (currentSpot.minigameType == MinigameType.WipeSmudge)
                        HandleWipe(mouseDelta);
                    else if (currentSpot.minigameType == MinigameType.DragPile)
                        HandleDragForSpot(currentSpot, mouseDelta);
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

            if (IsSameOrChild(hit, spot.worldObject))
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
        wipeProgress.Clear();

        if (spot.minigameType == MinigameType.WipeSmudge)
        {
            foreach (var mask in spot.smudgeMasks)
            {
                if (mask == null) continue;
                mask.SetActive(true);
                wipeProgress[mask] = 0f;
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
        if (Input.GetMouseButton(0))
        {
            GameObject hit = RaycastUI(Input.mousePosition);

            foreach (var mask in currentSpot.smudgeMasks)
            {
                if (mask == null || !mask.activeSelf) continue;
                if (!IsSameOrChild(hit, mask)) continue;

                // 실제로 마우스가 움직인만큼 누적
                if (!wipeProgress.ContainsKey(mask)) wipeProgress[mask] = 0f;
                wipeProgress[mask] += mouseDelta.magnitude;

                // 문지른 비율에 따라 알파값 조절(0~1)
                float progressRatio = Mathf.Clamp01(wipeProgress[mask] / wipeThreshold);

                Image maskImage = mask.GetComponent<Image>();
                if (maskImage != null)
                {
                    Color color = maskImage.color;
                    color.a = 1f - progressRatio; // progressRatio가 올라갈수록 Alpha는 1에서 0으로 감소
                    maskImage.color = color;
                }

                // 목표 수치(wipeThreshold) 달성 시 오브젝트 비활성화 및 카운트 증가
                if (wipeProgress[mask] >= wipeThreshold)
                {
                    mask.SetActive(false);
                    currentSpot.wipedCount++;
                }
                break;
            }
        }

        if (currentSpot.wipedCount >= currentSpot.smudgeMasks.Length)
        {
            GiveRandomFood(currentSpot);
            Invoke(nameof(ExitCloseup), 0.4f);
        }
    }

    // ---------------------------------------------------
    // 더미 드래그 (특정 스팟 기준으로 동작, 클로즈업/오버뷰 어디서든 재사용)
    // ---------------------------------------------------
    void HandleDragForSpot(InteractableSpot spot, Vector2 mouseDelta)
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject hit = RaycastUI(Input.mousePosition);
            if (IsSameOrChild(hit, spot.dragTarget))
            {
                spot.isDragging = true;
            }
        }
        else if (Input.GetMouseButton(0) && spot.isDragging && spot.dragRectTransform != null)
        {
            float scale = (mainCanvas != null) ? mainCanvas.scaleFactor : 1f;
            spot.dragRectTransform.anchoredPosition += mouseDelta / scale;
        }
        else if (Input.GetMouseButtonUp(0) && spot.isDragging)
        {
            spot.isDragging = false;

            if (spot.dragRectTransform != null)
            {
                float distance = Vector2.Distance(spot.dragRectTransform.anchoredPosition, spot.revealAnchoredPos);
                if (distance > spot.dragClearDistance && spot.hiddenReveal != null)
                {
                    spot.hiddenReveal.SetActive(true);
                    GiveRandomFood(spot);

                    // 클로즈업 화면을 거쳐 들어온 경우에만 화면을 닫는다.
                    // 오버뷰에서 바로 진행한 스팟은 화면 전환이 필요 없다.
                    if (!spot.playDirectlyInOverview)
                    {
                        Invoke(nameof(ExitCloseup), 0.8f);
                    }
                }
            }
        }
    }

    // ---------------------------------------------------
    // 랜덤 확률로 식량 지급
    // ---------------------------------------------------
    void GiveRandomFood(InteractableSpot spot)
    {
        if (spot.possibleFoods == null || spot.possibleFoods.Count == 0) return;

        float totalWeight = 0f;
        foreach (var f in spot.possibleFoods) totalWeight += f.weight;

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var f in spot.possibleFoods)
        {
            cumulative += f.weight;
            if (rand <= cumulative)
            {
                AddToInventory(f.itemName);
                break;
            }
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
        if (currentSpot != null && currentSpot.closeupView != null)
            currentSpot.closeupView.SetActive(false);

        currentSpot = null;

        if (closeButton != null) closeButton.SetActive(false);
        if (overviewRoot != null) overviewRoot.SetActive(true);

        currentState = GameState.Overview;
    }
}