
using UnityEngine;

public class FuelManager : MonoBehaviour
{
    public static FuelManager Instance { get; private set; }

    [Header("연료 설정")]
    public int maxFuel = 10;
    public int currentFuel = 10;

    private void Awake()
    {
        // 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // 연료 감소
    public void UseFuel(int amount)
    {
        if (amount <= 0) return;

        currentFuel = Mathf.Clamp(
            currentFuel - amount, 0, maxFuel
        );
    }

    // 연료 충전
    public void AddFuel(int amount)
    {
        if (amount <= 0) return;

        currentFuel = Mathf.Clamp(
            currentFuel + amount, 0, maxFuel
        );
    }

    // 연료를 특정 수치로 설정
    public void SetFuel(int amount)
    {
        currentFuel = Mathf.Clamp(amount, 0, maxFuel);
    }
}
