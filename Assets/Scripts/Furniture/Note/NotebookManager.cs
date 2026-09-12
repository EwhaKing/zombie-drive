using System;
using System.IO;
using UnityEngine;

public class NotebookManager : MonoBehaviour
{
    [Header("팝업 연결")]
    public GameObject notebookPopup; // 노트 팝업 전체 패널

    [Header("씬에 미리 배치해둔 슬롯 4개를 드래그")]
    public NotebookSlotUI[] slots;

    [Header("'불러오기/저장하기' 작은 메뉴")]
    public GameObject slotActionPopup; // 버튼 2개(불러오기, 저장하기)가 들어있는 패널

    private NotebookSaveData saveData;
    private int selectedSlotIndex = -1; // 지금 메뉴가 떠있는 칸이 몇 번인지 기억해두는 변수

    // 안드로이드/iOS 어디서든 앱 전용 저장 폴더 (별도 권한 필요 없음)
    private string SavePath => Path.Combine(Application.persistentDataPath, "notebook_save.json");

    private void Awake()
    {
        LoadFromDisk();

        if (slotActionPopup != null)
        {
            slotActionPopup.SetActive(false); // 시작할 땐 메뉴가 꺼져있어야 함
        }
    }

    // 노트 오브젝트를 클릭했을 때 호출
    public void OnNotebookClick()
    {
        notebookPopup.SetActive(true);
        RefreshAllSlots();
    }

    public void OnCloseClick()
    {
        notebookPopup.SetActive(false);
    }

    // 슬롯(STORY 칸)을 눌렀을 때 NotebookSlotUI가 호출하는 함수
    public void OnSlotClicked(int index, string defaultTitle)
    {
        if (index < 0 || index >= saveData.slots.Length) return;

        NotebookEntry entry = saveData.slots[index];

        if (entry.isFilled)
        {
            // 불러오기/저장하기 메뉴를 띄움
            OpenSlotActionMenu(index);
            return;
        }

        entry.title = defaultTitle;
        entry.Fill(DateTime.Now.ToString("yyyy-MM-dd")); // 실제 오늘 날짜로 기록

        SaveToDisk();
        slots[index].SetData(entry); // 이 칸만 바로 화면 갱신
    }

    // 이미 채워진 칸을 눌렀을 때 메뉴를 띄움
    private void OpenSlotActionMenu(int index)
    {
        selectedSlotIndex = index;

        if (slotActionPopup != null)
        {
            slotActionPopup.SetActive(true);
        }
    }

    // 불러오기 버튼 지금은 그냥 메뉴 닫힘 (x)버튼과 함께 사용중 
    public void OnClickLoadButton()
    {
        CloseSlotActionMenu();
    }

    // 저장하기 버튼
    public void OnClickSaveButton()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= saveData.slots.Length)
        {
            CloseSlotActionMenu();
            return;
        }

        NotebookEntry entry = saveData.slots[selectedSlotIndex];
        entry.Fill(DateTime.Now.ToString("yyyy-MM-dd")); 

        SaveToDisk();
        slots[selectedSlotIndex].SetData(entry); 

        CloseSlotActionMenu();
    }

    private void CloseSlotActionMenu()
    {
        selectedSlotIndex = -1;

        if (slotActionPopup != null)
        {
            slotActionPopup.SetActive(false);
        }
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                // NotebookInteraction 인스펙터에서 Slots 확인
                Debug.LogWarning($"[Notebook] Slots 배열의 {i}번 칸이 비어있습니다. 인스펙터에서 연결 필요");
                continue;
            }
            slots[i].SetData(saveData.slots[i]);
        }
    }

    private void SaveToDisk()
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
    }

    private void LoadFromDisk()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            saveData = JsonUtility.FromJson<NotebookSaveData>(json);
        }

        // 최초 실행 대비 슬롯 새로 채워넣기 
        if (saveData == null || saveData.slots == null || saveData.slots.Length != slots.Length)
        {
            saveData = new NotebookSaveData();
            saveData.slots = new NotebookEntry[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                saveData.slots[i] = new NotebookEntry("STORY " + (i + 1));
            }
        }
    }
    // 
    // 테스트용 저장 데이터 초기화
    [ContextMenu("저장 데이터 초기화 (테스트용)")]
    public void ResetAllData()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        saveData = null;    
        LoadFromDisk();     
        RefreshAllSlots();  // 노트가 열려있는 상태라면 화면 바로 갱식
        Debug.Log("[Notebook] 저장 데이터를 초기화했습니다.");
    }
}