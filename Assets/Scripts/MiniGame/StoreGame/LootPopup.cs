using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 재료 획득 / 미획득 / 가방 가득참 공용 알림 팝업
// 주의: 이 스크립트는 '항상 활성화된' 오브젝트에 붙이고,
//       panel에는 그 자식(팝업 창)을 연결하세요. 스크립트 오브젝트가 꺼져 있으면 Instance가 null이 됩니다.
public class LootPopup : MonoBehaviour
{
    public static LootPopup Instance;

    public GameObject panel;
    public Image itemIcon;               // 획득 시에만 표시 (없어도 됨)
    public TextMeshProUGUI messageText;
    public Button confirmButton;         // 확인 버튼

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
        if (confirmButton != null) confirmButton.onClick.AddListener(Hide);
    }

    // "ㅇㅇㅇ 획득!"
    public void ShowAcquired(ItemData item, int count = 1)
    {
        string countText = count > 1 ? $" x{count}" : "";
        Show($"{item.itemName}{countText} 획득!", item.icon);
    }

    // "아무것도 발견하지 못했습니다. 좀비 습격 확률 5% 증가"
    public void ShowNothing(float zombieRiskIncrease)
    {
        Show($"아무것도 발견하지 못했습니다.\n좀비 습격 확률 {zombieRiskIncrease:0.##}% 증가", null);
    }

    public void ShowFull()
    {
        Show("가방이 가득찼습니다!\n가방에서 아이템을 버린 후 다시 시도하세요.", null);
    }

    // 임의 문구 표시 (예: "이미 확인한 곳입니다.")
    public void ShowMessage(string message)
    {
        Show(message, null);
    }

    void Show(string message, Sprite icon)
    {
        if (messageText != null) messageText.text = message;

        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(icon != null);
            if (icon != null)
            {
                itemIcon.sprite = icon;
                itemIcon.preserveAspect = true;
            }
        }

        if (panel != null) panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}