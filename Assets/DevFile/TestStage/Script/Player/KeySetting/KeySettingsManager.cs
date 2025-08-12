using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Unity.Netcode;

public class KeySettingsManager : MonoBehaviour
{
    public static KeySettingsManager Instance { get; private set; }

    public enum KeyName { Interact, Drop, UseItem, ScanKey, Light , Sprint, Shop, Mic };

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
    [SerializeField]private KeyCode shopKey;
    [SerializeField]private KeyCode micKey;

    [Header("Senstive")]
    [SerializeField] private float mouseSenstive = 2f;
    [SerializeField] private float minSensitivity = 0.1f;
    [SerializeField] private float maxSensitivity = 10f;
    [SerializeField] private TMP_InputField sensitivityInput; // ���� �Է� �ʵ� (TMP_InputField ���)
    [SerializeField] public Slider sensitivitySlider; // �����̴� ����
    public Player localPlayer;

    public bool isEveryEvent = false;

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
    public KeyCode ShopKey
    {
        get { return shopKey; }
        set
        {
            if (shopKey != value)
            {
                shopKey = value;
                KeyCodeChanged?.Invoke();  // �� ���� �� �̺�Ʈ ȣ��
            }
        }
    }
    public KeyCode MicKey
    {
        get { return micKey; }
        set
        {
            if (micKey != value)
            {
                micKey = value;
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

        applyButton.onClick.AddListener(ApplyKeySettings); // ���� ��ư ������ �߰�
        cancelButton.onClick.AddListener(CancelKeySettings); // ��� ��ư�� ������ �߰�

        sensitivitySlider.onValueChanged.AddListener(UpdateInputField); //�����̴� �̺�Ʈ �߰�
        sensitivityInput.onEndEdit.AddListener(UpdateSliderFromInput); //�����̴� �̺�Ʈ �߰�
        senestiveSliderInit();
        Debug.Log("�׽�Ʈ 33333333333");

        keySettingsPanel.SetActive(false); // �ʱ� ���´� ��Ȱ��ȭ
        Debug.Log("�׽�Ʈ 2222222222");
        SetLanguage();
        Debug.Log("�׽�Ʈ 1111111111111");
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
        LightKey = GetKey("Light");
        SprintKey = GetKey("Sprint");
        ShopKey = GetKey("Shop");
        MicKey = GetKey("Mic");
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
        // �⺻ Ű ������ �̸� ����
        Dictionary<string, KeyCode> defaultKeySettings = new Dictionary<string, KeyCode>
    {
        { "Interact", KeyCode.F },
        { "Drop", KeyCode.Q },
        { "UseItem", KeyCode.E },
        { "ScanKey", KeyCode.T },
        { "Light", KeyCode.R },
        { "Sprint", KeyCode.LeftShift },
        { "Shop", KeyCode.Tab },
        { "Mic", KeyCode.M }
    };

        keySettings = new Dictionary<string, KeyCode>();

        if (File.Exists(settingsFilePath))
        {
            string json = File.ReadAllText(settingsFilePath);
            KeySettingList keySettingList = JsonUtility.FromJson<KeySettingList>(json);

            foreach (var entry in defaultKeySettings)
            {
                // �ش� Ű�� JSON�� �����ϴ��� Ȯ���ϰ�, �����ϸ� ���
                var loadedSetting = keySettingList.keySettings.Find(k => k.name == entry.Key);
                if (loadedSetting != null && loadedSetting.key != KeyCode.None)
                {
                    keySettings[entry.Key] = loadedSetting.key;
                }
                else
                {
                    // ���ų� None�̸� �⺻�� ���
                    keySettings[entry.Key] = entry.Value;

                    Debug.Log($"NonKeyCode : {entry.Key} , Value : {entry.Value}");
                }
            }

            // ���콺 ���� ����
            mouseSenstive = Mathf.Clamp(keySettingList.mouseSensitivity, minSensitivity, maxSensitivity);
            if (sensitivitySlider != null)
            {
                sensitivitySlider.value = mouseSenstive;
            }
        }
        else
        {
            // ������ ������ �⺻�� ���
            keySettings = new Dictionary<string, KeyCode>(defaultKeySettings);
        }
    }

    private void SaveKeySettings()
    {
        // Ű ���� ����Ʈ ��ü ����
        KeySettingList keySettingList = new KeySettingList
        {
            keySettings = new List<KeySetting>(),
            mouseSensitivity = sensitivitySlider.value // �����̴��� ���� ���� ����
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
	public void senestiveSliderInit()
	{
        // �����̴� �ʱ�ȭ
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = mouseSenstive;
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
            UpdateInputField(mouseSenstive);
        }
    }

    public void SetSensitivity(float sensitivity)
    {
        // �÷��̾� ��Ʈ�ѷ� �ʱ�ȭ
        if (localPlayer == null)
        {
            var playerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (playerObj != null)
            {
                localPlayer = playerObj.GetComponent<Player>();
            }
        }

        // localPlayer�� �غ���� �ʾҴٸ� �׳� return
        if (localPlayer == null)
        {
            Debug.Log("LocalPlayer ���� �����ȵ�...");
            return;
        }

        localPlayer.SetMouseSensitivity(sensitivity);
    }
    
    public void senestiveInit()
    {    
        // �÷��̾� ��Ʈ�ѷ� �ʱ�ȭ
        if (localPlayer == null)
        {
            var playerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (playerObj != null)
            {
                localPlayer = playerObj.GetComponent<Player>();
            }
        }

        // localPlayer�� �غ���� �ʾҴٸ� �׳� return
        if (localPlayer == null)
		{
            Debug.Log("LocalPlayer ���� �����ȵ�...");
            return;
        }

        localPlayer.SetMouseSensitivity(mouseSenstive);
    }

    // �����̴� ���� ����Ǹ� InputField ������Ʈ
    private void UpdateInputField(float sensitivity)
    {
        sensitivityInput.text = sensitivity.ToString("0.0"); // �Ҽ��� �� �ڸ����� ǥ��

        // �÷��̾� ��Ʈ�ѷ� �ʱ�ȭ
        if (localPlayer == null)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogWarning("[UpdateInputField] NetworkManager.Singleton�� ���� �����ϴ�.");
                return;
            }

            if (NetworkManager.Singleton.LocalClient == null)
            {
                Debug.LogWarning("[UpdateInputField] LocalClient�� ���� �������� �ʾҽ��ϴ�.");
                return;
            }

            if (NetworkManager.Singleton.LocalClient.PlayerObject == null)
            {
                Debug.LogWarning("[UpdateInputField] PlayerObject�� ���� �������� �ʾҽ��ϴ�.");
                return;
            }

            var playerObj = NetworkManager.Singleton.LocalClient?.PlayerObject; 
            if (playerObj == null)
            {
                Debug.Log("LocalPlayer ���� ����, ���߿� ���� ����");
                return;
            }
            localPlayer = playerObj.GetComponent<Player>();
        }
        Debug.Log("�׽�Ʈ -00000000000000");
        localPlayer.SetMouseSensitivity(sensitivity);
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
        public float mouseSensitivity;       // ���콺 ���� �߰�
    }

    #endregion
}
