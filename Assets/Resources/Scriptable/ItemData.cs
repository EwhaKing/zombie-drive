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
    public ItemCategory category; 
    public float weight; 

    // [추가] 각 아이템의 크기 비율 (기본값 1.0)
    public float itemScale = 1.0f; 
}