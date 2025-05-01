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
        public KeyName keyName; // 키 이름
        public TMP_Text keyText; // 키를 표시할 텍스트
        public Button keyButton; // 키 설정 버튼
    }

    private KeySettingField activeKeySettingField = null;
    public List<KeySettingField> keySettingFields; // 인스펙터에서 키 이름과 인풋 필드를 설정
    public Button applyButton; // 적용 버튼
    public Button cancelButton; // 취소 버튼
    public GameObject keySettingsPanel; // 키 설정 패널


    private Dictionary<string, KeyCode> keySettings = new Dictionary<string, KeyCode>(); // 키 설정을 저장하는 딕셔너리
    private string settingsFilePath; // 키 설정 파일 경로
    public TMP_Dropdown languageDropdown; // TMP 드롭다운 사용

    // 델리게이트와 이벤트 정의 (키값이 바뀌면...)
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
    [SerializeField] private TMP_InputField sensitivityInput; // 감도 입력 필드 (TMP_InputField 사용)
    [SerializeField] public Slider sensitivitySlider; // 슬라이더 연결
    public Player localPlayer;

    private bool isPaused = false;

    #region 이게 제일 빠를거같고 키도 몇개안되서 이래 함. 맘에 안들면 이거지우고 그냥 딕셔너리값 불러오게하면됨. 위에있음 ㅇㅇ
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
    public KeyCode SprintKey
    {
        get { return sprintKey; }
        set
        {
            if (sprintKey != value)
            {
                sprintKey = value;
                KeyCodeChanged?.Invoke();  // 값 변경 시 이벤트 호출
            }
        }
    }
    #endregion

    private void Start()
    {
        Instance = this;
        // 설정 파일 경로를 지정
        settingsFilePath = Path.Combine(Application.persistentDataPath, "keysettings.json");

        LoadKeySettings(); // 키 설정 로드
        InitializeKeySettingUI(); // 키 설정 UI 초기화

        applyButton.onClick.AddListener(ApplyKeySettings); // 적용 버튼에 리스너 추가
        cancelButton.onClick.AddListener(CancelKeySettings); // 취소 버튼에 리스너 추가

        sensitivitySlider.onValueChanged.AddListener(UpdateInputField); //슬라이더 이벤트 추가
        sensitivityInput.onEndEdit.AddListener(UpdateSliderFromInput); //슬라이더 이벤트 추가

        keySettingsPanel.SetActive(false); // 초기 상태는 비활성화
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
        Debug.Log("KeySetting2 를 찾았습니다.");
    }

    private void ApplyKeySettings()
    {
        // 각 키 설정 필드를 순회하며 설정 적용
        foreach (KeySettingField field in keySettingFields)
        {
            if (TryGetKeyCode(field.keyText.ToString(), out KeyCode newKey))
            {
                keySettings[field.keyName.ToString()] = newKey;
            }
            else
            {
                Debug.LogError("Invalid key entered for " + field.keyName.ToString()); // 잘못된 키 입력 시 오류 출력
            }
        }

        MenuManager.Instance.CloseCurrentMenu();
        SaveKeySettings(); // 키 설정 저장
        KeyCodeChanged?.Invoke();  // 값 변경 시 이벤트 호출
        SetKey(); // 키 변경 적용
    }

    private void CancelKeySettings()
    {
        MenuManager.Instance.CloseCurrentMenu();
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
                // 버튼에 표시할 텍스트 설정
                field.keyText.text = keySettings[field.keyName.ToString()].ToString();

                // 버튼 클릭 이벤트 설정
                field.keyButton.onClick.AddListener(() => OnKeyButtonClicked(field));
            }
        }
    }

    private void OnKeyButtonClicked(KeySettingField field)
    {
        activeKeySettingField = field; // 현재 활성화된 필드 설정
        Debug.Log("Press a key to set for: " + field.keyName);
    }
    private void UpdateKeySetting(KeySettingField field, KeyCode newKey)
    {
        string keyName = field.keyName.ToString();

        if (keySettings.ContainsKey(keyName))
        {
            keySettings[keyName] = newKey; // 키 설정 업데이트
            field.keyText.text = newKey.ToString(); // 텍스트 업데이트
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

	#region MouseSenestive
	public void senestiveInit()
	{
        // 슬라이더 초기화
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = mouseSenstive;
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }

        // 플레이어 컨트롤러 초기화
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

    // 슬라이더 값이 변경되면 InputField 업데이트
    private void UpdateInputField(float value)
    {
        sensitivityInput.text = value.ToString("0.0"); // 소수점 한 자리까지 표시
        if (localPlayer != null)
        {
            localPlayer.SetMouseSensitivity(value);
        }
    }

    // InputField 값이 변경되면 슬라이더 값 업데이트
    private void UpdateSliderFromInput(string input)
    {
        if (float.TryParse(input, out float value))
        {
            // 최소/최대 값 범위 제한
            value = Mathf.Clamp(value, minSensitivity, maxSensitivity);
            sensitivitySlider.value = value; // 슬라이더 값 변경
        }
        else
        {
            // 숫자가 아닐 경우 기본 값으로 복원
            sensitivityInput.text = sensitivitySlider.value.ToString("0");
        }
    }

    #endregion
    #region 키값
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

	#endregion
}
