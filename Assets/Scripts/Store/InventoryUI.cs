using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform slotContainer;
    private Dictionary<ItemData, int> currentInventoryData;

    void Awake()
    {
        ClearUI();
    }

    public void ToggleBag()
    {
        bool nextState = !gameObject.activeSelf;
        gameObject.SetActive(nextState);

        if (nextState && currentInventoryData != null)
        {
            UpdateUI(currentInventoryData);
        }
    }

    public void UpdateUI(Dictionary<ItemData, int> inventory)
    {
        currentInventoryData = inventory;
        ClearUI();

        if (slotContainer == null) return;

        int index = 0;
        foreach (var pair in inventory)
        {
            if (index >= slotContainer.childCount) break;

            Transform slot = slotContainer.GetChild(index);

            // Icon 세팅
            Transform iconTransform = slot.Find("Icon");
            if (iconTransform != null)
            {
                var img = iconTransform.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.sprite = pair.Key.icon;
                    img.preserveAspect = true;
                    iconTransform.gameObject.SetActive(true);
                }
            }

            // 수량(number) 텍스트 세팅
            Transform textTransform = slot.Find("number");
            if (textTransform != null)
            {
                var txt = textTransform.GetComponent<TMPro.TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = pair.Value.ToString();
                    textTransform.gameObject.SetActive(true);
                }
            }

            index++;
        }
    }

    public void ClearUI()
    {
        if (slotContainer == null) return;

        foreach (Transform child in slotContainer)
        {
            // Icon, number 및 기본 Name/Text 등 모든 자식 UI 컴포넌트 비활성화
            foreach (Transform subChild in child)
            {
                subChild.gameObject.SetActive(false);
            }
        }
    }
}