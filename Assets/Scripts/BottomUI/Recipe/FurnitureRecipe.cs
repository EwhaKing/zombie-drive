// FurnitureRecipe.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Furniture/Recipe")]
public class FurnitureRecipe : ScriptableObject
{
    public string furnitureId;
    public string displayName;           // 비우면 resultItem.itemName 사용
    [TextArea] public string description; // 비우면 resultItem.description 사용
    public Sprite icon;                  // 비우면 resultItem.icon 사용
    public int goldCost;

    [Header("제작 결과 (Category = Furniture 인 ItemData)")]
    public ItemData resultItem;

    [Header("필요 재료 (파밍으로 얻는 Material ItemData)")]
    public List<Ingredient> ingredients;

    public string DisplayName =>
        !string.IsNullOrEmpty(displayName) ? displayName : (resultItem != null ? resultItem.itemName : name);

    public string Description =>
        !string.IsNullOrEmpty(description) ? description : (resultItem != null ? resultItem.description : "");

    public Sprite ResultIcon =>
        icon != null ? icon : (resultItem != null ? resultItem.icon : null);
}

[System.Serializable]
public class Ingredient
{
    public ItemData item;       // 파밍에서 얻는 재료 ItemData (InventoryManager에 저장되는 것과 같은 에셋)
    public int amount = 1;      // 필요 수량
}