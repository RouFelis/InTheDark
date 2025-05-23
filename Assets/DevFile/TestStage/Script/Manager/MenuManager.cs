using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    public GameObject menuPanel; // �⺻ �޴�
    public GameObject keySettingsPanel; // Ű ���� �г�

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

	#region ���� ����
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
            // ���� �޴� �ݱ� �� ���� �޴��� ���ư���
            CloseCurrentMenu();
        }
        else
        {
            // ���� �Ͻ����� �Ǵ� �簳
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
        Application.Quit(); // �����Ϳ����� ������� ����
    }
    #endregion

    public void OpenKeySettingPanel()
	{
        ShowMenu(keySettingsPanel);
    }

}
