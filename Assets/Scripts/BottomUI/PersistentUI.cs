using UnityEngine;

public class PersistentUI : MonoBehaviour
{
    private static PersistentUI instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            // 이미 다른 씬에서 살아남은 UI가 있으면, 지금 새로 생긴 걸 파괴
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}