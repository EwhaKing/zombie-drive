using UnityEngine;

public class CharacterBottomUI : MonoBehaviour
{
    [Header("캐릭터 탭 패널")]
    public GameObject characterTabPanel; // 캐릭터 & 스탯 패널

    void Start()
    {
        // 시작 시 캐릭터 탭은 닫아둡니다.
        if (characterTabPanel != null)
            characterTabPanel.SetActive(false);
    }

    // 하단 UI의 '캐릭터' 버튼 OnClick에 이 함수 하나만 연결하세요!
    public void ToggleCharacterTab()
    {
        if (characterTabPanel != null)
        {
            // 현재 상태가 true(열림)이면 false(닫힘)로, false이면 true로 뒤집습니다.
            bool currentState = characterTabPanel.activeSelf;
            characterTabPanel.SetActive(!currentState);
        }
    }
}