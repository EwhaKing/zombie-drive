using System.Collections.Generic;
using UnityEngine;

// 파밍씬 전용 '임시 가방'.
// - 슬롯 개수(용량)가 정해져 있어서 가득 차면 획득 불가 → 버려야 함
// - 파밍씬에서 나갈 때 메인 InventoryManager(무제한, 카테고리별 스크롤)로 한꺼번에 옮겨짐
// 항상 활성화된 오브젝트에 붙이세요.
public class FarmingBag : MonoBehaviour
{
    public static FarmingBag Instance { get; private set; }

    public class Entry
    {
        public ItemData item;
        public int count;
    }

    [Header("UI")]
    public InventoryUI inventoryUI;            // 파밍씬 가방 UI

    [Header("용량")]
    public bool useUISlotCountAsCapacity = true; // true: inventoryUI의 슬롯 개수 = 용량 (UI와 항상 일치)
    public int manualCapacity = 16;              // 위가 false일 때만 사용
    // false(기획서 그대로): 슬롯이 다 찼으면 이미 가진 아이템이라도 획득 불가
    // true : 슬롯이 다 찼어도 이미 가진 종류는 수량만 늘려서 획득 가능
    public bool allowStackingWhenFull = false;

    [Header("복귀")]
    public bool autoCommitOnDestroy = true;      // 복귀 버튼을 안 거치고 씬이 닫혀도 아이템을 잃지 않게 하는 안전장치

    private readonly List<Entry> entries = new List<Entry>();
    public IReadOnlyList<Entry> Entries => entries;

    public int Capacity
    {
        get
        {
            if (useUISlotCountAsCapacity && inventoryUI != null && inventoryUI.SlotCount > 0)
                return inventoryUI.SlotCount;
            return Mathf.Max(1, manualCapacity);
        }
    }

    public int UsedSlots => entries.Count;
    public bool IsFull => entries.Count >= Capacity;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RefreshUI();
    }

    void OnDestroy()
    {
        if (autoCommitOnDestroy) Commit(false);
        if (Instance == this) Instance = null;
    }

    Entry Find(ItemData item)
    {
        foreach (var e in entries) if (e.item == item) return e;
        return null;
    }

    // 아이템 획득 시도
    public LootResult TryAdd(ItemData item, int count = 1)
    {
        if (item == null || count <= 0) return LootResult.Invalid;

        Entry existing = Find(item);

        if (existing != null)
        {
            if (IsFull && !allowStackingWhenFull) return LootResult.InventoryFull;
            existing.count += count;
        }
        else
        {
            if (IsFull) return LootResult.InventoryFull;
            entries.Add(new Entry { item = item, count = count });
        }

        RefreshUI();
        return LootResult.Success;
    }

    // 버리기. amount가 보유 수량 이상이면 슬롯이 비워짐
    public void Discard(ItemData item, int amount)
    {
        Entry e = Find(item);
        if (e == null || amount <= 0) return;

        if (amount >= e.count) entries.Remove(e);
        else e.count -= amount;

        RefreshUI();
    }

    // 복귀 버튼(OnClick) 또는 씬 전환 직전에 호출 → 메인 인벤토리로 이동
    public void CommitToMainInventory()
    {
        Commit(true);
    }

    void Commit(bool refresh)
    {
        if (entries.Count == 0) return;

        InventoryManager inv = InventoryManager.Instance;
        if (inv == null)
        {
            Debug.LogWarning("[FarmingBag] InventoryManager가 없어 아이템을 옮기지 못했습니다.");
            return;
        }

        foreach (var e in entries) inv.AddItem(e.item, e.count);
        entries.Clear();

        if (refresh) RefreshUI();
    }

    public void RefreshUI()
    {
        if (inventoryUI != null) inventoryUI.Refresh(entries);
    }
}