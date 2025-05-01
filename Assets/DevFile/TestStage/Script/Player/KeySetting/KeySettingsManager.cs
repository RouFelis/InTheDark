using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class KeySettingsManager : MonoBehaviour
{
    public static KeySettingsManager Instance { get; private set; }

    public enum KeyName { Interact, Drop, UseItem, ScanKey, Light };

    [System.Serializable]
    public class KeySettingField
    {
        public KeyName keyName; // Ű �̸�
        public TMP_Text keyText; // Ű�� ǥ���� �ؽ�Ʈ
        public Button keyButton; // Ű ���� ��ư
    }

    private KeySettingField activeKeySettingField = null;
    public List<KeySettingField> keySettingFields; // �ν����Ϳ��� Ű �̸��� ��ǲ �ʵ带 ����
    public Button applyButton; // ���� ��ư
    public Button cancelButton; // ��� ��ư
    public GameObject keySettingsPanel; // Ű ���� �г�


    private Dictionary<string, KeyCode> keySettings = new Dictionary<string, KeyCode>(); // Ű ������ �����ϴ� ��ųʸ�
    private string settingsFilePath; // Ű ���� ���� ���
    public TMP_Dropdown languageDropdown; // TMP ��Ӵٿ� ���

    // ��������Ʈ�� �̺�Ʈ ���� (Ű���� �ٲ��...)
    public delegate void OnKeyCodeChanged();
    public event OnKeyCodeChanged KeyCodeChanged;


    [Header("Keys")]
    [SerializeField]private KeyCode interactKey;
    [SerializeField]private KeyCode dropKey;
    [SerializeField]private KeyCode useItemKey;
    [SerializeField]private KeyCode scanKey;
    [SerializeField]private KeyCode lightKey;
    [SerializeField]private KeyCode sprintKey;

    [Header("Senstive")]
    [SerializeField] private float mouseSenstive = 2f;
    [SerializeField] private float minSensitivity = 0.1f;
    [SerializeField] private float maxSensitivity = 10f;
    [SerializeField] private TMP_InputField sensitivityInput; // ���� �Է� �ʵ� (TMP_InputField ���)
    [SerializeField] public Slider sensitivitySlider; // �����̴� ����
    public Player localPlayer;

    private bool isPaused = false;

    #region �̰� ���� �����Ű��� Ű�� ��ȵǼ� �̷� ��. ���� �ȵ�� �̰������ �׳� ��ųʸ��� �ҷ������ϸ��. �������� ����
    public KeyCode InteractKey {         
        get { return interactKey; }
        set
        {
            if (interactKey != value)
            {
                interactKey = value;
                KeyCodeChanged?.Invoke();  // �� ���� �� �̺�Ʈ ȣ��
            }
        }
    }
    public KeyCode DropKey {         
        get { return dropKey; }
        set
        {
            if (dropKey != value)
            {
                dropKey = value;
                KeyCodeChanged?.Invoke();  // �� ���� �� �̺�Ʈ ȣ��
            }
        }
    }
    public KeyCode UseItemKey { 
        get { return useItemKey; }
        set
        {
            if (useItemKey != value)
            {
                useItemKey = value;
                KeyCodeChanged?.Invoke();  // �� ���� �� �̺�Ʈ ȣ��
            }
        }
    }       
    public KeyCode ScanKey
    { 
        get { return scanKey; }
        set
        {
            if (scanKey != value)
            {
                scanKey = value;
                KeyCodeChanged?.Invoke();  // �� ���� �� �̺�Ʈ ȣ��
            }
        }
    }
    public KeyCode LightKey
    {
        get { return lightKey; }
        set
        {
            if (lightKey != value)
            {
                lightKey = value;
                KeyCodeChanged?.Invoke();  // �� ���� �� �̺�Ʈ ȣ��
            }
        }
    }
    public KeyCode SprintKey
    {
        get { return sprintKey; }
        set
        {
            if (sprintKey != value)
            {
                sprintKey = value;
                KeyCodeChanged?.Invoke();  // �� ���� �� �̺�Ʈ ȣ��
            }
        }
    }
    #endregion

    private void Start()
    {
        Instance = this;
        // ���� ���� ��θ� ����
        settingsFilePath = Path.Combine(Application.persistentDataPath, "keysettings.json");

        LoadKeySettings(); // Ű ���� �ε�
        InitializeKeySettingUI(); // Ű ���� UI �ʱ�ȭ

        applyButton.onClick.AddListener(ApplyKeySettings); // ���� ��ư�� ������ �߰�
        cancelButton.onClick.AddListener(CancelKeySettings); // ��� ��ư�� ������ �߰�

        sensitivitySlider.onValueChanged.AddListener(UpdateInputField); //�����̴� �̺�Ʈ �߰�
        sensitivityInput.onEndEdit.AddListener(UpdateSliderFromInput); //�����̴� �̺�Ʈ �߰�

        keySettingsPanel.SetActive(false); // �ʱ� ���´� ��Ȱ��ȭ
        SetLanguage();
        SetKey();
    }

    private void Update()
    {
        if (activeKeySettingField != null)
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    UpdateKeySetting(activeKeySettingField, key);
                    activeKeySettingField = null;
                    break;
                }
            }
        }
    }

  
    private void SetKey()
	{
        InteractKey = GetKey("Interact");
        DropKey = GetKey("Drop");
        UseItemKey = GetKey("UseItem");
        ScanKey = GetKey("ScanKey");
        lightKey = GetKey("Light");
        sprintKey = GetKey("Sprint");
        Debug.Log("KeySetting2 �� ã�ҽ��ϴ�.");
    }

    private void ApplyKeySettings()
    {
        // �� Ű ���� �ʵ带 ��ȸ�ϸ� ���� ����
        foreach (KeySettingField field in keySettingFields)
        {
            if (TryGetKeyCode(field.keyText.ToString(), out KeyCode newKey))
            {
                keySettings[field.keyName.ToString()] = newKey;
            }
            else
            {
                Debug.LogError("Invalid key entered for " + field.keyName.ToString()); // �߸��� Ű �Է� �� ���� ���
            }
        }

        MenuManager.Instance.CloseCurrentMenu();
        SaveKeySettings(); // Ű ���� ����
        KeyCodeChanged?.Invoke();  // �� ���� �� �̺�Ʈ ȣ��
        SetKey(); // Ű ���� ����
    }

    private void CancelKeySettings()
    {
        MenuManager.Instance.CloseCurrentMenu();
    }

    public KeyCode GetKey(string name)
    {
        return keySettings.ContainsKey(name) ? keySettings[name] : KeyCode.None; // Ű ���� ��ȯ
    }

    private void LoadKeySettings()
    {
        if (File.Exists(settingsFilePath))
        {
            string json = File.ReadAllText(settingsFilePath); // ���Ͽ��� JSON �б�
            KeySettingList keySettingList = JsonUtility.FromJson<KeySettingList>(json); // JSON�� ��ü�� ��ȯ
            keySettings = new Dictionary<string, KeyCode>(); // ��ųʸ� �ʱ�ȭ
            foreach (var keySetting in keySettingList.keySettings)
            {
                keySettings[keySetting.name] = keySetting.key; // Ű ���� �ε�
            }
        }
        else
        {
            // �⺻ Ű ���� �߰�
            keySettings["Interact"] = KeyCode.F;
            keySettings["Drop"] = KeyCode.Q;
            keySettings["UseItem"] = KeyCode.E;
            keySettings["ScanKey"] = KeyCode.Tab;
            keySettings["Light"] = KeyCode.R;
        }
    }

    private void SaveKeySettings()
    {
        // Ű ���� ����Ʈ ��ü ����
        KeySettingList keySettingList = new KeySettingList
        {
            keySettings = new List<KeySetting>()
        };

        // ��ųʸ��� Ű ������ ����Ʈ�� �߰�
        foreach (var kvp in keySettings)
        {
            keySettingList.keySettings.Add(new KeySetting
            {
                name = kvp.Key,
                key = kvp.Value
            });
        }

        // JSON���� ��ȯ�Ͽ� ���Ͽ� ����
        string json = JsonUtility.ToJson(keySettingList, true);
        File.WriteAllText(settingsFilePath, json);
    }

    private void InitializeKeySettingUI()
    {
        // �� Ű ���� �ʵ带 �ʱ�ȭ
        foreach (KeySettingField field in keySettingFields)
        {
            if (keySettings.ContainsKey(field.keyName.ToString()))
            {
                // ��ư�� ǥ���� �ؽ�Ʈ ����
                field.keyText.text = keySettings[field.keyName.ToString()].ToString();

                // ��ư Ŭ�� �̺�Ʈ ����
                field.keyButton.onClick.AddListener(() => OnKeyButtonClicked(field));
            }
        }
    }

    private void OnKeyButtonClicked(KeySettingField field)
    {
        activeKeySettingField = field; // ���� Ȱ��ȭ�� �ʵ� ����
        Debug.Log("Press a key to set for: " + field.keyName);
    }
    private void UpdateKeySetting(KeySettingField field, KeyCode newKey)
    {
        string keyName = field.keyName.ToString();

        if (keySettings.ContainsKey(keyName))
        {
            keySettings[keyName] = newKey; // Ű ���� ������Ʈ
            field.keyText.text = newKey.ToString(); // �ؽ�Ʈ ������Ʈ
        }
    }

    private bool TryGetKeyCode(string keyString, out KeyCode keyCode)
    {
        keyCode = KeyCode.None;
        // ���ڿ��� KeyCode�� ��ȯ �õ�
        if (System.Enum.TryParse(keyString, true, out KeyCode parsedKeyCode))
        {
            keyCode = parsedKeyCode;
            return true;
        }
        return false;
    }

    private void SetLanguage()
	{
        // TMP ��Ӵٿ� �ʱ�ȭ (������ ������ ����� ��Ӵٿ �߰�)
        languageDropdown.options.Clear();
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(locale.Identifier.CultureInfo.NativeName));
        }

        // ���� �� ���� ��Ӵٿ� ���� �ʱ�ȭ
        languageDropdown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        languageDropdown.onValueChanged.AddListener(ChangeLanguage);

    }

    public void ChangeLanguage(int index)
    {
        // ���õ� �ε����� ���� ��� ����
        Locale selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        StartCoroutine(SetLocale(selectedLocale));
    }

    IEnumerator SetLocale(Locale locale)
    {
        yield return LocalizationSettings.InitializationOperation; // ������ �ʱ�ȭ ���
        LocalizationSettings.SelectedLocale = locale; // ������ �����Ϸ� ����
    }

	#region MouseSenestive
	public void senestiveInit()
	{
        // �����̴� �ʱ�ȭ
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = mouseSenstive;
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }

        // �÷��̾� ��Ʈ�ѷ� �ʱ�ȭ
        if (localPlayer != null)
        {
            localPlayer.SetMouseSensitivity(mouseSenstive);
        }
    }

    public void SetSensitivity(float sensitivity)
    {
        if (localPlayer != null)
        {
            localPlayer.SetMouseSensitivity(sensitivity);
        }
    }

    // �����̴� ���� ����Ǹ� InputField ������Ʈ
    private void UpdateInputField(float value)
    {
        sensitivityInput.text = value.ToString("0.0"); // �Ҽ��� �� �ڸ����� ǥ��
        if (localPlayer != null)
        {
            localPlayer.SetMouseSensitivity(value);
        }
    }

    // InputField ���� ����Ǹ� �����̴� �� ������Ʈ
    private void UpdateSliderFromInput(string input)
    {
        if (float.TryParse(input, out float value))
        {
            // �ּ�/�ִ� �� ���� ����
            value = Mathf.Clamp(value, minSensitivity, maxSensitivity);
            sensitivitySlider.value = value; // �����̴� �� ����
        }
        else
        {
            // ���ڰ� �ƴ� ��� �⺻ ������ ����
            sensitivityInput.text = sensitivitySlider.value.ToString("0");
        }
    }

    #endregion
    #region Ű��
    [System.Serializable]
    private class KeySetting
    {
        public string name; // Ű �̸�
        public KeyCode key; // Ű ��
    }

    [System.Serializable]
    private class KeySettingList
    {
        public List<KeySetting> keySettings; // Ű ���� ����Ʈ
    }

	#endregion
}
