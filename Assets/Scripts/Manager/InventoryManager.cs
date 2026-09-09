using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // 아이템과 수량을 저장하는 딕셔너리
    public Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();

    private void Awake()
    {
        // 1. Instance가 비어있다면 자기 자신을 static 변수에 할당
        if (Instance == null)
        {
            Instance = this;
            // GameManager가 최상위 오브젝트가 아니라면 부모를 해제하여 Root로 생성
            transform.SetParent(null); 
            DontDestroyOnLoad(gameObject);
        }
        // 2. 이미 Instance가 존재하고, 그게 자기 자신이 아니라면 (중복 생성된 경우)
        else if (Instance != this)
        {
            // 오브젝트(gameObject) 전체를 지우지 않고, 중복 추가된 '스크립트 컴포넌트'만 삭제
            Destroy(this);
            return;
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

    public void RemoveItem(ItemData item, int amount = 1)
    {
        if (item == null || !items.ContainsKey(item)) return;

        items[item] -= amount;

        // 수량이 0 이하가 되면 딕셔너리에서 제거
        if (items[item] <= 0)
        {
            items.Remove(item);
        }
    }


}