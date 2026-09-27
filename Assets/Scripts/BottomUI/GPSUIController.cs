using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GPSUIController : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public GameObject gpsPanel;
    public TMP_Text remainingDistanceText;
    public TMP_Text remainingTimeText;
    public TMP_Text dayScopeText;
    public Slider progressSlider;

    [Header("마커 및 경로 UI 연결")]
    public RectTransform currentMarker;      // CurrentPoint (현재 위치 마커)
    public RectTransform startMarker;        // StartPoint (출발 지점)
    public RectTransform destinationMarker;  // DestinationPoint (도착 지점)

    [Header("버튼 연결")]
    public Button gpsNavigationButton;
    public Button closeButton;

    [Header("게임 진행 상태")]
    public int currentDay = 1;

    private void Start()
    {
        if (gpsNavigationButton != null)
        {
            gpsNavigationButton.onClick.RemoveAllListeners();
            gpsNavigationButton.onClick.AddListener(ToggleGPSPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ToggleGPSPanel);
        }

        UpdateGPSUI();
    }

    private void Update()
    {
        if (gpsPanel != null && gpsPanel.activeSelf)
        {
            UpdateGPSUI();
        }
    }

    public void ToggleGPSPanel()
    {
        if (gpsPanel == null) return;

        bool isActive = !gpsPanel.activeSelf;
        gpsPanel.SetActive(isActive);

        if (isActive)
        {
            UpdateGPSUI();
        }
    }

    public void UpdateGPSUI()
    {
        if (GPSManager.Instance == null) return;

        if (DrivingManager.Instance != null)
        {
            currentDay = DrivingManager.Instance.currentDay;
        }

        float totalDist = GPSManager.Instance.totalDistanceKM;
        float remainingDist = GPSManager.Instance.remainingDistanceKM;
        float traveledDist = totalDist - remainingDist; // 이동한 거리
        float progress = Mathf.Clamp01(traveledDist / totalDist); // 진행률 (0~1)

        // 1. 남은 거리 및 시간 갱신
        if (remainingDistanceText != null)
        {
            remainingDistanceText.text = $"남은 거리: {remainingDist:F1} km";
        }

        if (remainingTimeText != null)
        {
            TimeSpan remainingTime = GPSManager.Instance.GetRemainingRealTime();
            remainingTimeText.text = string.Format("목적지 도착까지: {0:D2}:{1:D2}:{2:D2}",
                remainingTime.Hours, remainingTime.Minutes, remainingTime.Seconds);
        }

        // 2. Day 목표 구간 갱신
        if (dayScopeText != null)
        {
            var (dayStart, dayEnd) = GPSManager.Instance.GetDayDistanceRange(currentDay);
            dayScopeText.text = $"[Day {currentDay}] 오늘 이동 가능 구간: {dayStart:F1}km ~ {dayEnd:F1}km";
        }

        // 3. 슬라이더 갱신
        if (progressSlider != null && totalDist > 0)
        {
            progressSlider.value = progress;
        }

        // 4. [핵심] 현재 위치 마커(CurrentPoint) 이동 로직
        if (currentMarker != null && startMarker != null && destinationMarker != null)
        {
            // 출발지 Y위치와 도착지 Y위치 사이를 진행률(progress: 0~1)만큼 보간하여 이동
            float startY = startMarker.anchoredPosition.y;
            float destY = destinationMarker.anchoredPosition.y;
            float currentY = Mathf.Lerp(startY, destY, progress);

            currentMarker.anchoredPosition = new Vector2(currentMarker.anchoredPosition.x, currentY);
        }
    }
}