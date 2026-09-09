using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum BoltState
{
    Inserted,       // Á¤»óÀûÀ¸·Î ³¢¿öÁü
    HalfInserted,   // ¹ÝÂë Æ¢¾î³ª¿È
    Empty           // ¾Æ¿¹ ºüÁü
}

public class BoltSlot : MonoBehaviour,
    IPointerClickHandler,
    IDropHandler
{
    [Header("UI")]
    [SerializeField] private GameObject holeImage;
    [SerializeField] private Image boltImage;

    [Header("³ª»ç ÀÌ¹ÌÁö")]
    [SerializeField] private Sprite insertedBoltSprite;
    [SerializeField] private Sprite halfInsertedBoltSprite;

    private BoltState currentState;

    private BoltPhaseManager manager;

    public BoltState CurrentState => currentState;

    // -----------------------------
    // ÃÊ±â »óÅÂ ¼³Á¤
    // -----------------------------
    public void Setup(
        BoltState state,
        BoltPhaseManager boltManager)
    {
        manager = boltManager;

        SetState(state);
    }

    // -----------------------------
    // »óÅÂ º¯°æ
    // -----------------------------
    private void SetState(BoltState state)
    {
        currentState = state;

        switch (currentState)
        {
            case BoltState.Inserted:

                holeImage.SetActive(true);

                boltImage.gameObject.SetActive(true);
                boltImage.sprite = insertedBoltSprite;

                break;


            case BoltState.HalfInserted:

                holeImage.SetActive(true);

                boltImage.gameObject.SetActive(true);
                boltImage.sprite = halfInsertedBoltSprite;

                break;


            case BoltState.Empty:

                holeImage.SetActive(true);

                boltImage.gameObject.SetActive(false);

                break;
        }
    }

    // -----------------------------
    // ¹ÝÂë ³ª¿Â ³ª»ç Å¬¸¯
    // -----------------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentState != BoltState.HalfInserted)
            return;

        SetState(BoltState.Inserted);

        manager.NotifyBoltChanged();
    }

    // -----------------------------
    // ¹Ù´Ú ³ª»ç¸¦ ºó ±¸¸Û¿¡ µå·Ó
    // -----------------------------
    public void OnDrop(PointerEventData eventData)
    {
        if (currentState != BoltState.Empty)
            return;

        if (eventData.pointerDrag == null)
            return;

        DraggableBolt draggableBolt =
            eventData.pointerDrag.GetComponent<DraggableBolt>();

        if (draggableBolt == null)
            return;

        if (draggableBolt.IsUsed)
            return;

        // ¹Ù´Ú ³ª»ç Á¦°Å
        draggableBolt.UseBolt();

        // ºó ±¸¸Û ¡æ Á¤»ó ³ª»ç
        SetState(BoltState.Inserted);

        manager.NotifyBoltChanged();
    }
}
