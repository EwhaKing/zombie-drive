using UnityEngine;

public class RepairShopOrientation : MonoBehaviour
{
    private void Awake()
    {
        // 세로 회전 금지
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        // 가로 방향 허용
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // 가로 화면
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
}