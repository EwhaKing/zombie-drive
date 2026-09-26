using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FrontWindowObjectSpawner : MonoBehaviour
{
    [Header("생성 Prefab")]
    [SerializeField]
    private GameObject objectPrefab;

    [Header("랜덤 이미지")]
    [SerializeField]
    private Sprite[] sprites;

    [Header("생성 간격")]
    [SerializeField]
    private float minSpawnInterval = 2f;

    [SerializeField]
    private float maxSpawnInterval = 5f;

    [Header("멀리 있는 시작 위치")]
    [SerializeField]
    private float startY = 80f;

    [SerializeField]
    private float minStartX = -60f;

    [SerializeField]
    private float maxStartX = 60f;

    [Header("가까워졌을 때 위치")]
    [SerializeField]
    private float endY = -250f;

    [SerializeField]
    private float horizontalSpread = 2f;

    [Header("크기")]
    [SerializeField]
    private float startScale = 0.15f;

    [SerializeField]
    private float endScale = 1.2f;

    [Header("이동 시간")]
    [SerializeField]
    private float minDuration = 3f;

    [SerializeField]
    private float maxDuration = 5f;

    [Header("움직임")]
    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float wait =
                Random.Range(
                    minSpawnInterval,
                    maxSpawnInterval
                );

            yield return new WaitForSeconds(wait);

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

        Image image =
            newObject.GetComponent<Image>();

        RectTransform rect =
            newObject.GetComponent<RectTransform>();

        FrontWindowMovingObject movement =
            newObject.GetComponent<FrontWindowMovingObject>();

        // 랜덤 Sprite
        image.sprite =
            sprites[Random.Range(0, sprites.Length)];

        image.SetNativeSize();

        float startX =
            Random.Range(
                minStartX,
                maxStartX
            );

        Vector2 startPosition =
            new Vector2(
                startX,
                startY
            );

        // 가까워질수록 바깥쪽으로 살짝 퍼짐
        float endX =
            startX * horizontalSpread;

        Vector2 endPosition =
            new Vector2(
                endX,
                endY
            );

        float duration =
            Random.Range(
                minDuration,
                maxDuration
            );

        movement.Setup(
            startPosition,
            endPosition,
            startScale,
            endScale,
            duration,
            movementCurve
        );
    }
}