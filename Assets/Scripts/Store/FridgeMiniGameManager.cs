using System.Collections.Generic;
using UnityEngine;

public class FridgeMiniGameManager : MonoBehaviour
{
    public enum GameState { Overview, Closeup }

    [Header("UI 및 참조")]
    public GameObject overviewRoot;
    public GameObject closeButton;
    public InventoryUI inventoryUI;

    [Header("현재 상태")]
    public GameState currentState = GameState.Overview;

    private Dictionary<ItemData, int> inventory = new Dictionary<ItemData, int>();
    private GameObject currentCloseupView;

    void Start()
    {
        if (closeButton != null) closeButton.SetActive(false);
        if (overviewRoot != null) overviewRoot.SetActive(true);
        currentState = GameState.Overview;
    }

    // 클로즈업 창 열기 (냉장고 내부 등)
    public void OpenCloseup(GameObject closeupView)
    {
        currentState = GameState.Closeup;
        currentCloseupView = closeupView;

        if (overviewRoot != null) overviewRoot.SetActive(false);
        if (currentCloseupView != null) currentCloseupView.SetActive(true);
        if (closeButton != null) closeButton.SetActive(true);
    }

    // 클로즈업 창 닫기
    public void ExitCloseup()
    {
        if (currentCloseupView != null) currentCloseupView.SetActive(false);
        currentCloseupView = null;

        if (closeButton != null) closeButton.SetActive(false);
        if (overviewRoot != null) overviewRoot.SetActive(true);

        currentState = GameState.Overview;
    }

    // 인벤토리 아이템 추가 (외부 스크립트에서 호출)
        public void AddToInventory(ItemData item, int count = 1)
        {
            if (item == null) return;

            // [수정된 부분] 메인 씬과 공유되는 싱글톤 인벤토리 매니저에 아이템 저장
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(item, count);
            }

            // 기존 씬 내부 UI 업데이트가 필요하다면 유지 (필요 없는 경우 제거 가능)
            if (inventory.ContainsKey(item)) inventory[item] += count;
            else inventory[item] = count;

            if (inventoryUI != null) inventoryUI.UpdateUI(inventory);
        }

    // 가중치(Weight) 기반 랜덤 아이템 뽑기 유틸리티
    public ItemData GetRandomItem(List<ItemData> possibleFoods)
    {
        if (possibleFoods == null || possibleFoods.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var f in possibleFoods) totalWeight += f.weight;

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var f in possibleFoods)
        {
            cumulative += f.weight;
            if (rand <= cumulative) return f;
        }
        return null;
    }
}