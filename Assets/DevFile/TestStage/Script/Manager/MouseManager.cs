using UnityEngine;
using System;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    [SerializeField] private GameObject pauseMenuUI;

    public event Action OnPause;   // Pause 이벤트
    public event Action OnResume; // Resume 이벤트

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
            OnPause?.Invoke(); // Pause 이벤트 호출
        }
        else
        {
            ResumeGame();
            OnResume?.Invoke(); // Resume 이벤트 호출
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
