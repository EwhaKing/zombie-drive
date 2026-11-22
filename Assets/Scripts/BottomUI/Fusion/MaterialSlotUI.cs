// MaterialSlotUI.cs  (Slot_0~3에 각각 붙임)
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text countText;

    public void Set(Ingredient ing, int have)
    {
        gameObject.SetActive(true);

        if (icon != null)
        {
            icon.sprite = (ing.item != null) ? ing.item.icon : null;
            icon.enabled = icon.sprite != null;
            icon.preserveAspect = true;
        }

        if (countText != null)
        {
            countText.text = $"{have}/{ing.amount}";
            countText.color = have >= ing.amount ? Color.white : Color.red;  // 부족하면 빨강
        }
    }

    public void Clear() => gameObject.SetActive(false);
}