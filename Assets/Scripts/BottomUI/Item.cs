using UnityEngine;

// 카테고리 종류를 명확하게 정의합니다.
public enum ItemCategory
{
    Food,      // 0: 식량
    Material,  // 1: 재료 (보내주신 사진의 '재료' 탭 맞춤)
    Furniture, // 2: 가구
    Clothes    // 3: 옷
}

[System.Serializable] // 이래야 유니티 창에서 아이템 정보를 직접 입력할 수 있어요!
public class Item
{
    public string itemName;       // 아이템 이름
    public ItemCategory category; // 카테고리 종류
    public Sprite itemIcon;       // 아이템 이미지
}