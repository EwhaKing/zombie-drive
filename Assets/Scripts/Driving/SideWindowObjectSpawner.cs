using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SideWindowObjectSpawner : MonoBehaviour
{
    [Header("생성할 UI Prefab")]
    [SerializeField]
    private GameObject objectPrefab;

    [Header("랜덤으로 사용할 이미지")]
    [SerializeField]
    private Sprite[] sprites;

    [Header("이동 방향")]
    [SerializeField]
    private bool moveRight = false;

    [Header("생성 시간 간격")]
    [SerializeField]
    private float minSpawnInterval = 2f;

    [SerializeField]
    private float maxSpawnInterval = 5f;

    [Header("이동 속도")]
    [SerializeField]
    private float minSpeed = 120f;

    [SerializeField]
    private float maxSpeed = 170f;

    [Header("생성 높이")]
    [SerializeField]
    private float minY = -100f;

    [SerializeField]
    private float maxY = 100f;

    [Header("크기")]
    [SerializeField]
    private float minScale = 0.7f;

    [SerializeField]
    private float maxScale = 1.1f;

    [Header("화면 밖 여유 거리")]
    [SerializeField]
    private float spawnPadding = 100f;

    private RectTransform layerRect;

    private void Start()
    {
        layerRect = GetComponent<RectTransform>();

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        // RectTransform 계산이 끝날 때까지 한 프레임 기다림
        yield return null;

        while (true)
        {
            float waitTime =
                Random.Range(
                    minSpawnInterval,
                    maxSpawnInterval
                );

            yield return new WaitForSeconds(waitTime);

            SpawnObject();
        }
    }

    private void SpawnObject()
    {
        if (objectPrefab == null)
            return;

        if (sprites == null || sprites.Length == 0)
            return;

        GameObject newObject =
            Instantiate(
                objectPrefab,
                transform
            );

        RectTransform rect =
            newObject.GetComponent<RectTransform>();

        Image image =
            newObject.GetComponent<Image>();

        SideWindowMovingObject movement =
            newObject.GetComponent<SideWindowMovingObject>();

        // 랜덤 이미지
        image.sprite =
            sprites[Random.Range(0, sprites.Length)];

        image.SetNativeSize();

        // 랜덤 크기
        float scale =
            Random.Range(minScale, maxScale);

        rect.localScale =
            Vector3.one * scale;

        float halfWidth =
            layerRect.rect.width * 0.5f;

        float spawnX;

        float destroyX;

        if (moveRight)
        {
            // 왼쪽 밖에서 생성
            spawnX =
                -halfWidth - spawnPadding;

            // 오른쪽 밖에서 삭제
            destroyX =
                halfWidth + spawnPadding;
        }
        else
        {
            // 오른쪽 밖에서 생성
            spawnX =
                halfWidth + spawnPadding;

            // 왼쪽 밖에서 삭제
            destroyX =
                -halfWidth - spawnPadding;
        }

        float spawnY =
            Random.Range(minY, maxY);

        rect.anchoredPosition =
            new Vector2(
                spawnX,
                spawnY
            );

        float speed =
            Random.Range(
                minSpeed,
                maxSpeed
            );

        movement.Setup(
            speed,
            moveRight,
            destroyX
        );
    }
}