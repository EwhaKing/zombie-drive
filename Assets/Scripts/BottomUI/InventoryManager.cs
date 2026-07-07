using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TMP_InputField searchInputField; // 검색창
    [SerializeField] private Transform contentParent;          // Scroll View의 Content
    [SerializeField] private GameObject itemSlotPrefab;       // 아까 만든 ItemSlot 프리팹

    [Header("보유 중인 아이템 리스트")]
    public List<Item> playerOwnedItems = new List<Item>(); 

    private ItemCategory currentCategory = ItemCategory.Food; // 기본 탭은 '식량'
    private string currentSearchQuery = "";                   // 현재 검색어

    private void Start()
    {
        // 검색창에 글자를 타이핑할 때마다 자동으로 화면을 새로고침하라는 규칙을 세웁니다.
        searchInputField.onValueChanged.AddListener(OnSearchValueChanged);
        
        // 처음 시작할 때 인벤토리 UI를 한번 그려줍니다.
        RefreshInventoryUI();

        // 셋업이 모두 끝났으니, 게임 시작 시 인벤토리 창을 자동으로 닫아서 숨깁니다.
        gameObject.SetActive(false);
    }

    // 카테고리 탭 버튼을 누르면 실행될 함수
    public void ChangeCategory(int categoryIndex)
    {
        currentCategory = (ItemCategory)categoryIndex;
        RefreshInventoryUI(); // 탭이 바뀌었으니 새로고침!
    }

    // 검색창 글자가 바뀔 때 실행될 함수
    private void OnSearchValueChanged(string value)
    {
        currentSearchQuery = value;
        RefreshInventoryUI(); // 검색어가 바뀌었으니 새로고침!
    }

    // 마인크래프트처럼 필터링해서 화면에 뿌려주는 핵심 함수
    public void RefreshInventoryUI()
    {
        // 1. 이미 눈에 보이고 있는 슬롯들을 싹 지워서 청소합니다.
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 내가 가진 전체 아이템을 하나씩 검사합니다.
        foreach (Item item in playerOwnedItems)
        {
            // 조건 1: 지금 켜진 탭의 카테고리와 일치하는가?
            bool matchCategory = (item.category == currentCategory);
            
            // 조건 2: 검색창이 비어있거나, 아이템 이름에 검색어가 포함되어 있는가?
            bool matchSearch = string.IsNullOrEmpty(currentSearchQuery) || 
                               item.itemName.Contains(currentSearchQuery);

            // 둘 다 맞으면 화면에 슬롯을 하나 만들어라!
            if (matchCategory && matchSearch)
            {
                GameObject newSlot = Instantiate(itemSlotPrefab, contentParent);
                ItemSlot slotScript = newSlot.GetComponent<ItemSlot>();
                slotScript.Setup(item);
            }
        }
    }

    // 하단 메인 UI에서 인벤토리 버튼 누르면 창을 켜주는 함수
    public void OpenInventory()
    {
        gameObject.SetActive(true);
        RefreshInventoryUI();
    }

    // X 버튼 누르면 창을 닫는 함수
    public void CloseInventory()
    {
        gameObject.SetActive(false);
    }
}