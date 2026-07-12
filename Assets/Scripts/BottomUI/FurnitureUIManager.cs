using UnityEngine;

public class FurnitureUIManager : MonoBehaviour
{
    public GameObject furnitureButton;
    public GameObject placementButton;
    public GameObject fusionButton;

    public void OnFurnitureButton()
    {
        furnitureButton.SetActive(false);
        placementButton.SetActive(true);
        fusionButton.SetActive(true);
    }
}