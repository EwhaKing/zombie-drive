using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TMPro.TextMeshProUGUI quantityText;

    // 슬롯 테두리와 아이콘 사이 여백
    public float iconPadding = 6f;

    private ItemData currentItem;
    private int itemCount;

    public void SetItem(ItemData item, int count)
    {
        currentItem = item;
        itemCount = count;

        if (item != null && count > 0)
        {
            if (iconImage != null)
            {
                iconImage.sprite = item.icon;
                iconImage.enabled = true;
                iconImage.preserveAspect = true;

                RectTransform rect = iconImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = new Vector2(iconPadding, iconPadding);
                rect.offsetMax = new Vector2(-iconPadding, -iconPadding);

                float scale = Mathf.Clamp(item.slotScale, 0.1f, 1f);
                rect.localScale = new Vector3(scale, scale, 1.0f);
            }

            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(true);
                quantityText.text = count.ToString();
            }
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

        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
            iconImage.transform.localScale = Vector3.one;
        }

        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }
    }

    // 슬롯 클릭 이벤트
    public void OnPointerClick(PointerEventData eventData)
    {
        // [수정] 빈 슬롯은 아무 동작/로그 없이 무시 (기존엔 빈 슬롯 클릭에도 "없음" 로그가 떴음)
        if (currentItem == null || itemCount <= 0) return;

        if (ItemUsePopUp.Instance == null)
        {
            Debug.LogError("ItemUsePopUp.Instance가 null, 팝업 오브젝트나 스크립트를 확인 필요");
            return;
        }

        ItemUsePopUp.Instance.ShowPopUp(currentItem);
    }
}