using System;                          // Action(콜백 함수)을 쓰기 위해 필요
using System.Collections;              // 코루틴(IEnumerator)을 쓰기 위해 필요
using UnityEngine;
using UnityEngine.SceneManagement;     // 씬 전환(LoadScene)을 쓰기 위해 필요

// 이 스크립트 하나만 "씬을 어떻게 바꿀지(암전 연출 포함)"를 전담합니다.
// 다른 스크립트(DrivingManager 등)는 이 스크립트를 "호출만" 하고,
// 실제 씬 전환 로직은 여기서만 처리합니다.
public class SceneTransition : MonoBehaviour
{
    // 싱글톤: 어디서든 SceneTransition.Instance 로 접근 가능하게 함
    public static SceneTransition Instance;

    // 화면을 덮는 검은 이미지의 CanvasGroup (인스펙터에서 직접 연결해야 함)
    public CanvasGroup blackScreen;

    private void Awake()
    {
        // 씬이 바뀌어도 이 매니저가 사라지지 않도록 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 시작할 때 검은 화면은 완전히 꺼진 상태로 초기화
            blackScreen.alpha = 0f;
            blackScreen.blocksRaycasts = false;
            blackScreen.interactable = false;
        }
        else
        {
            // 이미 매니저가 존재하면(씬 재진입 시 중복 생성 방지) 자기 자신을 파괴
            Destroy(gameObject);
        }
    }

    // ---------------------------------------------------------
    // 콜백 없이 그냥 씬만 전환하고 싶을 때 호출 (예: 내리지 않기 등에서는 안 씀)
    // ---------------------------------------------------------
    public void GoToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName, null));
    }

    // ---------------------------------------------------------
    // 씬 전환이 "완료된 직후" 특정 코드를 실행하고 싶을 때 호출
    // 예: 파밍 씬에서 복귀했을 때 "카운트 +1"을 씬 로드 끝난 뒤에 실행하고 싶은 경우
    // ---------------------------------------------------------
    public void GoToSceneWithCallback(string sceneName, Action onLoaded)
    {
        StartCoroutine(FadeAndLoad(sceneName, onLoaded));
    }

    // 실제 암전 → 씬 로드 → 콜백 실행 → 다시 밝아짐 순서로 진행되는 코루틴
    private IEnumerator FadeAndLoad(string sceneName, Action onLoaded)
    {
        // 1) 화면이 서서히 검게 (alpha 0 -> 1)
        yield return Fade(0f, 1f, 0.5f);

        // 2) 실제 씬 로드 (로드가 끝날 때까지 여기서 대기)
        yield return SceneManager.LoadSceneAsync(sceneName);

        // 3) 씬 로드가 끝난 직후, 넘겨받은 콜백이 있으면 실행
        onLoaded?.Invoke();

        // 4) 화면이 서서히 밝아짐 (alpha 1 -> 0)
        yield return Fade(1f, 0f, 0.5f);
    }

    // 검은 화면의 투명도(alpha)를 duration 초 동안 from -> to 로 서서히 바꾸는 함수
private IEnumerator Fade(float from, float to, float duration)
{
    // 화면이 어두워지기 시작하는 순간(to가 1에 가까울 때)부터 클릭을 막는다
    // to > 0 이면 (즉 검은 화면이 나타나는 중이면) 클릭 차단 시작
    if (to > 0f)
    {
        blackScreen.blocksRaycasts = true;
    }

    float t = 0f;
    while (t < duration)
    {
        t += Time.deltaTime;
        blackScreen.alpha = Mathf.Lerp(from, to, t / duration);
        yield return null;
    }
    blackScreen.alpha = to;

    // 화면이 완전히 밝아졌다면(to가 0이면) 그때 클릭 차단을 풀어준다
    if (to <= 0f)
    {
        blackScreen.blocksRaycasts = false;
    }
}

}