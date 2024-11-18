using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
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
        public TMP_InputField inputField; // ��ǲ �ʵ�
    }

    public List<KeySettingField> keySettingFields; // �ν����Ϳ��� Ű �̸��� ��ǲ �ʵ带 ����
    public Button applyButton; // ���� ��ư
    public Button cancelButton; // ��� ��ư
    public GameObject keySettingsPanel; // Ű ���� �г�

    private Dictionary<string, KeyCode> keySettings = new Dictionary<string, KeyCode>(); // Ű ������ �����ϴ� ��ųʸ�
    private string settingsFilePath; // Ű ���� ���� ���
    private TMP_InputField activeInputField = null; // ���� Ȱ��ȭ�� ��ǲ �ʵ�

    // ��������Ʈ�� �̺�Ʈ ����
    public delegate void OnKeyCodeChanged();
    public event OnKeyCodeChanged KeyCodeChanged;

    [SerializeField]private KeyCode interactKey;
    [SerializeField]private KeyCode dropKey;
    [SerializeField]private KeyCode useItemKey;
    [SerializeField]private KeyCode scanKey;
    [SerializeField]private KeyCode lightKey;
    public TMP_Dropdown languageDropdown; // TMP ��Ӵٿ� ���

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



    private void Start()
    {
        Instance = this;
        // ���� ���� ��θ� ����
        settingsFilePath = Path.Combine(Application.persistentDataPath, "keysettings.json");

        LoadKeySettings(); // Ű ���� �ε�
        InitializeKeySettingUI(); // Ű ���� UI �ʱ�ȭ

        applyButton.onClick.AddListener(ApplyKeySettings); // ���� ��ư�� ������ �߰�
        cancelButton.onClick.AddListener(CancelKeySettings); // ��� ��ư�� ������ �߰�

        keySettingsPanel.SetActive(false); // �ʱ� ���´� ��Ȱ��ȭ
        SetLanguage();
        SetKey();
    }

    private void Update()
    {
        // Esc Ű�� ���� Ű ���� UI�� ���
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            keySettingsPanel.SetActive(!keySettingsPanel.activeSelf);
            MouseFixed(!keySettingsPanel.activeSelf);
        }

        // ��ǲ �ʵ尡 Ȱ��ȭ�Ǿ��� �� Ű �Է��� ó��
        if (activeInputField != null)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    activeInputField.text = key.ToString();
                    activeInputField = null; // �Է��� �Ϸ�Ǹ� ��ǲ �ʵ带 ��Ȱ��ȭ
                    EnableAllInputFields(); // �ٸ� ��ǲ �ʵ� Ȱ��ȭ
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
        Debug.Log("KeySetting2 �� ã�ҽ��ϴ�.");
    }

    private void MouseFixed(bool isFix)
	{
        if (isFix)
        {
            Cursor.lockState = CursorLockMode.Locked; // Ŀ���� �߾ӿ� ����
        }
        else
        {
            Cursor.lockState = CursorLockMode.None; // Ŀ�� ���� ����
        }
    }

    private void ApplyKeySettings()
    {
        // �� Ű ���� �ʵ带 ��ȸ�ϸ� ���� ����
        foreach (KeySettingField field in keySettingFields)
        {
            if (TryGetKeyCode(field.inputField.text, out KeyCode newKey))
            {
                keySettings[field.keyName.ToString()] = newKey;
            }
            else
            {
                Debug.LogError("Invalid key entered for " + field.keyName.ToString()); // �߸��� Ű �Է� �� ���� ���
            }
        }

        MouseFixed(true);
        SaveKeySettings(); // Ű ���� ����
        SetKey(); // Ű ���� ����
        keySettingsPanel.SetActive(false); // Ű ���� �г� ��Ȱ��ȭ
    }

    private void CancelKeySettings()
    {
        MouseFixed(true);
        keySettingsPanel.SetActive(false); // Ű ���� �г� ��Ȱ��ȭ
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
                field.inputField.text = keySettings[field.keyName.ToString()].ToString(); // ��ǲ �ʵ忡 Ű ���� �� ǥ��
                field.inputField.onSelect.AddListener(delegate { OnInputFieldSelected(field.inputField); }); // ��ǲ �ʵ� ���� �� �̺�Ʈ ������ �߰�
            }
        }
    }

    private void OnInputFieldSelected(TMP_InputField selectedInputField)
    {
        // �ٸ� ��ǲ �ʵ� ��Ȱ��ȭ
        foreach (KeySettingField field in keySettingFields)
        {
            if (field.inputField != selectedInputField)
            {
                field.inputField.interactable = false;
            }
        }
        activeInputField = selectedInputField; // ���� Ȱ��ȭ�� ��ǲ �ʵ� ����
    }

    private void EnableAllInputFields()
    {
        // ��� ��ǲ �ʵ带 Ȱ��ȭ
        foreach (KeySettingField field in keySettingFields)
        {
            field.inputField.interactable = true;
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
}
