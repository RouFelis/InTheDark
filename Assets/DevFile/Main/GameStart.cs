using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{


    // ��ư Ŭ�� �̺�Ʈ�� ������ �޼���
    public void LoadScene()
    {
        SceneManager.LoadScene("Lobby");
    }
}
