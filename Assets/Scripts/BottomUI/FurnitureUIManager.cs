using UnityEngine;

public class FurnitureUIManager : MonoBehaviour
{
    public GameObject furnitureButton;   // 인스펙터 연결은 그대로 둬도 됨 (이제 안 끔)
    public GameObject placementButton;
    public GameObject fusionButton;
    public GameObject fusionPanel;       // 2번에서 만들 합성창 (지금은 비워둬도 됨)

    bool isOpen;

    public void OnFurnitureButton()
    {
        isOpen = !isOpen;
        placementButton.SetActive(isOpen);
        fusionButton.SetActive(isOpen);
    }

    public void OnFusionButton()
    {
        if (fusionPanel != null) fusionPanel.SetActive(true);
    }
}