using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("화면 암전")]
    public CanvasGroup blackScreen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            blackScreen.alpha = 0f;
            blackScreen.blocksRaycasts = false;
            blackScreen.interactable = false;

            // ★ 현재 시작 씬의 화면 방향도 설정
            SetOrientation(SceneManager.GetActiveScene().name);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void GoToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName, null));
    }


    public void GoToSceneWithCallback(string sceneName, Action onLoaded)
    {
        StartCoroutine(FadeAndLoad(sceneName, onLoaded));
    }


    private IEnumerator FadeAndLoad(string sceneName, Action onLoaded)
    {
        // 1. 화면 어둡게
        yield return Fade(0f, 1f, 0.5f);

        // ★ 2. 이동할 씬에 맞게 화면 방향 변경
        SetOrientation(sceneName);

        // 화면 방향 변경이 적용될 시간을 한 프레임 줌
        yield return null;

        // 3. 씬 로드
        yield return SceneManager.LoadSceneAsync(sceneName);

        // 4. 로드 직후 실행할 함수
        onLoaded?.Invoke();

        // 5. 화면 밝게
        yield return Fade(1f, 0f, 0.5f);
    }


    // =========================================================
    // ★ 씬에 따라 화면 방향을 변경하는 함수
    // =========================================================
    private void SetOrientation(string sceneName)
    {
        // -----------------------------
        // 가로 화면으로 사용할 씬
        // -----------------------------
        if (sceneName == "StoreGame" ||
            sceneName == "ChargingMinigame" ||
            sceneName == "RepairShop")
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            Debug.Log(sceneName + " → 가로 화면으로 변경");
        }

        // -----------------------------
        // 세로 화면으로 사용할 씬
        // -----------------------------
        else if (sceneName == "DrivingScene" ||
                 sceneName == "Farming")
        {
            Screen.orientation = ScreenOrientation.Portrait;

            Debug.Log(sceneName + " → 세로 화면으로 변경");
        }
    }


    private IEnumerator Fade(float from, float to, float duration)
    {
        if (to > 0f)
        {
            blackScreen.blocksRaycasts = true;
        }

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            blackScreen.alpha =
                Mathf.Lerp(from, to, t / duration);

            yield return null;
        }

        blackScreen.alpha = to;

        if (to <= 0f)
        {
            blackScreen.blocksRaycasts = false;
        }
    }
}