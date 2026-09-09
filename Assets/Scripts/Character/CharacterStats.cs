using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterStats : MonoBehaviour
{
    public static CharacterStats Instance { get; private set; }

    [Header("스탯 최대값")]
    public float maxSleep = 100f;
    public float maxHunger = 100f;
    public float maxHealth = 100f;

    [Header("현재 스탯")]
    public float currentSleep;
    public float currentHunger;
    public float currentHealth;

    [Header("잠 / 배고픔 감소율 (초당)")]
    public float sleepDepletionRate = 1f;   
    public float hungerDepletionRate = 1.5f; 

    [Header("건강 감소 주기 설정")]
    public float healthDamageInterval = 600f; 
    public float healthDamageAmount = 10f;    

    [Header("UI 슬라이더 연결 (현재 씬 기준)")]
    public Slider sleepSlider;
    public Slider hungerSlider;
    public Slider healthSlider;

    private bool isDead = false;
    private Coroutine healthDamageCoroutine;

    private void Awake()
    {
        // 1. Instance가 비어있다면 자기 자신을 static 변수에 할당
        if (Instance == null)
        {
            Instance = this;
            // GameManager가 최상위 오브젝트가 아니라면 부모를 해제하여 Root로 생성
            transform.SetParent(null); 
            DontDestroyOnLoad(gameObject);
        }
        // 2. 이미 Instance가 존재하고, 그게 자기 자신이 아니라면 (중복 생성된 경우)
        else if (Instance != this)
        {
            // 오브젝트(gameObject) 전체를 지우지 않고, 중복 추가된 '스크립트 컴포넌트'만 삭제
            Destroy(this);
            return;
        }

        // 초기 스탯 세팅
        currentSleep = maxSleep;
        currentHunger = maxHunger;
        currentHealth = maxHealth;
    }


    void OnEnable()
    {
        // 씬 로드 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 씬 로드 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 변경될 때마다 자동 실행되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로운 씬에 있는 UI 슬라이더들을 이름으로 자동 찾기 (태그나 이름 세팅 필요)
        FindAndAssignSliders();
        UpdateUI();
    }

    void Update()
    {
        if (isDead) return;

        // 시간에 따른 잠/배고픔 감소
        currentSleep -= sleepDepletionRate * Time.deltaTime;
        currentHunger -= hungerDepletionRate * Time.deltaTime;

        currentSleep = Mathf.Clamp(currentSleep, 0f, maxSleep);
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        // 건강 감소 코루틴 제어
        if (currentSleep <= 0f || currentHunger <= 0f)
        {
            if (healthDamageCoroutine == null)
            {
                healthDamageCoroutine = StartCoroutine(HealthDamageRoutine());
            }
        }
        else
        {
            if (healthDamageCoroutine != null)
            {
                StopCoroutine(healthDamageCoroutine);
                healthDamageCoroutine = null;
            }
        }

        UpdateUI();
    }

    private IEnumerator HealthDamageRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(healthDamageInterval);

            currentHealth -= healthDamageAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            if (currentHealth <= 0f)
            {
                Die();
                yield break; 
            }
        }
    }

    // 씬 이동 시 새 씬의 슬라이더 오브젝트를 검색하여 연결하는 함수
    private void FindAndAssignSliders()
    {
        // 씬 내부에서 슬라이더 오브젝트 탐색 (오브젝트 이름 기준)
        GameObject sleepObj = GameObject.Find("SleepSlider");
        if (sleepObj != null) sleepSlider = sleepObj.GetComponent<Slider>();

        GameObject hungerObj = GameObject.Find("HungerSlider");
        if (hungerObj != null) hungerSlider = hungerObj.GetComponent<Slider>();

        GameObject healthObj = GameObject.Find("HealthSlider");
        if (healthObj != null) healthSlider = healthObj.GetComponent<Slider>();
    }

    public void UpdateUI()
    {
        if (sleepSlider) sleepSlider.value = currentSleep / maxSleep;
        if (hungerSlider) hungerSlider.value = currentHunger / maxHunger;
        if (healthSlider) healthSlider.value = currentHealth / maxHealth;
    }

    // 기존 SleepInBed 함수를 삭제/수정하고 아래로 대체
    public void SleepInBed(float amountPerSecond)
    {
        if (isDead) return;
        
        // Time.deltaTime을 곱해 초당 amountPerSecond만큼 차오르도록 함
        currentSleep = Mathf.Min(currentSleep + (amountPerSecond * Time.deltaTime), maxSleep);
        UpdateUI();
    }

    public void EatFood(float amount)
    {
        if (isDead) return;
        currentHunger = Mathf.Min(currentHunger + amount, maxHunger);
        UpdateUI();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("캐릭터가 사망했습니다.");
    }


    public void HealHealth(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

}