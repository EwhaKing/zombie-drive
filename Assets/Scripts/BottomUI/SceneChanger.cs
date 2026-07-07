using UnityEngine;
using UnityEngine.UI; // 버튼(Button) 컴포넌트를 쓰기 위해 필수!
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // [SerializeField]를 쓰면 유니티 인스펙터 창에서 버튼을 직접 등록할 수 있게 해줍니다.
    [SerializeField] private Button navigationButton; 

    private void Start()
    {
        // 버튼이 정상적으로 연결되어 있다면, 클릭했을 때 이동하는 기능을 코드로 연결합니다.
        if (navigationButton != null)
        {
            navigationButton.onClick.AddListener(ChangeToTestScene);
        }
    }

    private void ChangeToTestScene()
    {
        SceneManager.LoadScene("Test");
    }
}