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
    public float itemScale = 1.0f;
    public float hungerRestoreAmount = 20f;
}