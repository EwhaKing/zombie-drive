using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryInventoryUI : MonoBehaviour
{
    [Header("UI 연결")]
    public Transform slotContainer; // Grid Layout Group이 들어간 슬롯 부모 오브젝트
    public ItemCategory currentCategory = ItemCategory.Food; // 기본 탭: 식량

    void OnEnable()
    {
        SelectCategory((int)currentCategory); // 창이 켜질 때 현재 탭 갱신
    }

    // 탭 버튼 클릭 시 실행할 함수 (버튼 OnClick에 연결)
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

        // 전체 아이템 중 현재 선택된 카테고리와 일치하는 아이템만 필터링
        foreach (var pair in InventoryManager.Instance.items)
        {
            if (pair.Key.category == currentCategory)
            {
                if (slotIndex >= slotContainer.childCount) break;

                Transform slot = slotContainer.GetChild(slotIndex);

                // 아이콘 표시
                Transform iconTF = slot.Find("Icon");
                if (iconTF != null)
                {
                    Image img = iconTF.GetComponent<Image>();
                    img.sprite = pair.Key.icon;
                    img.preserveAspect = true;
                    iconTF.gameObject.SetActive(true);
                }

                // 수량 표시
                Transform textTF = slot.Find("number");
                if (textTF != null)
                {
                    TextMeshProUGUI txt = textTF.GetComponent<TextMeshProUGUI>();
                    txt.text = pair.Value.ToString();
                    textTF.gameObject.SetActive(true);
                }

                slotIndex++;
            }
        }
    }

    void ClearUI()
    {
        foreach (Transform slot in slotContainer)
        {
            foreach (Transform child in slot)
            {
                child.gameObject.SetActive(false); // 슬롯 내 자식 UI(Icon, Text) 숨기기
            }
        }
    }


    // 팝업 열기
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        RefreshUI(); // 창이 열릴 때 아이템 목록 최신화
    }

    // 팝업 닫기
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}