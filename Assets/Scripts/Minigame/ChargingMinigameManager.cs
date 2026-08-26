using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ChargingMinigameManager : MonoBehaviour
{
    public TMP_Text timerText;
    public ChargingGameLogic gameLogic;
    public GameObject resultPopup;
    public TMP_Text resultText;
    public GameObject gameplayGroup;

    private float timeRemaining = 30f;
    private bool isGameActive = true;

    void Awake()
    {
        
    }

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
        gameLogic.StopGame();   // ← 추가: 게임 로직도 멈추기
        float finalGauge = gameLogic.GetGaugeValue();
        Debug.Log("30초 경과 - 미니게임 종료. 최종 게이지: " + finalGauge);

        gameplayGroup.SetActive(false);   // ← 추가: 게임 화면 전체 숨기기

        // 결과 팝업 띄우기 (씬 전환은 여기서 안 함)
        int percentage = Mathf.RoundToInt(finalGauge * 100);
        resultText.text = "충전 완료!\n게이지: " + percentage + "%";
        resultPopup.SetActive(true);
    }

    // "확인" 버튼에 연결할 함수
    public void OnConfirmClick()
    {
        SceneManager.LoadScene("Farming");
    }
}