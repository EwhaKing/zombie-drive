using UnityEngine;
using UnityEngine.SceneManagement;

public class VehicleTireStatusManager : MonoBehaviour
{
    public static VehicleTireStatusManager Instance { get; private set; }

    [Header("타이어 설정")]
    public int maxTire = 10;
    public int currentTire = 10;

    [Header("자동 감소 설정")]
    public float tireDecreaseInterval = 180f;

    private float tireTimer = 0f;

    // 타이어가 남아 있는지 확인
    public bool HasTire => currentTire > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        // DrivingScene에서만 타이어 감소
        if (SceneManager.GetActiveScene().name != "DrivingScene")
            return;

        // 주행 중이 아닐 때 감소 중지
        if (GPSManager.Instance != null &&
            !GPSManager.Instance.isDriving)
            return;

        // 타이어가 0이면 감소 중지
        if (!HasTire)
            return;

        // 연료가 0이면 감소 중지
        if (FuelManager.Instance != null &&
            !FuelManager.Instance.HasFuel)
            return;

        if (tireDecreaseInterval <= 0f)
            return;

        // 현실 시간 누적
        tireTimer += Time.unscaledDeltaTime;

        // 180초마다 타이어 1 감소
        if (tireTimer >= tireDecreaseInterval)
        {
            tireTimer -= tireDecreaseInterval;
            UseTire(1);

            Debug.Log(
                "타이어 1 감소! 현재 타이어: " + currentTire
            );
        }
    }

    // 타이어 감소
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
        currentTire = Mathf.Clamp(
            amount, 0, maxTire
        );
    }
}
