using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BottomUISlideController : MonoBehaviour
{
    [Header("하단 UI 전체")]
    [SerializeField]
    private RectTransform bottomPanel;

    [Header("하단 UI를 내리는 ↓ 버튼")]
    [SerializeField]
    private Button closeButton;

    [Header("하단 UI를 올리는 ↑ 버튼")]
    [SerializeField]
    private Button openButton;

    [Header("↑ 버튼 페이드용")]
    [SerializeField]
    private CanvasGroup openButtonCanvasGroup;

    [Header("슬라이드 설정")]
    [SerializeField]
    private float slideDuration = 0.35f;

    [Tooltip("0이면 패널 높이를 기준으로 자동 계산")]
    [SerializeField]
    private float slideDistance = 0f;

    [Tooltip("자동 계산 시 패널이 확실하게 화면 밖으로 나가도록 추가 이동")]
    [SerializeField]
    private float extraHideDistance = 60f;

    [Header("↑ 버튼 페이드 시간")]
    [SerializeField]
    private float arrowFadeDuration = 0.15f;

    [Header("시작 상태")]
    [SerializeField]
    private bool startHidden = false;


    private Vector2 shownPosition;
    private Vector2 hiddenPosition;

    private bool isHidden;
    private bool isAnimating;

    private Coroutine transitionCoroutine;


    private void Start()
    {
        if (bottomPanel == null)
        {
            Debug.LogError("Bottom Panel이 연결되지 않았습니다.");
            return;
        }

        if (closeButton == null)
        {
            Debug.LogError("Close Button이 연결되지 않았습니다.");
            return;
        }

        if (openButton == null)
        {
            Debug.LogError("Open Button이 연결되지 않았습니다.");
            return;
        }

        if (openButtonCanvasGroup == null)
        {
            Debug.LogError("Open Button CanvasGroup이 연결되지 않았습니다.");
            return;
        }


        // UI 크기 계산
        Canvas.ForceUpdateCanvases();

        // 현재 위치를 펼쳐진 위치로 저장
        shownPosition = bottomPanel.anchoredPosition;


        // 이동 거리 계산
        float distance = slideDistance;

        if (distance <= 0f)
        {
            distance =
                bottomPanel.rect.height +
                extraHideDistance;
        }

        // 하단 UI이므로 아래 방향으로 숨김
        hiddenPosition =
            shownPosition +
            Vector2.down * distance;


        // 버튼 이벤트 연결
        closeButton.onClick.AddListener(HideBottomUI);
        openButton.onClick.AddListener(ShowBottomUI);


        // 시작 상태 설정
        isHidden = startHidden;

        if (startHidden)
        {
            bottomPanel.anchoredPosition =
                hiddenPosition;

            SetOpenButtonVisible(true, true);

            closeButton.interactable = false;
        }
        else
        {
            bottomPanel.anchoredPosition =
                shownPosition;

            SetOpenButtonVisible(false, true);

            closeButton.interactable = true;
        }
    }


    // ↓ 클릭
    public void HideBottomUI()
    {
        if (isHidden || isAnimating)
            return;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine =
            StartCoroutine(HideRoutine());
    }


    // ↑ 클릭
    public void ShowBottomUI()
    {
        if (!isHidden || isAnimating)
            return;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine =
            StartCoroutine(ShowRoutine());
    }


    private IEnumerator HideRoutine()
    {
        isAnimating = true;

        closeButton.interactable = false;
        openButton.interactable = false;

        // 하단 UI 전체를 아래로 이동
        yield return StartCoroutine(
            SlidePanel(
                bottomPanel.anchoredPosition,
                hiddenPosition
            )
        );

        isHidden = true;

        // UI가 사라진 후 ↑ 버튼 등장
        yield return StartCoroutine(
            FadeOpenButton(0f, 1f)
        );

        openButtonCanvasGroup.blocksRaycasts = true;
        openButtonCanvasGroup.interactable = true;

        openButton.interactable = true;

        isAnimating = false;
    }


    private IEnumerator ShowRoutine()
    {
        isAnimating = true;

        openButton.interactable = false;

        // ↑ 버튼을 먼저 부드럽게 숨김
        yield return StartCoroutine(
            FadeOpenButton(1f, 0f)
        );

        openButtonCanvasGroup.blocksRaycasts = false;
        openButtonCanvasGroup.interactable = false;

        // 하단 UI 다시 올라옴
        yield return StartCoroutine(
            SlidePanel(
                bottomPanel.anchoredPosition,
                shownPosition
            )
        );

        isHidden = false;

        closeButton.interactable = true;

        isAnimating = false;
    }


    private IEnumerator SlidePanel(
        Vector2 start,
        Vector2 target)
    {
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / slideDuration
                );

            // 부드러운 가속 / 감속
            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            bottomPanel.anchoredPosition =
                Vector2.Lerp(
                    start,
                    target,
                    t
                );

            yield return null;
        }

        bottomPanel.anchoredPosition = target;
    }


    private IEnumerator FadeOpenButton(
        float startAlpha,
        float targetAlpha)
    {
        float elapsed = 0f;

        while (elapsed < arrowFadeDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / arrowFadeDuration
                );

            openButtonCanvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );

            yield return null;
        }

        openButtonCanvasGroup.alpha =
            targetAlpha;
    }


    private void SetOpenButtonVisible(
        bool visible,
        bool instant)
    {
        float alpha =
            visible ? 1f : 0f;

        openButtonCanvasGroup.alpha = alpha;

        openButtonCanvasGroup.interactable =
            visible;

        openButtonCanvasGroup.blocksRaycasts =
            visible;

        openButton.interactable =
            visible;
    }


    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                HideBottomUI
            );
        }

        if (openButton != null)
        {
            openButton.onClick.RemoveListener(
                ShowBottomUI
            );
        }
    }
}