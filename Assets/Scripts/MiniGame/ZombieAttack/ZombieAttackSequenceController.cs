using UnityEngine;

public class ZombieAttackSequenceController : MonoBehaviour
{
    [Header("공격 경로")]
    [SerializeField]
    private AttackPathController attackPathController;

    [Header("좀비")]
    [SerializeField]
    private ZombieVisualController zombieVisualController;


    private void OnEnable()
    {
        if (attackPathController != null)
        {
            attackPathController.PreviewFinished +=
                OnPathPreviewFinished;
        }
    }


    private void OnDisable()
    {
        if (attackPathController != null)
        {
            attackPathController.PreviewFinished -=
                OnPathPreviewFinished;
        }
    }


    // ==============================
    // 경로 미리보기 종료
    // ==============================

    private void OnPathPreviewFinished()
    {
        Debug.Log(
            "경로 표시 종료 → 좀비 등장"
        );


        if (zombieVisualController != null)
        {
            zombieVisualController.ShowZombie();
        }


        // ===================================
        // 나중에 공격 구현 시 여기서
        //
        // zombieVisualController
        //     .PlayAttackAnimation();
        //
        // 등을 호출할 수 있음
        // ===================================
    }
}