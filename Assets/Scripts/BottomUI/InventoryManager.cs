using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // 아이템과 수량을 저장하는 딕셔너리
    public Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 변경되어도 데이터 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 아이템 획득 시 호출
    public void AddItem(ItemData item, int amount = 1)
    {
        if (item == null) return;

        if (items.ContainsKey(item))
            items[item] += amount;
        else
            items.Add(item, amount);
    }
}