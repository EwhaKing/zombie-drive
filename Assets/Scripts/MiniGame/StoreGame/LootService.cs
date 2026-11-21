using System.Collections.Generic;
using UnityEngine;

public enum LootResult
{
    Success,       // 가방에 추가됨
    InventoryFull, // 가방이 가득 참
    Invalid        // 아이템 null / FarmingBag 없음
}

// 파밍씬의 모든 획득(선반, 계산대 통, 박스, 냉장고, 더미)이 공통으로 쓰는 로직
public static class LootService
{
    // 파밍씬에서는 '파밍 가방(FarmingBag)'에 넣는다. (메인 인벤토리로는 복귀 시 이동)
    public static LootResult TryAddToInventory(ItemData item, int count = 1)
    {
        if (item == null) return LootResult.Invalid;

        FarmingBag bag = FarmingBag.Instance;
        if (bag == null)
        {
            Debug.LogError("[LootService] 씬에 FarmingBag가 없습니다.");
            return LootResult.Invalid;
        }

        return bag.TryAdd(item, count);
    }

    // 가중치(weight) 기반 랜덤 뽑기
    public static ItemData PickWeighted(List<ItemData> list)
    {
        if (list == null || list.Count == 0) return null;

        float total = 0f;
        foreach (var f in list) if (f != null) total += f.weight;

        if (total <= 0f) return list[Random.Range(0, list.Count)];

        float rand = Random.Range(0f, total);
        float cumulative = 0f;
        foreach (var f in list)
        {
            if (f == null) continue;
            cumulative += f.weight;
            if (rand <= cumulative) return f;
        }
        return null;
    }
}