using UnityEngine;

public class ScreenOrientationSetter : MonoBehaviour
{
    public ScreenOrientation targetOrientation = ScreenOrientation.Portrait;

    void Awake()
    {
        Screen.orientation = targetOrientation;
    }
}