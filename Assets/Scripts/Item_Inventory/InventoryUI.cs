using System.Collections.Generic;
using UnityEngine;

// 파밍씬 가방 UI (고정 슬롯). 데이터는 FarmingBag가 들고 있고, 이 스크립트는 '그리기'만 담당.
public class InventoryUI : MonoBehaviour
{
    public Transform slotContainer;   // 슬롯(SlotPrefab, ItemSlot 포함)들의 부모

    [Header("수량 숫자 설정")]
    public float numberFontSize = 24f;
    public Vector2 numberBoxSize = new Vector2(60f, 30f);
    public Vector2 numberOffset = new Vector2(-4f, 2f);

    // 화면에 있는 슬롯 개수 = 가방 용량
    public int SlotCount => slotContainer != null ? slotContainer.childCount : 0;

    void OnEnable()
    {
        // 가방을 열 때마다 최신 상태로 다시 그림
        if (FarmingBag.Instance != null) FarmingBag.Instance.RefreshUI();
    }

    public void ToggleBag()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void Refresh(IReadOnlyList<FarmingBag.Entry> entries)
    {
        if (slotContainer == null) return;

        for (int i = 0; i < slotContainer.childCount; i++)
        {
            ItemSlot slot = slotContainer.GetChild(i).GetComponent<ItemSlot>();
            if (slot == null) continue;

            if (i < entries.Count)
            {
                // [핵심 수정] 아이콘을 직접 바꾸지 않고 ItemSlot.SetItem을 거치게 함
                // → ItemSlot이 '어떤 아이템인지' 알게 되어 클릭 시 팝업이 정상적으로 뜸
                slot.SetItem(entries[i].item, entries[i].count);
                ApplyNumberStyle(slot.quantityText);
            }
            else
            {
                slot.ClearSlot();
            }
        }
    }

    void ApplyNumberStyle(TMPro.TextMeshProUGUI txt)
    {
        if (txt == null) return;

        txt.enableAutoSizing = false;
        txt.fontSize = numberFontSize;
        txt.alignment = TMPro.TextAlignmentOptions.BottomRight;

        RectTransform rt = txt.rectTransform;
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.sizeDelta = numberBoxSize;
        rt.anchoredPosition = numberOffset;
    }
}