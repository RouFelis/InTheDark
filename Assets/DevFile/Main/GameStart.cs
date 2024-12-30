using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{


    // 버튼 클릭 이벤트에 연결할 메서드
    public void LoadScene()
    {
        SceneManager.LoadScene("Lobby");
    }
}
