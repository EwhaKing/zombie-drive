using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotebookManager : MonoBehaviour
{
    public GameObject notebookPopup;
    public TMP_Text locationListText;

    private List<string> visitedLocations = new List<string>()
    {
        "정류장 A", "쉘터", "세이브 포인트"  // 테스트용 임시 데이터
    };

    public void OnNotebookClick()
    {
        notebookPopup.SetActive(true);
        UpdateLocationList();
    }

    public void OnCloseClick()
    {
        notebookPopup.SetActive(false);
    }

    void UpdateLocationList()
    {
        string result = "지나온 장소:\n";
        foreach (string location in visitedLocations)
        {
            result += "- " + location + "\n";
        }
        locationListText.text = result;
    }

    // 나중에 파밍 시스템에서 이 함수를 호출해서 실제 장소를 추가하면 됨
    public void AddLocation(string locationName)
    {
        visitedLocations.Add(locationName);
    }
}
