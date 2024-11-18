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
        public KeyName keyName; // 키 이름
        public TMP_InputField inputField; // 인풋 필드
    }

    public List<KeySettingField> keySettingFields; // 인스펙터에서 키 이름과 인풋 필드를 설정
    public Button applyButton; // 적용 버튼
    public Button cancelButton; // 취소 버튼
    public GameObject keySettingsPanel; // 키 설정 패널

    private Dictionary<string, KeyCode> keySettings = new Dictionary<string, KeyCode>(); // 키 설정을 저장하는 딕셔너리
    private string settingsFilePath; // 키 설정 파일 경로
    private TMP_InputField activeInputField = null; // 현재 활성화된 인풋 필드

    // 델리게이트와 이벤트 정의
    public delegate void OnKeyCodeChanged();
    public event OnKeyCodeChanged KeyCodeChanged;

    [SerializeField]private KeyCode interactKey;
    [SerializeField]private KeyCode dropKey;
    [SerializeField]private KeyCode useItemKey;
    [SerializeField]private KeyCode scanKey;
    [SerializeField]private KeyCode lightKey;
    public TMP_Dropdown languageDropdown; // TMP 드롭다운 사용

    public KeyCode InteractKey {         
        get { return interactKey; }
        set
        {
            if (interactKey != value)
            {
                interactKey = value;
                KeyCodeChanged?.Invoke();  // 값 변경 시 이벤트 호출
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
                KeyCodeChanged?.Invoke();  // 값 변경 시 이벤트 호출
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
                KeyCodeChanged?.Invoke();  // 값 변경 시 이벤트 호출
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
                KeyCodeChanged?.Invoke();  // 값 변경 시 이벤트 호출
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
                KeyCodeChanged?.Invoke();  // 값 변경 시 이벤트 호출
            }
        }
    }



    private void Start()
    {
        Instance = this;
        // 설정 파일 경로를 지정
        settingsFilePath = Path.Combine(Application.persistentDataPath, "keysettings.json");

        LoadKeySettings(); // 키 설정 로드
        InitializeKeySettingUI(); // 키 설정 UI 초기화

        applyButton.onClick.AddListener(ApplyKeySettings); // 적용 버튼에 리스너 추가
        cancelButton.onClick.AddListener(CancelKeySettings); // 취소 버튼에 리스너 추가

        keySettingsPanel.SetActive(false); // 초기 상태는 비활성화
        SetLanguage();
        SetKey();
    }

    private void Update()
    {
        // Esc 키를 눌러 키 설정 UI를 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            keySettingsPanel.SetActive(!keySettingsPanel.activeSelf);
            MouseFixed(!keySettingsPanel.activeSelf);
        }

        // 인풋 필드가 활성화되었을 때 키 입력을 처리
        if (activeInputField != null)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    activeInputField.text = key.ToString();
                    activeInputField = null; // 입력이 완료되면 인풋 필드를 비활성화
                    EnableAllInputFields(); // 다른 인풋 필드 활성화
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
        Debug.Log("KeySetting2 를 찾았습니다.");
    }

    private void MouseFixed(bool isFix)
	{
        if (isFix)
        {
            Cursor.lockState = CursorLockMode.Locked; // 커서를 중앙에 고정
        }
        else
        {
            Cursor.lockState = CursorLockMode.None; // 커서 고정 해제
        }
    }

    private void ApplyKeySettings()
    {
        // 각 키 설정 필드를 순회하며 설정 적용
        foreach (KeySettingField field in keySettingFields)
        {
            if (TryGetKeyCode(field.inputField.text, out KeyCode newKey))
            {
                keySettings[field.keyName.ToString()] = newKey;
            }
            else
            {
                Debug.LogError("Invalid key entered for " + field.keyName.ToString()); // 잘못된 키 입력 시 오류 출력
            }
        }

        MouseFixed(true);
        SaveKeySettings(); // 키 설정 저장
        SetKey(); // 키 변경 적용
        keySettingsPanel.SetActive(false); // 키 설정 패널 비활성화
    }

    private void CancelKeySettings()
    {
        MouseFixed(true);
        keySettingsPanel.SetActive(false); // 키 설정 패널 비활성화
    }

    public KeyCode GetKey(string name)
    {
        return keySettings.ContainsKey(name) ? keySettings[name] : KeyCode.None; // 키 설정 반환
    }

    private void LoadKeySettings()
    {
        if (File.Exists(settingsFilePath))
        {
            string json = File.ReadAllText(settingsFilePath); // 파일에서 JSON 읽기
            KeySettingList keySettingList = JsonUtility.FromJson<KeySettingList>(json); // JSON을 객체로 변환
            keySettings = new Dictionary<string, KeyCode>(); // 딕셔너리 초기화
            foreach (var keySetting in keySettingList.keySettings)
            {
                keySettings[keySetting.name] = keySetting.key; // 키 설정 로드
            }
        }
        else
        {
            // 기본 키 설정 추가
            keySettings["Interact"] = KeyCode.F;
            keySettings["Drop"] = KeyCode.Q;
            keySettings["UseItem"] = KeyCode.E;
            keySettings["ScanKey"] = KeyCode.Tab;
            keySettings["Light"] = KeyCode.R;
        }
    }

    private void SaveKeySettings()
    {
        // 키 설정 리스트 객체 생성
        KeySettingList keySettingList = new KeySettingList
        {
            keySettings = new List<KeySetting>()
        };

        // 딕셔너리의 키 설정을 리스트에 추가
        foreach (var kvp in keySettings)
        {
            keySettingList.keySettings.Add(new KeySetting
            {
                name = kvp.Key,
                key = kvp.Value
            });
        }

        // JSON으로 변환하여 파일에 쓰기
        string json = JsonUtility.ToJson(keySettingList, true);
        File.WriteAllText(settingsFilePath, json);
    }

    private void InitializeKeySettingUI()
    {
        // 각 키 설정 필드를 초기화
        foreach (KeySettingField field in keySettingFields)
        {
            if (keySettings.ContainsKey(field.keyName.ToString()))
            {
                field.inputField.text = keySettings[field.keyName.ToString()].ToString(); // 인풋 필드에 키 설정 값 표시
                field.inputField.onSelect.AddListener(delegate { OnInputFieldSelected(field.inputField); }); // 인풋 필드 선택 시 이벤트 리스너 추가
            }
        }
    }

    private void OnInputFieldSelected(TMP_InputField selectedInputField)
    {
        // 다른 인풋 필드 비활성화
        foreach (KeySettingField field in keySettingFields)
        {
            if (field.inputField != selectedInputField)
            {
                field.inputField.interactable = false;
            }
        }
        activeInputField = selectedInputField; // 현재 활성화된 인풋 필드 설정
    }

    private void EnableAllInputFields()
    {
        // 모든 인풋 필드를 활성화
        foreach (KeySettingField field in keySettingFields)
        {
            field.inputField.interactable = true;
        }
    }

    private bool TryGetKeyCode(string keyString, out KeyCode keyCode)
    {
        keyCode = KeyCode.None;
        // 문자열을 KeyCode로 변환 시도
        if (System.Enum.TryParse(keyString, true, out KeyCode parsedKeyCode))
        {
            keyCode = parsedKeyCode;
            return true;
        }
        return false;
    }

    private void SetLanguage()
	{
        // TMP 드롭다운 초기화 (설정된 로케일 목록을 드롭다운에 추가)
        languageDropdown.options.Clear();
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(locale.Identifier.CultureInfo.NativeName));
        }

        // 현재 언어에 맞춰 드롭다운 선택 초기화
        languageDropdown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        languageDropdown.onValueChanged.AddListener(ChangeLanguage);

    }

    public void ChangeLanguage(int index)
    {
        // 선택된 인덱스에 따라 언어 변경
        Locale selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        StartCoroutine(SetLocale(selectedLocale));
    }

    IEnumerator SetLocale(Locale locale)
    {
        yield return LocalizationSettings.InitializationOperation; // 로케일 초기화 대기
        LocalizationSettings.SelectedLocale = locale; // 선택한 로케일로 변경
    }

    [System.Serializable]
    private class KeySetting
    {
        public string name; // 키 이름
        public KeyCode key; // 키 값
    }

    [System.Serializable]
    private class KeySettingList
    {
        public List<KeySetting> keySettings; // 키 설정 리스트
    }
}
