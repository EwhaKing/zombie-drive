using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoltSlot : MonoBehaviour, IPointerClickHandler
{
    public enum BoltState
    {
        Full,
        Half,
        Missing
    }

    [Header("초기 상태")]
    public BoltState initialState;

    [Header("이미지")]
    public Image boltImage;
    public GameObject holeVisual;

    public Sprite fullBoltSprite;
    public Sprite halfBoltSprite;

    [Header("빠진 나사일 경우 바닥 나사")]
    public DraggableBolt floorBolt;

    [Header("Manager")]
    public RepairShopGameManager gameManager;

    private BoltState currentState;

    public bool IsCompleted
    {
        get { return currentState == BoltState.Full; }
    }

    public RectTransform DropArea
    {
        get { return transform as RectTransform; }
    }

    public void ResetSlot()
    {
        currentState = initialState;

        if (floorBolt != null)
        {
            floorBolt.Bind(this);

            bool shouldShow =
                initialState == BoltState.Missing;

            floorBolt.gameObject.SetActive(shouldShow);
        }

        ApplyVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager == null)
            return;

        if (!gameManager.IsBoltStageActive)
            return;

        // 반쯤 끼워진 나사만 터치로 완전 체결
        if (currentState == BoltState.Half)
        {
            currentState = BoltState.Full;

            ApplyVisual();

            gameManager.NotifyBoltChanged();
        }
    }

    public bool AttachFromDrag()
    {
        if (gameManager == null)
            return false;

        if (!gameManager.IsBoltStageActive)
            return false;

        if (currentState != BoltState.Missing)
            return false;

        currentState = BoltState.Full;

        ApplyVisual();

        gameManager.NotifyBoltChanged();

        return true;
    }

    private void ApplyVisual()
    {
        switch (currentState)
        {
            case BoltState.Full:

                boltImage.enabled = true;
                boltImage.sprite = fullBoltSprite;

                if (holeVisual != null)
                    holeVisual.SetActive(true);

                break;


            case BoltState.Half:

                boltImage.enabled = true;
                boltImage.sprite = halfBoltSprite;

                if (holeVisual != null)
                    holeVisual.SetActive(true);

                break;


            case BoltState.Missing:

                boltImage.enabled = false;

                if (holeVisual != null)
                    holeVisual.SetActive(true);

                break;
        }
    }
}