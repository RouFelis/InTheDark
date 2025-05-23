using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    public GameObject menuPanel; // 기본 메뉴
    public GameObject keySettingsPanel; // 키 설정 패널

    public delegate void PauseEvent();
    public event PauseEvent OnPause;
    public event PauseEvent OnResume;


    [SerializeField]private bool isEvenet = false;
    [SerializeField]private bool isPaused = false;

    public bool IsEvenet { get => isEvenet; set => isEvenet = value;  }
    public bool IsPaused { get => isPaused; set => isPaused = value;  }

    private Stack<GameObject> menuStack = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

	private void Start()
	{
        InitUI();
    }

    private void InitUI()
	{
        keySettingsPanel.SetActive(false);
        menuPanel.SetActive(false);
    }

	private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

	#region 퍼즈 관련
	public void TogglePause()
    {
        isPaused = !isPaused;
       
        if (isPaused && !isEvenet)
        {
            OnPause?.Invoke();
            ShowMenu(menuPanel);
        }
        else
        {
            OnResume?.Invoke();
            CloseCurrentMenu();
        }
    }

    public void HandleEscapeKey()
    {
        if (menuStack.Count > 1)
        {
            // 현재 메뉴 닫기 및 이전 메뉴로 돌아가기
            CloseCurrentMenu();
        }
        else
        {
            // 게임 일시정지 또는 재개
            TogglePause();
        }
    }

    public void ShowMenu(GameObject menu)
    {
        if (menuStack.Count > 0)
        {
            menuStack.Peek().SetActive(false);
        }

        menu.SetActive(true);
        menuStack.Push(menu);
    }

    public void CloseCurrentMenu()
    {
        if (menuStack.Count > 0)
        {
            menuStack.Pop().SetActive(false);
            if (menuStack.Count > 0)
            {
                menuStack.Peek().SetActive(true);
            }
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game clicked!");
        Application.Quit(); // 에디터에서는 종료되지 않음
    }
    #endregion

    public void OpenKeySettingPanel()
	{
        ShowMenu(keySettingsPanel);
    }

}
