using UnityEngine;

public class SceneOrientation : MonoBehaviour
{
    public enum OrientationType
    {
        Portrait,
        Landscape
    }

    [Header("이 씬의 화면 방향")]
    public OrientationType orientation;

    private void Awake()
    {
        SetOrientation();
    }

    private void SetOrientation()
    {
        if (orientation == OrientationType.Portrait)
        {
            // 세로 화면
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else
        {
            // 가로 화면
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
    }
}