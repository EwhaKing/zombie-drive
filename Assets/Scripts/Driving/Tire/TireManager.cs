
using UnityEngine;

public class TireManager : MonoBehaviour
{
    public static TireManager Instance { get; private set; }

    [Header("타이어 설정")]
    public int maxTire = 10;
    public int currentTire = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        // 기존 GameManager와 함께 유지
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    // 타이어 내구도 감소
    public void UseTire(int amount)
    {
        if (amount <= 0) return;

        currentTire = Mathf.Clamp(
            currentTire - amount, 0, maxTire
        );
    }

    // 타이어 수리
    public void AddTire(int amount)
    {
        if (amount <= 0) return;

        currentTire = Mathf.Clamp(
            currentTire + amount, 0, maxTire
        );
    }

    // 타이어 상태 직접 설정
    public void SetTire(int amount)
    {
        currentTire = Mathf.Clamp(amount, 0, maxTire);
    }

    // 타이어가 남아 있는지 확인
    public bool HasTire => currentTire > 0;
}
