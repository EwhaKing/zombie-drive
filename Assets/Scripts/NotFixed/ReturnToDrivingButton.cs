using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToDrivingButton : MonoBehaviour
{
    public void ReturnToDriving()
    {
        // 1. 파밍/복귀 시 10~20km 랜덤 거리 차감 실행
        if (GPSManager.Instance != null)
        {
            GPSManager.Instance.ReduceDistanceOnReturn();
        }

        // 2. DrivingScene으로 복귀 처리
        if (DrivingManager.Instance != null)
        {
            DrivingManager.Instance.ReturnFromDestination();
        }
        else
        {
            // DrivingManager가 없는 단독 테스트 환경일 경우 Direct 씬 이동
            Debug.LogWarning(
                "DrivingManager가 없습니다. 테스트용으로 DrivingScene으로 직접 이동합니다."
            );

            SceneManager.LoadScene("DrivingScene");
        }
    }
}