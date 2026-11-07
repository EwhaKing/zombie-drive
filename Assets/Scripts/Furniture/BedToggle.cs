using System.Collections;
using UnityEngine;

public class BedToggle : MonoBehaviour
{
    private bool isSleeping = false;
    public GameObject wakeUpButton;

    [Header("캐릭터 상태 (Canvas Group 필요)")]
    public CanvasGroup standingGroup;   // 서 있는 캐릭터
    public CanvasGroup sleepingGroup;   // 누운 침대 이미지
    public float fadeTime = 0.4f;

    [Header("수면 회복 설정")]
    public float sleepSpeed = 10f;
    private float targetSleepAdd = 0f;
    private float currentSleepAdded = 0f;

    private Coroutine fadeRoutine;

    private void Start()
    {
        // 시작: 서 있는 캐릭터만 보이게
        SetAlpha(standingGroup, 1f);
        SetAlpha(sleepingGroup, 0f);
        if (wakeUpButton != null) wakeUpButton.SetActive(false);
    }

    public void OnBedClick()
    {
        if (!isSleeping)
        {
            isSleeping = true;
            targetSleepAdd = Random.Range(20f, 30f);
            currentSleepAdded = 0f;
            StartFade(true);
        }
        else
        {
            wakeUpButton.SetActive(true);
        }
    }

    void Update()
    {
        if (isSleeping && currentSleepAdded < targetSleepAdd)
        {
            float increment = sleepSpeed * Time.deltaTime;
            if (currentSleepAdded + increment > targetSleepAdd)
                increment = targetSleepAdd - currentSleepAdded;

            currentSleepAdded += increment;

            if (CharacterStats.Instance != null)
                CharacterStats.Instance.SleepInBed(sleepSpeed);

            if (currentSleepAdded >= targetSleepAdd)
            {
                Debug.Log("충분히 잠을 잤습니다.");
                wakeUpButton.SetActive(true);
            }
        }
    }

    public void OnWakeUpClick()
    {
        isSleeping = false;
        wakeUpButton.SetActive(false);
        StartFade(false);
    }

    private void StartFade(bool sleeping)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(sleeping));
    }

    private IEnumerator FadeRoutine(bool sleeping)
    {
        float standFrom = standingGroup.alpha;
        float sleepFrom = sleepingGroup.alpha;
        float standTo = sleeping ? 0f : 1f;
        float sleepTo = sleeping ? 1f : 0f;

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float k = t / fadeTime;
            standingGroup.alpha = Mathf.Lerp(standFrom, standTo, k);
            sleepingGroup.alpha = Mathf.Lerp(sleepFrom, sleepTo, k);
            yield return null;
        }

        standingGroup.alpha = standTo;
        sleepingGroup.alpha = sleepTo;
    }

    private void SetAlpha(CanvasGroup g, float a)
    {
        if (g != null) g.alpha = a;
    }
}