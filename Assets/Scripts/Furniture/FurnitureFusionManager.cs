using UnityEngine;
using UnityEngine.SceneManagement;

public class FurnitureFusionManager : MonoBehaviour
{
    public void GoToFusionScene()
    {
        SceneManager.LoadScene("FurnitureFusion");
    }

    public void BackToDriving()
    {
        SceneManager.LoadScene("DrivingScene");
    }
}