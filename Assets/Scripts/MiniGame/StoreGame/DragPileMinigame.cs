using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragPileMinigame : MonoBehaviour, IDragHandler
{
    public FridgeMiniGameManager gameManager;
    public RectTransform dragTarget; // 치울 더미 오브젝트
    public RectTransform spawnArea;
    
    [Header("아이템 랜덤 설정")]
    public int minRewardCount = 0; 
    public int maxRewardCount = 2; 
    public List<ItemData> possibleFoods; 
    
    [Header("아이템 UI 크기")]
    public Vector2 itemSize = new Vector2(80f, 80f); 

    private Vector2 startPos;
    private bool isInitialized = false;

    void Awake()
    {
        if (dragTarget != null)
        {
            startPos = dragTarget.anchoredPosition;
        }
    }

    void OnEnable()
    {
        if (isInitialized) return;

        // 미니게임 시작 시 더미 아래에 아이템 미리 생성
        SpawnHiddenItemsUnderPile();
        isInitialized = true;
    }

    void SpawnHiddenItemsUnderPile()
    {
        int finalRewardCount = Random.Range(minRewardCount, maxRewardCount + 1);

        for (int i = 0; i < finalRewardCount; i++)
        {
            ItemData randomItem = gameManager.GetRandomItem(possibleFoods);
            if (randomItem != null)
            {
                SpawnItem(randomItem);
            }
        }

        // [핵심] 더미(dragTarget)가 아이템보다 항상 위에 오도록 레이어 순서 맨 위로 변경
        if (dragTarget != null)
        {
            dragTarget.SetAsLastSibling();
        }
    }

    void SpawnItem(ItemData item)
    {
        Transform parent = (spawnArea != null) ? spawnArea : transform.parent;

        GameObject foodObj = new GameObject("HiddenItem", typeof(RectTransform), typeof(Image), typeof(Button));
        foodObj.transform.SetParent(parent, false);

        Image img = foodObj.GetComponent<Image>();
        img.sprite = item.icon;
        img.preserveAspect = true;

        RectTransform rect = foodObj.GetComponent<RectTransform>();
        // 더미 아래 범위 안에서 약간 무작위로 배치
        rect.anchoredPosition = startPos + new Vector2(Random.Range(-50f, 50f), Random.Range(-50f, 50f));
        rect.sizeDelta = itemSize;

        Button btn = foodObj.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            gameManager.AddToInventory(item);
            Destroy(foodObj);
        });
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragTarget == null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        float scale = (canvas != null) ? canvas.scaleFactor : 1f;
        dragTarget.anchoredPosition += eventData.delta / scale;
    }
}