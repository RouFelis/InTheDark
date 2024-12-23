using UnityEngine;
using System;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    [SerializeField] private GameObject pauseMenuUI;

    public event Action OnPause;   // Pause �̺�Ʈ
    public event Action OnResume; // Resume �̺�Ʈ

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            ActivatePause();
            OnPause?.Invoke(); // Pause �̺�Ʈ ȣ��
        }
        else
        {
            ResumeGame();
            OnResume?.Invoke(); // Resume �̺�Ʈ ȣ��
        }
    }

    private void ActivatePause()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        Cursor.visible = false;
    }
}
