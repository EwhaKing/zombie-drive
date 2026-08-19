using UnityEngine;

public enum ItemCategory
{
    Food,      // 식량
    Material,  // 재료
    Medicine,  // 약
    Furniture, // 가구
    Clothes    // 옷
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemCategory category; 
    public float weight; 
}