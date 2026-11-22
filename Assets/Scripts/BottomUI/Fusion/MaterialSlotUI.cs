// MaterialSlotUI.cs  (Slot_0~3에 각각 붙임)
// 슬롯 구조 예:  Slot_0 (배경 Image)
//                 ├ Icon  (Image)   ← 재료 이미지
//                 ├ Name  (TMP)     ← 재료 이름 (선택)
//                 └ Count (TMP)     ← 보유/필요 수량
// 칸 연결이 비어 있어도 위 이름("Icon", "Name", "Count")의 자식을 자동으로 찾아 연결합니다.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;     // 선택 사항
    public TMP_Text countText;

    [Tooltip("true: 보유/필요 (예: 1/2), false: 필요 수량만 (예: x2)")]
    public bool showOwned = true;

    bool wired;

    void AutoWire()
    {
        if (wired) return;
        wired = true;

        if (icon == null)
        {
            Transform t = transform.Find("Icon");
            if (t != null) icon = t.GetComponent<Image>();
            if (icon == null) icon = GetComponent<Image>();   // 자식이 없으면 슬롯 배경 이미지를 아이콘으로 사용
        }

        if (countText == null)
        {
            Transform t = transform.Find("Count");
            if (t == null) t = transform.Find("CountText");
            if (t != null) countText = t.GetComponent<TMP_Text>();
        }

        if (nameText == null)
        {
            Transform t = transform.Find("Name");
            if (t == null) t = transform.Find("NameText");
            if (t != null) nameText = t.GetComponent<TMP_Text>();
        }

        // 그래도 없으면 자식 TMP 하나를 수량 텍스트로 사용
        if (countText == null)
        {
            foreach (var tmp in GetComponentsInChildren<TMP_Text>(true))
            {
                if (tmp != nameText) { countText = tmp; break; }
            }
        }

        if (icon == null) Debug.LogWarning($"[MaterialSlotUI] {name}: Icon(Image) 연결이 없습니다. 자식 'Icon'을 만들거나 칸에 연결하세요.", this);
        if (countText == null) Debug.LogWarning($"[MaterialSlotUI] {name}: Count(TMP) 연결이 없습니다. 자식 'Count'를 만들거나 칸에 연결하세요.", this);
    }

    public void Set(Ingredient ing, int have)
    {
        gameObject.SetActive(true);
        AutoWire();

        bool enough = have >= ing.amount;

        if (icon != null)
        {
            icon.sprite = (ing.item != null) ? ing.item.icon : null;
            icon.enabled = icon.sprite != null;
            icon.preserveAspect = true;
            icon.color = Color.white;
        }

        if (nameText != null)
            nameText.text = (ing.item != null) ? ing.item.itemName : "";

        if (countText != null)
        {
            countText.text = showOwned ? $"{have}/{ing.amount}" : $"x{ing.amount}";
            countText.color = enough ? Color.white : Color.red;  // 부족하면 빨강
        }
    }

    public void Clear() => gameObject.SetActive(false);
}