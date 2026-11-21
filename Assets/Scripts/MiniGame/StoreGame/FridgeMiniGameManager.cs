using System.Collections.Generic;
using UnityEngine;

public class FridgeMiniGameManager : MonoBehaviour
{
    public enum GameState { Overview, Closeup }

    [Header("UI 및 참조")]
    public GameObject overviewRoot;
    public GameObject closeButton;
    // [제거] inventoryUI → 이제 FarmingBag가 가방 UI를 관리함

    [Header("현재 상태")]
    public GameState currentState = GameState.Overview;

    private GameObject currentCloseupView;

    void Start()
    {
        if (closeButton != null) closeButton.SetActive(false);
        if (overviewRoot != null) overviewRoot.SetActive(true);
        currentState = GameState.Overview;
    }

    public void OpenCloseup(GameObject closeupView)
    {
        currentState = GameState.Closeup;
        currentCloseupView = closeupView;

        if (overviewRoot != null) overviewRoot.SetActive(false);
        if (currentCloseupView != null) currentCloseupView.SetActive(true);
        if (closeButton != null) closeButton.SetActive(true);
    }

    public void ExitCloseup()
    {
        if (currentCloseupView != null) currentCloseupView.SetActive(false);
        currentCloseupView = null;

        if (closeButton != null) closeButton.SetActive(false);
        if (overviewRoot != null) overviewRoot.SetActive(true);

        currentState = GameState.Overview;
    }

    // 아이템 획득 시도. 성공하면 true, 가방이 가득 차면 팝업을 띄우고 false.
    // 호출하는 쪽은 true일 때만 화면의 아이템 오브젝트를 Destroy 해야 함.
    public bool AddToInventory(ItemData item, int count = 1)
    {
        LootResult result = LootService.TryAddToInventory(item, count);

        if (result == LootResult.InventoryFull)
        {
            if (LootPopup.Instance != null) LootPopup.Instance.ShowFull();
            return false;
        }

        return result == LootResult.Success;
    }

    // 기존 호출부(gameManager.GetRandomItem)를 깨지 않기 위해 유지
    public ItemData GetRandomItem(List<ItemData> possibleFoods)
    {
        return LootService.PickWeighted(possibleFoods);
    }
}