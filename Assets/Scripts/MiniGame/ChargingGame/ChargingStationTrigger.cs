using UnityEngine;
using UnityEngine.SceneManagement;

public class ChargingStationTrigger : MonoBehaviour
{
    public void OnChargingStationClick()
    {
        SceneManager.LoadScene("ChargingMinigame");
    }
}