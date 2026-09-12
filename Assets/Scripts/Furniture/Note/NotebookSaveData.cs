using System;

// JsonUtility로 저장하기 위해 슬롯 배열을 감싸는 클래스
[Serializable]
public class NotebookSaveData
{
    public NotebookEntry[] slots;
}