using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private Button navigationButton;

    // 이 버튼이 어느 씬으로 갈지 Inspector에서 지정할 수 있게 변수로 뺌
    [SerializeField] private string targetSceneName;

    private void Start()
    {
        if (navigationButton != null)
        {
            navigationButton.onClick.AddListener(ChangeScene);
        }
    }

    private void ChangeScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}