using UnityEngine;

public class NavigationInteraction : MonoBehaviour
{
    [Header("네비게이션 팝업")]
    [SerializeField] private GameObject navigationPopup;

    private void Start()
    {
        if (navigationPopup != null)
        {
            navigationPopup.SetActive(false);
        }
    }

    public void OpenNavigation()
    {
        if (navigationPopup != null)
        {
            navigationPopup.SetActive(true);
        }
    }

    public void CloseNavigation()
    {
        if (navigationPopup != null)
        {
            navigationPopup.SetActive(false);
        }
    }
}