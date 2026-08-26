using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToDrivingButton : MonoBehaviour
{
    public void ReturnToDriving()
    {
        // 정상적으로 DrivingScene에서 넘어온 경우
        if (DrivingManager.Instance != null)
        {
            DrivingManager.Instance.ReturnFromDestination();
        }
        else
        {
            // 해당 미니게임 씬만 단독으로 실행해서 테스트했을 경우
            Debug.LogWarning(
                "DrivingManager가 없습니다. 테스트용으로 DrivingScene으로 직접 이동합니다."
            );

            SceneManager.LoadScene("DrivingScene");
        }
    }
}