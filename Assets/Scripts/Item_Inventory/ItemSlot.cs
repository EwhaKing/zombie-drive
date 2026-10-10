using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TMPro.TextMeshProUGUI quantityText;


    // [추가] 슬롯 테두리와 아이콘 사이 여백 (너무 꽉 차 보이면 올리세요)
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

                // [변경] 고정 sizeDelta(100x100) 삭제 → 슬롯 크기에 맞춰 꽉 채우도록 stretch 앵커 사용
                // 슬롯이 어떤 크기·해상도여도 아이콘이 슬롯 안에 들어옴
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = new Vector2(iconPadding, iconPadding);   // 왼쪽/아래 여백
                rect.offsetMax = new Vector2(-iconPadding, -iconPadding); // 오른쪽/위 여백

                // [변경] itemScale 대신 슬롯 전용 slotScale 사용
                // 슬롯 UI에서 아이템마다 크기를 따로 조절 가능 (냉장고의 itemScale에 영향 없음)
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
        Debug.Log($"[슬롯 클릭됨] 클릭한 아이템: {(currentItem != null ? currentItem.itemName : "없음")}");

        if (currentItem == null || itemCount <= 0) return;

        if (ItemUsePopUp.Instance == null)
        {
            Debug.LogError("ItemUsePopUp.Instance가 null, 팝업 오브젝트나 스크립트를 확인 필요");
            return;
        }

        ItemUsePopUp.Instance.ShowPopUp(currentItem);
    }
}