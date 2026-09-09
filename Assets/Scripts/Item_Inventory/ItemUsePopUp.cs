using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemUsePopUp : MonoBehaviour
{
    public static ItemUsePopUp Instance;

    public GameObject panel;
    public Image itemIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Button useButton;
    public Button cancelButton;

    private ItemData targetItem;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
        if (cancelButton != null) cancelButton.onClick.AddListener(() => panel.SetActive(false));
    }

    public void ShowPopUp(ItemData item)
    {
        targetItem = item;

        if (itemIcon != null) 
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;

            // [핵심] 원본 이미지 비율을 유지하면서 영역 안에 맞춤
            itemIcon.preserveAspect = true; 

            // 이미지 크기가 영역을 넘어가지 않도록 정방형 고정 (원하는 크기로 조절)
            RectTransform rect = itemIcon.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(250f, 250f); // 팝업 내 이미지 틀 크기
                itemIcon.transform.localScale = Vector3.one; // 스케일 1로 리셋
            }
        }

        if (nameText != null) nameText.text = item.itemName;
        if (descriptionText != null) descriptionText.text = $"{item.description}\n\n<b>이 아이템을 사용하시겠습니까?</b>";

        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(OnConfirmUse);
        }

        if (panel != null) panel.SetActive(true);
    }

    private void OnConfirmUse()
    {
        if (targetItem == null) return;

        // 1. 음식 카테고리인 경우 배고픔 채우기
        CharacterStats character = FindFirstObjectByType<CharacterStats>();
        if (character != null && targetItem.category == ItemCategory.Food)
        {
            character.EatFood(targetItem.hungerRestoreAmount);
        }

        // 2. 인벤토리에서 아이템 1개 차감
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(targetItem, 1);
        }
        
        // 3. 인벤토리 UI 즉시 최신화
        CategoryInventoryUI ui = FindFirstObjectByType<CategoryInventoryUI>();
        if (ui != null) ui.RefreshUI();

        // 4. 팝업 닫기
        if (panel != null) panel.SetActive(false);
    }
}