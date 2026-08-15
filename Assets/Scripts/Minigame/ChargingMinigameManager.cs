using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ChargingMinigameManager : MonoBehaviour
{
    public TMP_Text timerText;
    private float timeRemaining = 30f;
    private bool isGameActive = true;

    void Update()
    {
        if (!isGameActive) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndGame();
        }

        timerText.text = Mathf.CeilToInt(timeRemaining).ToString() + "초";
    }

   public ChargingGameLogic gameLogic; // Inspector에서 연결

void EndGame()
{
    isGameActive = false;
    float finalGauge = gameLogic.GetGaugeValue();
    Debug.Log("30초 경과 - 미니게임 종료. 최종 게이지: " + finalGauge);
    // TODO: 나중에 이 finalGauge 값을 실제 배터리 시스템에 전달
    Screen.orientation = ScreenOrientation.Portrait;
    SceneManager.LoadScene("Farming");
}
    void Awake()
{
    Screen.orientation = ScreenOrientation.LandscapeLeft;
}


}