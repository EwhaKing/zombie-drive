using System.Collections;
using UnityEngine;

public class ZombieAttackSequenceController : MonoBehaviour
{
    [Header("게임")]
    [SerializeField]
    private ZombieAttackGameManager gameManager;


    [Header("공격 경로")]
    [SerializeField]
    private AttackPathController attackPathController;


    [Header("좀비")]
    [SerializeField]
    private ZombieVisualController zombieVisualController;


    [Header("터치 입력")]
    [SerializeField]
    private TouchPathInputController touchInputController;


    [Header("다음 좀비 등장 간격")]
    [SerializeField]
    private float nextRoundDelay = 0.4f;


    private Coroutine nextRoundCoroutine;


    private void OnEnable()
    {
        if (attackPathController != null)
        {
            attackPathController.PreviewFinished +=
                OnPathPreviewFinished;
        }


        if (touchInputController != null)
        {
            touchInputController.AttemptSucceeded +=
                OnAttackSucceeded;

            touchInputController.AttemptFailed +=
                OnAttackFailed;
        }
    }


    private void OnDisable()
    {
        if (attackPathController != null)
        {
            attackPathController.PreviewFinished -=
                OnPathPreviewFinished;
        }


        if (touchInputController != null)
        {
            touchInputController.AttemptSucceeded -=
                OnAttackSucceeded;

            touchInputController.AttemptFailed -=
                OnAttackFailed;
        }
    }


    // ==============================
    // 경로 미리보기 종료
    // ==============================

    private void OnPathPreviewFinished()
    {
        if (
            gameManager != null &&
            !gameManager.IsGameRunning)
        {
            return;
        }


        Debug.Log(
            "경로 표시 종료 → 좀비 등장"
        );


        if (zombieVisualController != null)
        {
            zombieVisualController.ShowZombie();

            // Animator가 없으면 아무 일도 안 일어나고,
            // 나중에 Animator가 연결되면
            // Attack Trigger가 실행됨
            zombieVisualController
                .PlayAttackAnimation();
        }


        // 현재는 좀비 등장과 동시에
        // 입력 가능 상태 시작
        //
        // 나중에는 공격 애니메이션의
        // 실제 타격 순간에 이 함수를 호출하면 됨.
        if (touchInputController != null)
        {
            touchInputController
                .BeginInputWindow();
        }
    }


    // ==============================
    // 플레이어 성공
    // ==============================

    private void OnAttackSucceeded()
    {
        Debug.Log(
            "좀비 처치 성공!"
        );


        if (zombieVisualController != null)
        {
            zombieVisualController
                .HideZombie();
        }


        StartNextRound();
    }


    // ==============================
    // 플레이어 실패
    // ==============================

    private void OnAttackFailed()
    {
        Debug.Log(
            "좀비 처치 실패!"
        );


        // 목숨 감소
        if (gameManager != null)
        {
            gameManager.LoseLife();
        }


        if (zombieVisualController != null)
        {
            zombieVisualController
                .HideZombie();
        }


        // 목숨 0이라 게임이 끝났다면
        // 다음 좀비 생성 X
        if (
            gameManager != null &&
            !gameManager.IsGameRunning)
        {
            return;
        }


        StartNextRound();
    }


    // ==============================
    // 다음 라운드 준비
    // ==============================

    private void StartNextRound()
    {
        if (nextRoundCoroutine != null)
        {
            StopCoroutine(
                nextRoundCoroutine
            );
        }


        nextRoundCoroutine =
            StartCoroutine(
                NextRoundCoroutine()
            );
    }


    private IEnumerator
        NextRoundCoroutine()
    {
        yield return new WaitForSeconds(
            nextRoundDelay
        );


        if (
            gameManager != null &&
            !gameManager.IsGameRunning)
        {
            yield break;
        }


        if (attackPathController != null)
        {
            attackPathController
                .ShowRandomPath();
        }


        nextRoundCoroutine = null;
    }
}