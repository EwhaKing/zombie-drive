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

    void EndGame()
    {
        isGameActive = false;
        Debug.Log("30초 경과 - 미니게임 종료");
        // 여기서 나중에: 게이지 값 저장 + 충전소 화면 복귀
        SceneManager.LoadScene("Farming");
    }
}