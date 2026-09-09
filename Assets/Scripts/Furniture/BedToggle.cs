using UnityEngine;

public class BedToggle : MonoBehaviour
{
    private bool isSleeping = false;
    public GameObject wakeUpButton;

    [Header("수면 회복 설정")]
    public float sleepSpeed = 10f; // 초당 회복되는 속도 (예: 1초에 10씩 회복)
    private float targetSleepAdd = 0f; // 자면서 총 회복할 목표치 (20~30 사이)
    private float currentSleepAdded = 0f; // 지금까지 회복된 누적량

    public void OnBedClick()
    {
        if (!isSleeping)
        {
            isSleeping = true;
            // 20~30 사이의 총 회복량 목표 설정
            targetSleepAdd = Random.Range(20f, 30f);
            currentSleepAdded = 0f;
            Debug.Log($"캐릭터가 누웠습니다. (목표 수면 회복량: {targetSleepAdd:F1})");
        }
        else
        {
            wakeUpButton.SetActive(true);
        }
    }

    void Update()
    {
        // 침대에 누워있고, 목표 회복량까지 아직 다 안 찼다면
        if (isSleeping && currentSleepAdded < targetSleepAdd)
        {
            // 이번 프레임에 증가할 양 계산
            float increment = sleepSpeed * Time.deltaTime;

            // 목표치를 넘지 않도록 제한
            if (currentSleepAdded + increment > targetSleepAdd)
            {
                increment = targetSleepAdd - currentSleepAdded;
            }

            currentSleepAdded += increment;

            // CharacterStats에 초당 회복량 적용
            if (CharacterStats.Instance != null)
            {
                CharacterStats.Instance.SleepInBed(sleepSpeed);
            }

            // 목표치에 모두 도달했으면 일어나는 버튼 활성화
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
        Debug.Log("캐릭터가 깨어났습니다.");
    }
}