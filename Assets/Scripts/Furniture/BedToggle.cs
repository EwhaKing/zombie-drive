using UnityEngine;

public class BedToggle : MonoBehaviour
{
    private bool isSleeping = false;
    public GameObject wakeUpButton;

    public void OnBedClick()
    {
        if (!isSleeping)
        {
            isSleeping = true;
            Debug.Log("캐릭터가 침대에 누워 잠들었습니다.");
        }
        else
        {
            wakeUpButton.SetActive(true);
        }
    }

    public void OnWakeUpClick()
    {
        isSleeping = false;
        wakeUpButton.SetActive(false);
        Debug.Log("캐릭터가 깨어나 원래 자리로 돌아갔습니다.");
    }
}