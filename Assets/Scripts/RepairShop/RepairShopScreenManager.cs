using UnityEngine;

public class RepairShopScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject whitePanel;
    [SerializeField] private WheelRemovalMiniGame wheelMiniGame;

    public void OpenWhiteScreen()
    {
        whitePanel.SetActive(true);
        wheelMiniGame.ResetMiniGame();
    }

    public void CloseWhiteScreen()
    {
        whitePanel.SetActive(false);
    }
}