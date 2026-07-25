using UnityEngine;
using UnityEngine.UI;

public class LightToggle : MonoBehaviour
{
    private bool isOn = false;
    public Image lightImage;
    public Color onColor = Color.yellow;
    public Color offColor = Color.gray;

    void Start()
    {
        UpdateLight();
    }

    public void OnLightClick()
    {
        isOn = !isOn;
        UpdateLight();
        Debug.Log(isOn ? "불 켜짐" : "불 꺼짐");
    }

    void UpdateLight()
    {
        lightImage.color = isOn ? onColor : offColor;
    }
}