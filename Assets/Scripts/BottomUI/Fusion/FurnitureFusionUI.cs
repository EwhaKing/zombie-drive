// FurnitureFusionUI.cs  (FusionPanel에 붙임)
// 재료/결과 가구는 모두 InventoryManager(파밍 재료가 저장되는 그 인벤토리)를 사용합니다.
// 골드만 PlayerInventory.gold 를 사용합니다.
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureFusionUI : MonoBehaviour
{
    [Header("도안(레시피) 목록")]
    public FurnitureRecipe[] recipes;                 // 가진 도안 목록
    public Transform recipeListContent;               // 도안 칸들이 생성될 부모 (ScrollView의 Content)
    public FurnitureRecipeSlotUI recipeSlotPrefab;    // 도안 칸 프리팹 (비워두면 첫 도안이 자동 선택됨)

    [Header("선택한 도안 상세")]
    public Image resultImage;
    public TMP_Text nameText, descText, costText;
    public MaterialSlotUI[] slots;
    public Button craftButton;

    [Header("선택 사항")]
    public GameObject emptyGuide;                     // "제작할 가구를 선택하세요" 안내 오브젝트
    public TMP_Text messageText;                      // "○○ 제작 완료!" 등 짧은 메시지
    public float messageSeconds = 2f;
    public bool autoSelectFirst = false;              // 열 때 첫 도안을 자동 선택할지

    FurnitureRecipe selected;
    readonly List<FurnitureRecipeSlotUI> recipeSlots = new List<FurnitureRecipeSlotUI>();
    Coroutine messageRoutine;

    void OnEnable()
    {
        BuildRecipeList();

        bool noList = recipeSlotPrefab == null || recipeListContent == null;
        bool pickFirst = (autoSelectFirst || noList) && recipes != null && recipes.Length > 0;
        selected = pickFirst ? recipes[0] : null;

        if (messageText != null) messageText.text = "";
        Refresh(); // 열 때마다 인벤토리 최신 수량으로 갱신
    }

    void BuildRecipeList()
    {
        foreach (var s in recipeSlots)
        {
            if (s == null) continue;
            s.gameObject.SetActive(false);
            Destroy(s.gameObject);
        }
        recipeSlots.Clear();

        if (recipeSlotPrefab == null || recipeListContent == null || recipes == null) return;

        foreach (var r in recipes)
        {
            if (r == null) continue;
            FurnitureRecipeSlotUI s = Instantiate(recipeSlotPrefab, recipeListContent);
            s.Setup(r, Select);
            recipeSlots.Add(s);
        }
    }

    public void Select(FurnitureRecipe recipe)   // 도안 칸 클릭 시 호출됨
    {
        selected = recipe;
        Refresh();
    }

    // ===== 인벤토리 조회 (InventoryManager 사용) =====
    static int CountOf(ItemData item)
    {
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null || item == null) return 0;
        return inv.items.TryGetValue(item, out int n) ? n : 0;
    }

    static int Gold => PlayerInventory.Instance != null ? PlayerInventory.Instance.gold : 0;

    // 재료(같은 재료가 여러 줄이어도 합산) + 골드가 충분한지
    bool CanCraft(FurnitureRecipe r)
    {
        if (r == null || InventoryManager.Instance == null) return false;
        if (Gold < r.goldCost) return false;

        var need = new Dictionary<ItemData, int>();
        foreach (var ing in r.ingredients)
        {
            if (ing == null || ing.item == null) return false;   // 레시피 설정 오류
            need[ing.item] = need.GetValueOrDefault(ing.item) + ing.amount;
        }
        foreach (var kv in need)
        {
            if (CountOf(kv.Key) < kv.Value) return false;
        }
        return true;
    }

    void Refresh()
    {
        bool has = selected != null;

        if (emptyGuide != null) emptyGuide.SetActive(!has);

        if (resultImage != null)
        {
            resultImage.enabled = has;
            if (has)
            {
                resultImage.sprite = selected.ResultIcon;
                resultImage.preserveAspect = true;
            }
        }
        if (nameText != null) nameText.text = has ? selected.DisplayName : "";
        if (descText != null) descText.text = has ? selected.Description : "";

        if (costText != null)
        {
            costText.text = has ? selected.goldCost.ToString() : "";
            costText.color = (has && Gold < selected.goldCost) ? Color.red : Color.white;
        }

        // 재료 칸
        for (int i = 0; i < slots.Length; i++)
        {
            if (has && i < selected.ingredients.Count && selected.ingredients[i] != null)
            {
                Ingredient ing = selected.ingredients[i];
                slots[i].Set(ing, CountOf(ing.item));
            }
            else slots[i].Clear();
        }
        if (has && selected.ingredients.Count > slots.Length)
            Debug.LogWarning($"[FurnitureFusionUI] '{selected.name}' 재료가 {selected.ingredients.Count}개인데 재료 칸은 {slots.Length}개뿐입니다.");

        if (craftButton != null) craftButton.interactable = has && CanCraft(selected);

        // 도안 목록: 선택 표시 + 제작 가능 표시
        foreach (var s in recipeSlots)
        {
            if (s == null) continue;
            s.SetState(s.Recipe == selected, CanCraft(s.Recipe));
        }
    }

    public void OnCraft()   // CraftButton OnClick에 연결
    {
        if (selected == null) return;

        if (!CanCraft(selected)) { Refresh(); return; }   // 한 번 더 검사

        if (selected.resultItem == null)
        {
            Debug.LogError($"[FurnitureFusionUI] '{selected.name}' 레시피에 Result Item이 비어 있습니다.");
            ShowMessage("레시피 설정 오류");
            return;
        }

        InventoryManager inv = InventoryManager.Instance;

        foreach (var ing in selected.ingredients)
            inv.RemoveItem(ing.item, ing.amount);          // 재료 차감 (0이 되면 자동으로 목록에서 제거)

        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.gold -= selected.goldCost;

        inv.AddItem(selected.resultItem, 1);               // 결과 가구 → 메인 인벤토리 '가구' 탭에 표시됨

        ShowMessage($"{selected.DisplayName} 제작 완료!");
        Refresh();   // 수량 갱신, 부족해지면 버튼 비활성화
    }

    void ShowMessage(string msg)
    {
        if (messageText == null) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(msg));
    }

    IEnumerator MessageRoutine(string msg)
    {
        messageText.text = msg;
        yield return new WaitForSeconds(messageSeconds);
        messageText.text = "";
        messageRoutine = null;
    }

    // ===== 테스트용: Play 중 컴포넌트 우측 ⋮ 메뉴에서 실행 =====
    [ContextMenu("디버그: 선택한 도안의 재료와 골드 지급")]
    void DebugGiveForSelected()
    {
        if (selected == null) { Debug.LogWarning("먼저 도안을 선택하세요."); return; }
        if (InventoryManager.Instance == null) { Debug.LogWarning("InventoryManager가 씬에 없습니다."); return; }

        foreach (var ing in selected.ingredients)
            if (ing != null && ing.item != null) InventoryManager.Instance.AddItem(ing.item, ing.amount);

        if (PlayerInventory.Instance != null) PlayerInventory.Instance.gold += selected.goldCost;
        Refresh();
    }
}