using UnityEngine;

public enum ItemCategory
{
    Food,
    Material,
    Medicine,
    Furniture,
    Clothes
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public ItemCategory category; 
    public float weight; 
    public float itemScale = 1.0f;            // 냉장고 미니게임용 (기존)
    // [추가] 슬롯 UI 전용 배율. 1 = 슬롯을 꽉 채움, 0.8 = 80% 크기
    public float slotScale = 1.0f;
    public float hungerRestoreAmount = 20f;
}