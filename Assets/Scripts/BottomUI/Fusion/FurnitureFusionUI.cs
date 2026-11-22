// FurnitureFusionUI.cs  (FusionPanel에 붙임)
// 재료/결과 가구는 모두 InventoryManager(파밍 재료가 저장되는 그 인벤토리)를 사용합니다.
// 도안은 Resources/Recipes 폴더에서 자동으로 불러옵니다. (골드/코인은 사용하지 않음)
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureFusionUI : MonoBehaviour
{
    [Header("도안(레시피) 목록")]
    public bool autoLoadFromResources = true;         // true: Resources 폴더의 모든 도안을 자동으로 불러옴
    public string resourcesPath = "Recipes";          // Assets/Resources/Recipes
    public FurnitureRecipe[] recipes;                 // 자동 로드가 꺼져 있거나 폴더가 비었을 때 쓰는 수동 목록
    public Transform recipeListContent;               // 도안 칸들이 생성될 부모 (ScrollView의 Content)
    public FurnitureRecipeSlotUI recipeSlotPrefab;    // 도안 칸 프리팹 (비워두면 첫 도안이 자동 선택됨)

    [Header("선택한 도안 상세")]
    public Image resultImage;
    public TMP_Text nameText, descText;
    public MaterialSlotUI[] slots;
    public Button craftButton;

    [Header("선택 사항")]
    public GameObject emptyGuide;                     // "제작할 가구를 선택하세요" 안내 오브젝트
    // 제작 불가 이유(재료 부족)와 "제작 완료!" 메시지를 보여줄 글자
    public TMP_Text messageText;
    public float messageSeconds = 2f;
    public bool autoSelectFirst = false;              // 열 때 첫 도안을 자동 선택할지

    FurnitureRecipe selected;
    readonly List<FurnitureRecipeSlotUI> recipeSlots = new List<FurnitureRecipeSlotUI>();
    Coroutine messageRoutine;

    void OnEnable()
    {
        LoadRecipes();
        BuildRecipeList();

        bool noList = recipeSlotPrefab == null || recipeListContent == null;
        bool pickFirst = (autoSelectFirst || noList) && recipes != null && recipes.Length > 0;
        selected = pickFirst ? recipes[0] : null;

        Refresh(); // 열 때마다 인벤토리 최신 수량으로 갱신
    }

    // Resources/Recipes 안의 도안을 전부 읽어옴 (하위 폴더 포함)
    void LoadRecipes()
    {
        if (!autoLoadFromResources) return;

        FurnitureRecipe[] loaded = Resources.LoadAll<FurnitureRecipe>(resourcesPath);
        if (loaded.Length == 0)
        {
            if (recipes == null || recipes.Length == 0)
                Debug.LogWarning($"[FurnitureFusionUI] Resources/{resourcesPath} 폴더에서 도안을 찾지 못했습니다.");
            return;   // 수동 목록 유지
        }

        Array.Sort(loaded, (a, b) =>
        {
            int c = a.sortOrder.CompareTo(b.sortOrder);
            return c != 0 ? c : string.Compare(a.name, b.name, StringComparison.Ordinal);
        });
        recipes = loaded;
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
        StopMessage();
        Refresh();
    }

    // ===== 인벤토리 조회 (InventoryManager 사용) =====
    static int CountOf(ItemData item)
    {
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null || item == null) return 0;
        return inv.items.TryGetValue(item, out int n) ? n : 0;
    }

    // 제작할 수 없는 이유 (제작 가능하면 null)
    string GetBlockReason(FurnitureRecipe r)
    {
        if (r == null) return "도안을 선택하세요.";
        if (InventoryManager.Instance == null) return "InventoryManager가 씬에 없습니다.";

        // 같은 재료가 여러 줄이어도 합산해서 검사
        var need = new Dictionary<ItemData, int>();
        foreach (var ing in r.ingredients)
        {
            if (ing == null || ing.item == null) return "도안의 재료 설정이 비어 있습니다.";
            need[ing.item] = need.GetValueOrDefault(ing.item) + ing.amount;
        }
        foreach (var kv in need)
        {
            int have = CountOf(kv.Key);
            if (have < kv.Value) return $"재료가 부족합니다. ({kv.Key.itemName} {have}/{kv.Value})";
        }

        return null;
    }

    bool CanCraft(FurnitureRecipe r) => GetBlockReason(r) == null;

    void Refresh()
    {
        bool has = selected != null;
        string reason = has ? GetBlockReason(selected) : null;

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

        // 재료 칸: 이미지 + 이름 + 보유/필요 수량
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

        // 제작 버튼: 도안을 골랐다면 항상 누를 수 있게 두고, 못 만드는 상태면 흐리게 표시.
        if (craftButton != null)
        {
            craftButton.interactable = has;
            if (craftButton.targetGraphic != null)
                craftButton.targetGraphic.color = (reason == null) ? Color.white : new Color(1f, 1f, 1f, 0.55f);
        }

        // 도안 목록: 선택 표시 + 제작 가능 표시
        foreach (var s in recipeSlots)
        {
            if (s == null) continue;
            s.SetState(s.Recipe == selected, CanCraft(s.Recipe));
        }

        // 상태 메시지 (제작 불가 이유를 빨간 글씨로 계속 표시)
        if (messageRoutine == null && messageText != null)
        {
            messageText.color = Color.red;
            messageText.text = reason ?? "";
        }
    }

    public void OnCraft()   // CraftButton OnClick에 연결
    {
        if (selected == null) return;

        string reason = GetBlockReason(selected);
        if (reason != null)
        {
            ShowMessage(reason, Color.red);
            Debug.Log($"[FurnitureFusionUI] 제작 불가: {reason}");
            return;
        }

        if (selected.resultItem == null)
        {
            Debug.LogError($"[FurnitureFusionUI] '{selected.name}' 도안에 Result Item이 비어 있습니다.");
            ShowMessage("도안에 결과 아이템이 없습니다.", Color.red);
            return;
        }

        InventoryManager inv = InventoryManager.Instance;

        foreach (var ing in selected.ingredients)
            inv.RemoveItem(ing.item, ing.amount);          // 재료 차감 (0이 되면 자동으로 목록에서 제거)

        inv.AddItem(selected.resultItem, 1);               // 결과 가구 → 메인 인벤토리 '가구' 탭에 표시됨

        string doneMsg = $"{selected.DisplayName} 제작 완료!";
        Refresh();                                         // 수량 갱신
        ShowMessage(doneMsg, Color.white);
    }

    // ===== 메시지 =====
    void ShowMessage(string msg, Color color)
    {
        if (messageText == null) return;
        StopMessage();
        messageRoutine = StartCoroutine(MessageRoutine(msg, color));
    }

    void StopMessage()
    {
        if (messageRoutine != null) { StopCoroutine(messageRoutine); messageRoutine = null; }
    }

    IEnumerator MessageRoutine(string msg, Color color)
    {
        messageText.color = color;
        messageText.text = msg;
        yield return new WaitForSeconds(messageSeconds);
        messageRoutine = null;
        Refresh();   // 메시지가 끝나면 현재 상태 메시지로 되돌림
    }

    // ===== 테스트용: Play 중 컴포넌트 우측 ⋮ 메뉴에서 실행 =====
    [ContextMenu("디버그: 선택한 도안의 재료 지급")]
    void DebugGiveForSelected()
    {
        if (selected == null) { Debug.LogWarning("먼저 도안을 선택하세요."); return; }
        if (InventoryManager.Instance == null) { Debug.LogWarning("InventoryManager가 씬에 없습니다."); return; }

        foreach (var ing in selected.ingredients)
            if (ing != null && ing.item != null) InventoryManager.Instance.AddItem(ing.item, ing.amount);

        Refresh();
    }
}