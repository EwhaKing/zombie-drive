using UnityEngine;

public class RadioToggle : MonoBehaviour
{
    // 나중에 실제 사운드 파일이 생기면 여기에 연결
    public AudioSource audioSource;
    public AudioClip[] tracks;
    private int currentTrackIndex = 0;

    public void OnRadioClick()
    {
        if (tracks == null || tracks.Length == 0)
        {
            Debug.Log("라디오 클릭 - 다음 곡으로 변경 (사운드 파일 아직 없음)");
            return;
        }

        currentTrackIndex = (currentTrackIndex + 1) % tracks.Length;

        if (audioSource != null)
        {
            audioSource.clip = tracks[currentTrackIndex];
            audioSource.Play();
        }

        Debug.Log("라디오 트랙 변경: " + currentTrackIndex);
    }
}