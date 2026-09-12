using System;

// 슬롯 한 칸(STORY 한 칸)의 상태를 담는 데이터
[Serializable]
public class NotebookEntry
{
    public bool isFilled;    // 아직 안 눌렀으면 false, 눌러서 기록되면 true
    public string title;     // 슬롯에 표시될 제목 (ex STORY 1)
    public string dateText;  // 기록된 실제 날짜 (ex 2026-09-12)

    public NotebookEntry(string title)
    {
        this.title = title;
        this.isFilled = false;
        this.dateText = "";
    }

    // 슬롯을 누른 순간 호출되어 실제 날짜로 채움
    public void Fill(string date)
    {
        isFilled = true;
        dateText = date;
    }
}