using UnityEngine;

public class RepairShopScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject whitePanel;
    [SerializeField] private WheelRemovalMiniGame wheelMiniGame;
    [SerializeField] private RepairShopGameManager gameManager;

    public void OpenWhiteScreen()
    {
        whitePanel.SetActive(true);

        wheelMiniGame.ResetMiniGame();

        gameManager.BeginRepair();
    }

    public void CloseWhiteScreen()
    {
        whitePanel.SetActive(false);
    }
}