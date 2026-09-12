using UnityEngine;

public class CharacterBottomUI : MonoBehaviour
{
    [Header("캐릭터 탭 패널")]
    public GameObject characterTabPanel; // 캐릭터 & 스탯 패널

    void Start()
    {
        // 시작 시 캐릭터 탭은 닫음
        if (characterTabPanel != null)
            characterTabPanel.SetActive(false);
    }

    public void ToggleCharacterTab()
    {
        if (characterTabPanel != null)
        {
            bool currentState = characterTabPanel.activeSelf;
            characterTabPanel.SetActive(!currentState);
        }
    }
}