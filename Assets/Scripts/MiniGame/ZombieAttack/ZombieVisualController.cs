using UnityEngine;

public class ZombieVisualController : MonoBehaviour
{
    [Header("좀비 오브젝트")]
    [SerializeField] private GameObject zombieVisual;

    [Header("애니메이션 - 나중에 연결")]
    [SerializeField] private Animator animator;

    [SerializeField] private string attackTriggerName = "Attack";


    private void Awake()
    {
        HideZombie();
    }


    // ==============================
    // 좀비 등장
    // ==============================

    public void ShowZombie()
    {
        if (zombieVisual == null)
        {
            Debug.LogWarning(
                "Zombie Visual이 연결되지 않았습니다!"
            );

            return;
        }

        zombieVisual.SetActive(true);

        Debug.Log("좀비 등장!");
    }


    // ==============================
    // 좀비 숨기기
    // ==============================

    public void HideZombie()
    {
        if (zombieVisual != null)
        {
            zombieVisual.SetActive(false);
        }
    }


    // ==============================
    // 공격 애니메이션
    // ==============================

    public void PlayAttackAnimation()
    {
        // 아직 Animator가 없다면
        // 아무것도 하지 않음
        if (animator == null)
        {
            Debug.Log(
                "공격 애니메이션 없음 - 현재는 임시 좀비 이미지 사용"
            );

            return;
        }


        if (HasTrigger(attackTriggerName))
        {
            animator.SetTrigger(
                attackTriggerName
            );
        }
        else
        {
            Debug.LogWarning(
                $"Animator에 '{attackTriggerName}' Trigger가 없습니다."
            );
        }
    }


    // ==============================
    // Animator에 해당 Trigger가 있는지 확인
    // ==============================

    private bool HasTrigger(string triggerName)
    {
        if (animator == null)
            return false;


        foreach (
            AnimatorControllerParameter parameter
            in animator.parameters)
        {
            if (
                parameter.type ==
                AnimatorControllerParameterType.Trigger
                &&
                parameter.name ==
                triggerName)
            {
                return true;
            }
        }

        return false;
    }
}