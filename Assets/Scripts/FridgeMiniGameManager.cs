using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Button의 OnClick() 파라미터 수동 등록, Event Trigger 설정을 전부 없애고
/// 이 스크립트 하나가 마우스 입력을 직접 감지하도록 만든 버전입니다.
///
/// 필요한 것: 클릭/드래그 대상이 되는 오브젝트에는 Image 컴포넌트만 있으면 됩니다.
/// (Raycast Target 체크만 켜져 있으면 OK. Button, Event Trigger는 필요 없습니다.)
///
/// 얼룩(Smudge)은 "누르고 있는 상태로 그 위에서 마우스를 움직인 누적 거리"가
/// wipeThreshold를 넘으면 지워집니다. 그냥 한 번 클릭하는 것만으로는 안 지워집니다.
/// </summary>
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
        public GameObject worldObject;     // 전체 화면에서 클릭할 대상 (Image만 있으면 됨)
        public GameObject closeupView;
        public MinigameType minigameType;

        [Header("얼룩 닦기용 (WipeSmudge)")]
        public GameObject[] smudgeMasks;   // 각각 Image만 있으면 됨 (Button 불필요)

        [Header("더미 치우기용 (DragPile)")]
        public GameObject dragTarget;      // Image만 있으면 됨 (Event Trigger 불필요)
        public GameObject hiddenReveal;
        public float dragClearDistance = 150f;

        [Header("여기서 얻을 수 있는 식량")]
        public List<FoodItem> possibleFoods;

        [HideInInspector] public int wipedCount;
        [HideInInspector] public Vector2 dragStartAnchoredPos;
        [HideInInspector] public Vector2 revealAnchoredPos;
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
    public float wipeThreshold = 250f;

    private InteractableSpot currentSpot;
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    private Vector2 prevMousePos;
    private Dictionary<GameObject, float> wipeProgress = new Dictionary<GameObject, float>();

    private bool isDraggingPile = false;
    private RectTransform draggingRect;

    void Start()
    {
        foreach (var spot in spots)
        {
            if (spot.closeupView != null) spot.closeupView.SetActive(false);

            if (spot.minigameType == MinigameType.DragPile)
            {
                if (spot.dragTarget != null)
                    spot.dragStartAnchoredPos = spot.dragTarget.GetComponent<RectTransform>().anchoredPosition;
                if (spot.hiddenReveal != null)
                    spot.revealAnchoredPos = spot.hiddenReveal.GetComponent<RectTransform>().anchoredPosition;
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

        switch (currentState)
        {
            case GameState.Overview:
                HandleOverviewClick();
                break;
            case GameState.Minigame:
                if (currentSpot != null)
                {
                    if (currentSpot.minigameType == MinigameType.WipeSmudge)
                        HandleWipe(mouseDelta);
                    else if (currentSpot.minigameType == MinigameType.DragPile)
                        HandleDrag(mouseDelta);
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
            if (IsSameOrChild(hit, spot.worldObject))
            {
                EnterCloseup(spot);
                return;
            }
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

                // 실제로 마우스가 "움직인" 만큼만 누적 (가만히 눌러만 두면 안 지워짐)
                if (!wipeProgress.ContainsKey(mask)) wipeProgress[mask] = 0f;
                wipeProgress[mask] += mouseDelta.magnitude;

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
    // 더미 드래그
    // ---------------------------------------------------
    void HandleDrag(Vector2 mouseDelta)
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject hit = RaycastUI(Input.mousePosition);
            if (IsSameOrChild(hit, currentSpot.dragTarget))
            {
                isDraggingPile = true;
                draggingRect = currentSpot.dragTarget.GetComponent<RectTransform>();
            }
        }
        else if (Input.GetMouseButton(0) && isDraggingPile && draggingRect != null)
        {
            float scale = (mainCanvas != null) ? mainCanvas.scaleFactor : 1f;
            draggingRect.anchoredPosition += mouseDelta / scale;
        }
        else if (Input.GetMouseButtonUp(0) && isDraggingPile)
        {
            isDraggingPile = false;

            if (draggingRect != null)
            {
                float distance = Vector2.Distance(draggingRect.anchoredPosition, currentSpot.revealAnchoredPos);
                if (distance > currentSpot.dragClearDistance && currentSpot.hiddenReveal != null)
                {
                    currentSpot.hiddenReveal.SetActive(true);
                    GiveRandomFood(currentSpot);
                    Invoke(nameof(ExitCloseup), 0.8f);
                }
            }
            draggingRect = null;
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

    // 닫기 버튼의 OnClick()에 연결 (파라미터 없음, 이 버튼만 예전처럼 Button + OnClick 사용)
    public void ExitCloseup()
    {
        if (currentSpot != null && currentSpot.closeupView != null)
            currentSpot.closeupView.SetActive(false);

        currentSpot = null;
        isDraggingPile = false;
        draggingRect = null;

        if (closeButton != null) closeButton.SetActive(false);
        if (overviewRoot != null) overviewRoot.SetActive(true);

        currentState = GameState.Overview;
    }
}
