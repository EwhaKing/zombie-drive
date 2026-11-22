using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 메인씬 인벤토리: 카테고리별 표시 + 아이템이 많아지면 슬롯이 아래로 늘어나고 스크롤
public class CategoryInventoryUI : MonoBehaviour
{
    [Header("UI 연결")]
    public Transform slotContainer;      // ScrollRect의 Content (GridLayoutGroup이 붙은 오브젝트)
    public ItemSlot slotPrefab;          // SlotPrefab (부족할 때 자동 생성)
    public ScrollRect scrollRect;        // 카테고리를 바꿀 때 맨 위로 올리기 위해 (선택)
    public ItemCategory currentCategory = ItemCategory.Food;

    [Header("슬롯 배치")]
    public int columns = 4;              // GridLayoutGroup의 열 개수와 같게
    public int minRows = 4;              // 아이템이 없어도 보여줄 최소 줄 수 (4x4 = 16칸)

    void OnEnable()
    {
        SelectCategory((int)currentCategory);
    }

    // 카테고리 탭 버튼 OnClick에 연결 (Food=0, Material=1, Medicine=2, Furniture=3, Clothes=4)
    public void SelectCategory(int categoryIndex)
    {
        currentCategory = (ItemCategory)categoryIndex;
        RefreshUI();
        ScrollToTop();
    }

    public void RefreshUI()
    {
        if (slotContainer == null) return;

        // 1) 현재 카테고리의 아이템 모으기 (이름순 정렬 → 표시 순서가 항상 일정)
        var list = new List<KeyValuePair<ItemData, int>>();
        if (InventoryManager.Instance != null)
        {
            foreach (var pair in InventoryManager.Instance.items)
            {
                if (pair.Key != null && pair.Key.category == currentCategory) list.Add(pair);
            }
        }
        list.Sort((a, b) => string.Compare(a.Key.itemName, b.Key.itemName, System.StringComparison.Ordinal));

        // 2) 필요한 슬롯 수 계산: 최소 줄 수 보장 + 마지막 줄은 열 개수만큼 채움
        int cols = Mathf.Max(1, columns);
        int rows = Mathf.Max(minRows, Mathf.CeilToInt(list.Count / (float)cols));
        int needed = rows * cols;

        // 3) 부족하면 슬롯 생성 (한 번 만든 슬롯은 재사용)
        while (slotContainer.childCount < needed)
        {
            if (slotPrefab == null)
            {
                Debug.LogError("[CategoryInventoryUI] slotPrefab이 연결되지 않아 슬롯을 더 만들 수 없습니다.");
                break;
            }
            Instantiate(slotPrefab, slotContainer, false);
        }

        // 4) 필요한 만큼만 켜고, 나머지는 끄기 (끈 슬롯은 GridLayout에서 제외됨)
        for (int i = 0; i < slotContainer.childCount; i++)
        {
            Transform child = slotContainer.GetChild(i);
            bool on = i < needed;
            if (child.gameObject.activeSelf != on) child.gameObject.SetActive(on);
            if (!on) continue;

            ItemSlot slot = child.GetComponent<ItemSlot>();
            if (slot == null) continue;

            if (i < list.Count) slot.SetItem(list[i].Key, list[i].Value);
            else slot.ClearSlot();
        }
    }

    void ScrollToTop()
    {
        if (scrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        RefreshUI();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}