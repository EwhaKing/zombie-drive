using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform slotContainer;

    // [추가] 수량 숫자 스타일 (이 스크립트를 쓰는 Store씬에만 적용, Driving씬은 영향 없음)
    [Header("수량 숫자 설정")]
    public float numberFontSize = 24f;                         // 숫자 크기 (더 줄이려면 값을 낮추세요)
    public Vector2 numberBoxSize = new Vector2(60f, 30f);      // 숫자 영역 크기
    public Vector2 numberOffset = new Vector2(-4f, 2f);        // 슬롯 오른쪽 아래 모서리에서 안쪽으로 띄울 거리

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

            // Icon 세팅 (기존과 동일)
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

                    // [추가] 숫자 크기와 위치 스타일 적용
                    ApplyNumberStyle(txt);
                }
            }

            index++;
        }
    }

    // [추가] 숫자를 작게 + 슬롯 오른쪽 아래에 배치하는 함수
    void ApplyNumberStyle(TMPro.TextMeshProUGUI txt)
    {
        // 프리팹의 Auto Size를 끄고 인스펙터 값으로 고정 크기 사용
        txt.enableAutoSizing = false;
        txt.fontSize = numberFontSize;
        // 글자를 영역의 오른쪽 아래로 정렬
        txt.alignment = TMPro.TextAlignmentOptions.BottomRight;

        // 앵커/피벗을 슬롯의 오른쪽 아래 모서리로 지정
        RectTransform rt = txt.rectTransform;
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.sizeDelta = numberBoxSize;
        rt.anchoredPosition = numberOffset;
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