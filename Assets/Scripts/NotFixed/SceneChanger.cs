using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "YourSceneName";

    public void ChangeScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}