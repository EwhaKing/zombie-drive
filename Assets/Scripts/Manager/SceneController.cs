using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // 어디서든 접근할 수 있는 싱글톤 인스턴스
    public static SceneController Instance { get; private set; }

    private void Awake()
    {
        // 1. Instance가 비어있다면 자기 자신을 static 변수에 할당
        if (Instance == null)
        {
            Instance = this;
            // GameManager가 최상위 오브젝트가 아니라면 부모를 해제하여 Root로 생성
            transform.SetParent(null); 
            DontDestroyOnLoad(gameObject);
        }
        // 2. 이미 Instance가 존재하고, 그게 자기 자신이 아니라면 (중복 생성된 경우)
        else if (Instance != this)
        {
            // 오브젝트(gameObject) 전체를 지우지 않고, 중복 추가된 '스크립트 컴포넌트'만 삭제
            Destroy(this);
            return;
        }
    }

    // 1. 씬 이름(String)으로 이동
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 2. Build Settings의 씬 인덱스(Int)로 이동
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // 3. 현재 씬 재시작 (Restart)
    public void ReloadCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    // 4. 다음 씬으로 이동 (Build Settings 순서 기준)
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextSceneIndex);
    }
}