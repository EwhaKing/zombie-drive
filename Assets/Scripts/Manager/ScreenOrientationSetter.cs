using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenOrientationSetter : MonoBehaviour
{
    [System.Serializable]
    public struct SceneOrientationEntry
    {
        public string sceneName;
        public ScreenOrientation orientation;
    }

    [Header("씬별 화면 방향 설정")]
    public SceneOrientationEntry[] orientationSettings;

    [Header("목록에 없는 씬의 기본값")]
    public ScreenOrientation defaultOrientation = ScreenOrientation.Portrait;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 최초 실행 시 현재 씬에도 바로 적용
        ApplyOrientation(SceneManager.GetActiveScene().name);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyOrientation(scene.name);
    }

    private void ApplyOrientation(string sceneName)
    {
        foreach (var entry in orientationSettings)
        {
            if (entry.sceneName == sceneName)
            {
                Screen.orientation = entry.orientation;
                return;
            }
        }

        Screen.orientation = defaultOrientation;
    }
}