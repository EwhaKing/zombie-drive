
using UnityEngine;
using UnityEngine.SceneManagement;

public class FuelManager : MonoBehaviour
{
    public static FuelManager Instance { get; private set; }

    [Header("연료 설정")]
    public int maxFuel = 10;
    public int currentFuel = 10;

    [Header("연료 자동 감소 설정")]
    public float fuelDecreaseInterval = 180f;

    private float fuelTimer = 0f;

    // 연료가 남아 있으면 true
    public bool HasFuel => currentFuel > 0;

    private void Awake()
    {
        // 중복 컴포넌트 방지
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        // 기존 GameManager와 함께 씬 이동 시 유지
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        // DrivingScene에서만 감소
        if (SceneManager.GetActiveScene().name != "DrivingScene")
            return;

        // GPS가 주행 중이 아니면 감소 중단
        if (GPSManager.Instance != null &&
            !GPSManager.Instance.isDriving)
            return;

        // 연료가 없으면 감소 중단
        if (!HasFuel)
            return;

        // 타이어가 없으면 감소 중단
        if (VehicleTireStatusManager.Instance != null &&
            !VehicleTireStatusManager.Instance.HasTire)
            return;

        // 감소 간격이 잘못되었으면 실행하지 않음
        if (fuelDecreaseInterval <= 0f)
            return;

        // 실제 플레이 시간 누적
        fuelTimer += Time.deltaTime;

        // 설정된 시간이 지나면 연료 1 감소
        if (fuelTimer >= fuelDecreaseInterval)
        {
            fuelTimer -= fuelDecreaseInterval;
            UseFuel(1);

            Debug.Log(
                "연료 1 감소! 현재 연료: " + currentFuel
            );
        }
    }

    // 연료 감소
    public void UseFuel(int amount)
    {
        if (amount <= 0)
            return;

        currentFuel = Mathf.Clamp(
            currentFuel - amount,
            0,
            maxFuel
        );
    }

    // 연료 충전
    public void AddFuel(int amount)
    {
        if (amount <= 0)
            return;

        currentFuel = Mathf.Clamp(
            currentFuel + amount,
            0,
            maxFuel
        );
    }

    // 연료값 직접 설정
    public void SetFuel(int amount)
    {
        currentFuel = Mathf.Clamp(
            amount,
            0,
            maxFuel
        );
    }
}
