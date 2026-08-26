using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI quantityText;

    private ItemData currentItem;
    private int itemCount;

    public void SetItem(ItemData item, int count)
    {
        currentItem = item;
        itemCount = count;

        if (item != null && count > 0)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;

            // [추가 1] 원본 이미지 비율 유지 (이미지 눌림/일그러짐 방지)
            iconImage.preserveAspect = true;

            // [추가 2] ItemData에 설정한 itemScale 값 적용
            RectTransform rect = iconImage.GetComponent<RectTransform>();
            if (rect != null)
            {
                float scale = item.itemScale;
                rect.localScale = new Vector3(scale, scale, 1.0f);
            }

            if (quantityText != null)
                quantityText.text = count > 1 ? count.ToString() : "";
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        itemCount = 0;
        if (iconImage != null) iconImage.enabled = false;
        if (quantityText != null) quantityText.text = "";
    }
}