using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 메인씬/파밍씬 공용 아이템 상세 팝업. 씬마다 하나씩 두고 mode만 다르게 설정.
//  Main    : 이미지 + 설명 + [먹기] [X]
//  Farming : 이미지 + 설명 + [버리기] [X]
public class ItemUsePopUp : MonoBehaviour
{
    public enum PopupMode { Main, Farming }

    public static ItemUsePopUp Instance;

    public PopupMode mode = PopupMode.Main;

    public GameObject panel;
    public Image itemIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Button useButton;       // Main 전용
    public Button discardButton;   // Farming 전용
    public Button cancelButton;

    [Header("아이콘 크기 (씬마다 다르게 설정)")]
    // false: 씬에서 ItemIcon의 Rect Transform을 직접 맞춘 크기 그대로 사용 (권장)
    // true : 아래 iconSize 값으로 코드가 크기를 덮어씀
    public bool overrideIconSize = false;
    public Vector2 iconSize = new Vector2(250f, 250f);

    [Header("버리기 설정 (Farming)")]
    public bool discardWholeStack = true; // true: 해당 아이템 전부 버림 / false: 1개만

    private ItemData targetItem;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
        if (cancelButton != null) cancelButton.onClick.AddListener(Close);
        if (discardButton != null) discardButton.onClick.AddListener(OnConfirmDiscard);
    }

    public void ShowPopUp(ItemData item)
    {
        targetItem = item;

        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
            itemIcon.preserveAspect = true;
            itemIcon.transform.localScale = Vector3.one;

            if (overrideIconSize)
            {
                RectTransform rect = itemIcon.GetComponent<RectTransform>();
                if (rect != null) rect.sizeDelta = iconSize;
            }
        }

        if (nameText != null) nameText.text = item.itemName;

        if (descriptionText != null)
        {
            descriptionText.text = (mode == PopupMode.Main)
                ? $"{item.description}\n\n<b>이 아이템을 사용하시겠습니까?</b>"
                : item.description;
        }

        // 모드에 맞는 버튼만 표시
        if (useButton != null)
        {
            useButton.gameObject.SetActive(mode == PopupMode.Main);
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(OnConfirmUse);
        }
        if (discardButton != null)
        {
            discardButton.gameObject.SetActive(mode == PopupMode.Farming);
        }

        if (panel != null) panel.SetActive(true);
    }

    // ===== 메인씬: 먹기/사용 =====
    private void OnConfirmUse()
    {
        if (targetItem == null) return;

        CharacterStats character = FindFirstObjectByType<CharacterStats>();
        if (character != null && targetItem.category == ItemCategory.Food)
        {
            character.EatFood(targetItem.hungerRestoreAmount);
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(targetItem, 1);
        }

        CategoryInventoryUI ui = FindFirstObjectByType<CategoryInventoryUI>();
        if (ui != null) ui.RefreshUI();

        Close();
    }

    // ===== 파밍씬: 버리기 =====
    private void OnConfirmDiscard()
    {
        if (targetItem == null) return;

        if (FarmingBag.Instance != null)
        {
            FarmingBag.Instance.Discard(targetItem, discardWholeStack ? int.MaxValue : 1);
        }

        Close();
    }

    private void Close()
    {
        if (panel != null) panel.SetActive(false);
    }
}