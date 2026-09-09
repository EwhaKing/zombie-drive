using System.Collections.Generic;
using UnityEngine;

public class CategoryInventoryUI : MonoBehaviour
{
    [Header("UI 연결")]
    public Transform slotContainer; 
    public ItemCategory currentCategory = ItemCategory.Food; 

    void OnEnable()
    {
        SelectCategory((int)currentCategory); 
    }

    public void SelectCategory(int categoryIndex)
    {
        currentCategory = (ItemCategory)categoryIndex;
        RefreshUI();
    }

    public void RefreshUI()
    {
        ClearUI();

        if (InventoryManager.Instance == null || slotContainer == null) return;

        int slotIndex = 0;

        foreach (var pair in InventoryManager.Instance.items)
        {
            if (pair.Key.category == currentCategory)
            {
                if (slotIndex >= slotContainer.childCount) break;

                Transform slotTF = slotContainer.GetChild(slotIndex);
                ItemSlot slotScript = slotTF.GetComponent<ItemSlot>();

                // [핵심] ItemSlot 컴포넌트에 데이터 넘겨주기
                if (slotScript != null)
                {
                    slotScript.SetItem(pair.Key, pair.Value);
                }

                slotIndex++;
            }
        }
    }

    void ClearUI()
    {
        if (slotContainer == null) return;

        foreach (Transform slotTF in slotContainer)
        {
            ItemSlot slotScript = slotTF.GetComponent<ItemSlot>();
            if (slotScript != null)
            {
                slotScript.ClearSlot(); // 각 슬롯의 데이터 초기화
            }
        }
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