using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 노트 팝업 안에 미리 배치해두는 슬롯(STORY 한 칸)에 붙이는 스크립트
// 슬롯을 개수에 따라 오브젝트 배치하고 Slot Index만 0, 1, 2, 3, 4, 5로 각각 다르게 설정
public class NotebookSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("이 슬롯의 번호 (0번부터 시작, 슬롯마다 겹치면 안 됨)")]
    [SerializeField] private int slotIndex;

    [Header("이 슬롯이 처음 채워질 때 붙는 제목")]
    [SerializeField] private string defaultTitle = "STORY 1";

    [Header("연결")]
    [SerializeField] private NotebookManager manager;   
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private Image background;

    [Header("색상")]
    [SerializeField] private Color filledColor = new Color(0.11f, 0.75f, 0.79f); // 채워졌을 때 (청록)
    [SerializeField] private Color emptyColor = new Color(0.62f, 0.62f, 0.62f);  // 비어있을 때 (회색)

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.OnSlotClicked(slotIndex, defaultTitle);
        }
    }

    public void SetData(NotebookEntry entry)
    {
        if (titleText != null)
        {
            titleText.text = entry.title;
        }

        if (dateText != null)
        {
            dateText.gameObject.SetActive(entry.isFilled);
            dateText.text = entry.dateText;
        }

        if (background != null)
        {
            background.color = entry.isFilled ? filledColor : emptyColor;
        }
    }
}