
using UnityEngine;
using UnityEngine.SceneManagement;

public class FuelManager : MonoBehaviour
{
    public static FuelManager Instance { get; private set; }

    [Header("연료 설정")]
    public int maxFuel = 10;
    public int currentFuel = 10;

    // 연료가 남아 있어야 주행 가능
    public bool HasFuel => currentFuel > 0;

    [Header("연료 감소 시간 (초)")]
    public float fuelDecreaseInterval = 180f;

    // 현재까지 누적된 주행 시간
    private float fuelTimer = 0f;

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

    private void Update()
    {
        // 1. DrivingScene에서만 연료 감소
        if (SceneManager.GetActiveScene().name != "DrivingScene")
            return;

        // 2. 차량이 주행 중이 아닐 때 감소 중단
        if (GPSManager.Instance != null &&
            !GPSManager.Instance.isDriving)
            return;

        // 3. 연료가 0이면 감소 중단
        if (currentFuel <= 0)
            return;

        // 4. 감소 주기가 올바른지 확인
        if (fuelDecreaseInterval <= 0f)
            return;

        // 5. 실제 플레이 시간 누적
        fuelTimer += Time.deltaTime;

        // 6. 설정된 시간이 지날 때마다 연료 1 감소
        if (fuelTimer >= fuelDecreaseInterval)
        {
            fuelTimer -= fuelDecreaseInterval;
            UseFuel(1);

            Debug.Log("연료 1 감소! 현재 연료: " + currentFuel);
        }
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

    // 연료값 직접 설정
    public void SetFuel(int amount)
    {
        currentFuel = Mathf.Clamp(amount, 0, maxFuel);
    }
}
