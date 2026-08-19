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