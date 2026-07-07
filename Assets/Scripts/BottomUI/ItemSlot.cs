using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image itemIconImage;     // 아이콘이 들어갈 Image 컴포넌트
    [SerializeField] private TextMeshProUGUI itemNameText; // 이름이 들어갈 Text 컴포넌트

    // 이 함수를 실행하면 네모칸의 그림과 글자가 바뀝니다.
    public void Setup(Item item)
    {
        itemNameText.text = item.itemName;
        itemIconImage.sprite = item.itemIcon;
    }
}