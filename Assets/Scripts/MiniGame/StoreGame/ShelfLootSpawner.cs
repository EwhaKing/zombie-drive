using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 선반 위치에 맞춰 아이템을 랜덤 배치하고, 클릭하면 바로 획득
public class ShelfLootSpawner : MonoBehaviour
{
    [Header("참조")]
    public RectTransform spawnArea;          // 선반 배경 영역 (WipeSmudgeMinigame의 spawnArea와 같은 방식)
    public List<ItemData> possibleItems;

    [Header("선반(칸) 설정 - 배경 이미지의 선반 위치에 맞춰 조절")]
    // (아래쪽 비율, 위쪽 비율): spawnArea 높이 기준 0(맨 아래) ~ 1(맨 위)
    public Vector2[] shelfBands = new Vector2[]
    {
        new Vector2(0.05f, 0.30f),
        new Vector2(0.38f, 0.63f),
        new Vector2(0.70f, 0.95f)
    };

    [Header("배치 개수 (선반 한 칸당)")]
    public int minPerShelf = 0;
    public int maxPerShelf = 2;
    public int slotsPerShelf = 4;            // 한 칸을 가로로 나눌 셀 수 (셀당 최대 1개 → 겹침 방지)
    public float sidePadding = 60f;

    [Header("아이템 UI 크기")]
    public float itemTargetSize = 130f;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private bool isInitialized = false;

    void OnEnable()
    {
        if (isInitialized) return; // 재진입 시 리셋 방지
        Spawn();
        isInitialized = true;
    }

    public void Spawn()
    {
        Clear();
        if (spawnArea == null || possibleItems == null || possibleItems.Count == 0) return;

        Rect areaRect = spawnArea.rect;
        Vector2 center = areaRect.center;
        int cells = Mathf.Max(1, slotsPerShelf);
        float cellW = (areaRect.width - sidePadding * 2f) / cells;

        foreach (Vector2 band in shelfBands)
        {
            float bandMinY = areaRect.yMin + areaRect.height * Mathf.Min(band.x, band.y);
            float bandMaxY = areaRect.yMin + areaRect.height * Mathf.Max(band.x, band.y);

            int count = Mathf.Min(Random.Range(minPerShelf, maxPerShelf + 1), cells);
            if (count <= 0) continue;

            // 셀 번호를 섞어서 앞에서 count개만 사용 (중복 없음)
            List<int> cellOrder = new List<int>();
            for (int i = 0; i < cells; i++) cellOrder.Add(i);
            for (int i = cellOrder.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (cellOrder[i], cellOrder[j]) = (cellOrder[j], cellOrder[i]);
            }

            for (int n = 0; n < count; n++)
            {
                ItemData item = LootService.PickWeighted(possibleItems);
                if (item == null) continue;

                float size = itemTargetSize * item.itemScale;
                int c = cellOrder[n];

                float cellCenterX = areaRect.xMin + sidePadding + cellW * (c + 0.5f);
                float jitterX = Mathf.Max(0f, (cellW - size) * 0.5f);
                float x = cellCenterX + Random.Range(-jitterX, jitterX);

                float yMin = bandMinY + size * 0.5f;
                float yMax = bandMaxY - size * 0.5f;
                float y = (yMin < yMax) ? Random.Range(yMin, yMax) : (bandMinY + bandMaxY) * 0.5f;

                SpawnItem(item, new Vector2(x, y) - center);
            }
        }
    }

    void SpawnItem(ItemData item, Vector2 pos)
    {
        GameObject obj = new GameObject("ShelfItem", typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(spawnArea, false);
        spawned.Add(obj);

        Image img = obj.GetComponent<Image>();
        img.sprite = item.icon;
        img.preserveAspect = true;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;

        if (item.icon != null)
        {
            float w = item.icon.rect.width;
            float h = item.icon.rect.height;
            rect.sizeDelta = (w > h)
                ? new Vector2(itemTargetSize, itemTargetSize * (h / w))
                : new Vector2(itemTargetSize * (w / h), itemTargetSize);
        }
        else
        {
            rect.sizeDelta = new Vector2(itemTargetSize, itemTargetSize);
        }
        rect.localScale = new Vector3(item.itemScale, item.itemScale, 1f);

        obj.GetComponent<Button>().onClick.AddListener(() => OnItemClicked(item, obj));
    }

    void OnItemClicked(ItemData item, GameObject obj)
    {
        LootResult result = LootService.TryAddToInventory(item);

        if (result == LootResult.Success)
        {
            if (LootPopup.Instance != null) LootPopup.Instance.ShowAcquired(item);
            spawned.Remove(obj);
            Destroy(obj);
        }
        else if (result == LootResult.InventoryFull)
        {
            // 아이템은 그대로 두고, 가방을 비운 뒤 다시 클릭하면 획득
            if (LootPopup.Instance != null) LootPopup.Instance.ShowFull();
        }
    }

    void Clear()
    {
        foreach (var o in spawned) if (o != null) Destroy(o);
        spawned.Clear();
    }
}