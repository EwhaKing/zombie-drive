using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

public class BackSceneChanger : MonoBehaviour
{
    [SerializeField] private Button BackButton; 

    private void Start()
    {
        if (BackButton != null)
        {
            // 버튼을 누르면 밑에 있는 ChangeToBottomUIScene 함수가 실행됩니다.
            BackButton.onClick.AddListener(ChangeToBottomUIScene);
        }
    }

    private void ChangeToBottomUIScene()
    {
        // 이동할 씬의 정확한 이름을 적어줍니다. (대소문자 구분 필수!)
        SceneManager.LoadScene("BottomUI"); 
    }}