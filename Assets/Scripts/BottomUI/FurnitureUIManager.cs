using UnityEngine;

public class FurnitureUIManager : MonoBehaviour
{
    public GameObject placementButton;
    public GameObject fusionButton;

    public void OnFurnitureButton()
    {
        placementButton.SetActive(true);
        fusionButton.SetActive(true);
    }
}