using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// 계산대 통 / 바닥 박스 등 "클릭하면 확률로 재료 획득" 오브젝트용
// 클릭할 영역의 Image(Raycast Target ON, 투명해도 됨)에 붙이세요.
public class ChanceLootContainer : MonoBehaviour, IPointerClickHandler
{
    [Header("획득 설정")]
    public List<ItemData> possibleItems;
    [Range(0f, 1f)] public float successChance = 0.5f;

    [Header("사용 후 처리")]
    public bool oneTimeOnly = true;                       // true: 한 번 확인하면 끝 / false: 계속 클릭 가능
    public string alreadyUsedMessage = "이미 확인한 곳입니다.";
    public bool tintWhenUsed = false;                     // 눈에 보이는 이미지일 때만 켜세요 (투명 클릭영역이면 끄기)
    public Image targetImage;                             // 색을 바꿀 이미지 (비워두면 자기 Image)
    public Color usedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("꽝 처리")]
    public float zombieRiskIncrease = 5f;                 // 팝업에 표시되는 증가 수치(%)
    // 좀비 습격 확률 시스템과 인스펙터에서 연결 (인자: zombieRiskIncrease). 비워두면 팝업 문구만 나옴
    public UnityEvent<float> onMissed;

    private bool used = false;
    private bool rolled = false;             // 결과가 이미 정해졌는지
    private ItemData rolledItem = null;      // 정해진 결과 (null = 꽝)

    public void OnPointerClick(PointerEventData eventData)
    {
        if (used)
        {
            if (LootPopup.Instance != null) LootPopup.Instance.ShowMessage(alreadyUsedMessage);
            return;
        }

        // 1) 결과는 처음 클릭 때 한 번만 결정 → 가방이 가득 차서 실패해도
        //    계속 눌러서 결과를 다시 뽑는(리롤) 꼼수를 방지
        if (!rolled)
        {
            rolledItem = (Random.value < successChance) ? LootService.PickWeighted(possibleItems) : null;
            rolled = true;
        }

        // 2) 꽝
        if (rolledItem == null)
        {
            if (LootPopup.Instance != null) LootPopup.Instance.ShowNothing(zombieRiskIncrease);
            onMissed?.Invoke(zombieRiskIncrease);
            Finish();
            return;
        }

        // 3) 성공 → 가방에 추가 시도
        LootResult result = LootService.TryAddToInventory(rolledItem);

        if (result == LootResult.Success)
        {
            if (LootPopup.Instance != null) LootPopup.Instance.ShowAcquired(rolledItem);
            Finish();
        }
        else if (result == LootResult.InventoryFull)
        {
            // 소모하지 않음: 가방을 비우고 다시 클릭하면 같은 아이템 획득
            if (LootPopup.Instance != null) LootPopup.Instance.ShowFull();
        }
    }

    void Finish()
    {
        if (oneTimeOnly)
        {
            used = true;
            if (tintWhenUsed)
            {
                Image img = targetImage != null ? targetImage : GetComponent<Image>();
                if (img != null) img.color = usedColor;
            }
        }
        else
        {
            rolled = false;
            rolledItem = null;
        }
    }
}