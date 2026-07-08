using UnityEngine;
using UnityEngine.UI;

public class FarmingSceneController : MonoBehaviour
{
    public Button returnButton; // 좌측 상단 "복귀" 버튼

    private void Start()
    {
        returnButton.onClick.AddListener(OnClickReturn);
    }

    private void OnClickReturn()
    {
        // DrivingManager는 DontDestroyOnLoad로 살아있으므로 파밍 씬에서도 그대로 접근 가능
        DrivingManager.Instance.ReturnFromFarming();
    }
}