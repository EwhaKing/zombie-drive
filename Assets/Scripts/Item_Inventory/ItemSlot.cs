using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TMPro.TextMeshProUGUI quantityText;

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

                RectTransform rect = iconImage.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(100f, 100f); // 원하는 기본 크기
                    float scale = item.itemScale;
                    rect.localScale = new Vector3(scale, scale, 1.0f);
                }
            }

            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(true);
                // 1개 이상이면 수량 출력
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
        Debug.Log($"[슬롯 클릭됨] 클릭한 아이템: {(currentItem != null ? currentItem.itemName : "없음")}");

        if (currentItem == null || itemCount <= 0) return;

        if (ItemUsePopUp.Instance == null)
        {
            Debug.LogError("ItemUsePopUp.Instance가 null입니다! 팝업 오브젝트나 스크립트를 확인하세요.");
            return;
        }

        ItemUsePopUp.Instance.ShowPopUp(currentItem);
    }
}