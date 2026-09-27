using UnityEngine;
using System;

public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance { get; private set; }

    [Header("목적지 설정")]
    public float totalDistanceKM = 100f;
    public float remainingDistanceKM = 100f;
    public float speedKmPerHour = 50f; // 시속 (km/h)

    [Header("Day 설정")]
    public float distancePerDay = 33.3f;

    public bool isDriving = true; // 주행 중 여부

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 차로 주행 중일 때만 실시간 시간/거리 차감 (초당 이동 거리 계산)
        if (isDriving && remainingDistanceKM > 0f)
        {
            float speedKmPerSec = speedKmPerHour / 3600f;
            remainingDistanceKM -= speedKmPerSec * Time.deltaTime;

            if (remainingDistanceKM <= 0f)
            {
                remainingDistanceKM = 0f;
                isDriving = false;
            }
        }
    }

    // 내렸다 돌아왔을 때 추가 차감 (10 ~ 20km)
    public float ReduceDistanceOnReturn()
    {
        float randomTravel = UnityEngine.Random.Range(10f, 20f);
        randomTravel = Mathf.Round(randomTravel * 10f) / 10f;

        remainingDistanceKM -= randomTravel;

        if (remainingDistanceKM <= 0f)
        {
            remainingDistanceKM = 0f;
            isDriving = false;
        }

        Debug.Log($"탐험 복귀! 추가 이동: {randomTravel}km / 남은 거리: {remainingDistanceKM}km");
        return randomTravel;
    }

    // 남은 시간 계산
    public TimeSpan GetRemainingRealTime()
    {
        if (remainingDistanceKM <= 0) return TimeSpan.Zero;

        double remainingHours = remainingDistanceKM / speedKmPerHour;
        return TimeSpan.FromHours(remainingHours);
    }

    public (float start, float end) GetDayDistanceRange(int currentDay)
    {
        float startDist = (currentDay - 1) * distancePerDay;
        float endDist = Mathf.Min(currentDay * distancePerDay, totalDistanceKM);
        return (startDist, endDist);
    }
}