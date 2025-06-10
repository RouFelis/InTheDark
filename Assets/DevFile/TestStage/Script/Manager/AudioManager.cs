using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using TMPro;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; set; } // 싱글톤 인스턴스

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioUI masterUI;
    [SerializeField] private AudioUI musicUI;
    [SerializeField] private AudioUI sfxUI;
    [SerializeField] private AudioUI uiUI;

    private const float MinVolumeDb = -80f; // AudioMixer에서 무음으로 설정되는 dB 값
    private const float MaxVolumeDb = 0f;  // AudioMixer에서 최대 볼륨으로 설정되는 dB 값
    private const float minSensitivity = 0f;  // 볼륨 최소값
    private const float maxSensitivity = 1f;  // 볼륨 최대값
    private const string FilePath = "AudioSettings.json"; // 설정 파일 이름

    [SerializeField] private AudioSource buttonAudioSource;


    [Header("오디오 클립")]
    public AudioClip fallDamged;

    private void Awake()
    {
        // 싱글톤 패턴 적용
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
        KeySettingsManager.Instance.applyButton.onClick.AddListener(SaveAudioSettings); // 적용 버튼에 리스너 추가

        // 초기 슬라이더 값을 AudioMixer에서 가져오기
        masterUI.slider.value = DbToLinear(GetMixerVolume("MasterVolume"));
        musicUI.slider.value = DbToLinear(GetMixerVolume("MusicVolume"));
        sfxUI.slider.value = DbToLinear(GetMixerVolume("SFXVolume"));
        uiUI.slider.value = DbToLinear(GetMixerVolume("UIVolume"));

        // 슬라이더 이벤트 연결
        masterUI.slider.onValueChanged.AddListener(value => SetVolume(masterUI));
        musicUI.slider.onValueChanged.AddListener(value => SetVolume(musicUI));
        sfxUI.slider.onValueChanged.AddListener(value => SetVolume(sfxUI));
        uiUI.slider.onValueChanged.AddListener(value => SetVolume(uiUI));

        //업데이트 이벤트 연결....
        masterUI.slider.onValueChanged.AddListener(value => UpdateInputField(masterUI, value));
        musicUI.slider.onValueChanged.AddListener(value => UpdateInputField(musicUI, value));
        sfxUI.slider.onValueChanged.AddListener(value => UpdateInputField(sfxUI, value));
        uiUI.slider.onValueChanged.AddListener(value => UpdateInputField(uiUI, value));

        masterUI.inputField.onEndEdit.AddListener(value => UpdateSliderFromInput(uiUI, value));
        musicUI.inputField.onEndEdit.AddListener(value => UpdateSliderFromInput(uiUI, value));
        sfxUI.inputField.onEndEdit.AddListener(value => UpdateSliderFromInput(uiUI, value));
        uiUI.inputField.onEndEdit.AddListener(value => UpdateSliderFromInput(uiUI, value));


        LoadAudioSettings();
    }

    public void SetbuttonSorce(AudioSource source)
	{
        buttonAudioSource = source;
    }

	#region 볼륨 설정 관련
	private float GetMixerVolume(string parameter)
    {
        float value;
        audioMixer.GetFloat(parameter, out value);
        return value;
    }

    private float DbToLinear(float dB)
    {
        return dB <= MinVolumeDb ? 0f : Mathf.Pow(10f, dB / 20f);
    }

    private float LinearToDb(float linear)
    {
        return linear <= 0f ? MinVolumeDb : Mathf.Log10(linear) * 20f;
    }
      
    public void SetVolume(AudioUI audioUI)
    {
        audioMixer.SetFloat(audioUI.name, LinearToDb(audioUI.slider.value));
    }

    // JSON 저장
    public void SaveAudioSettings()
    {
        AudioSettingsData settings = new AudioSettingsData
        {
            masterVolume = masterUI.slider.value,
            musicVolume = musicUI.slider.value,
            sfxVolume = sfxUI.slider.value,
            uiVolume = uiUI.slider.value
        };

        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(GetFilePath(), json);
        Debug.Log($"Audio settings saved to {GetFilePath()}");
    }

    // JSON 불러오기
    public void LoadAudioSettings()
    {
        if (File.Exists(GetFilePath()))
        {
            string json = File.ReadAllText(GetFilePath());
            AudioSettingsData settings = JsonUtility.FromJson<AudioSettingsData>(json);

            masterUI.slider.value = settings?.masterVolume ?? 1.0f;
            musicUI.slider.value = settings?.musicVolume ?? 1.0f;
            sfxUI.slider.value = settings?.sfxVolume ?? 1.0f;
            uiUI.slider.value = settings?.uiVolume ?? 1.0f;

            // 불러온 값으로 AudioMixer 업데이트
            SetVolume(masterUI);
            SetVolume(musicUI);
            SetVolume(sfxUI);
            SetVolume(uiUI);

            UpdateInputField(masterUI, masterUI.slider.value);
            UpdateInputField(musicUI, musicUI.slider.value);
            UpdateInputField(sfxUI, sfxUI.slider.value);
            UpdateInputField(uiUI, uiUI.slider.value);

            UpdateSliderFromInput(uiUI, masterUI.slider.value.ToString());
            UpdateSliderFromInput(uiUI, musicUI.slider.value.ToString());
            UpdateSliderFromInput(uiUI, sfxUI.slider.value.ToString());
            UpdateSliderFromInput(uiUI, uiUI.slider.value.ToString());

            Debug.Log($"Audio settings loaded from {GetFilePath()}");
        }
        else
        {
            // JSON 파일이 없을 경우 기본값 1로 설정
            masterUI.slider.value = 1.0f;
            musicUI.slider.value = 1.0f;
            sfxUI.slider.value = 1.0f;
            uiUI.slider.value = 1.0f;

            SetVolume(masterUI);
            SetVolume(musicUI);
            SetVolume(sfxUI);
            SetVolume(uiUI);

            Debug.Log("No audio settings file found. Using default values.");
        }
    }

    // 파일 경로 가져오기 (플랫폼 독립적 경로)
    private string GetFilePath()
    {
        Debug.Log(Application.persistentDataPath);
        return Path.Combine(Application.persistentDataPath, FilePath);
    }


    // 슬라이더 값이 변경되면 InputField 업데이트
    public void UpdateInputField(AudioUI audioUI, float value)
    {
        audioUI.inputField.text = value.ToString("0.0"); // 소수점 한 자리까지 표시
    }

    // InputField 값이 변경되면 슬라이더 값 업데이트
    public void UpdateSliderFromInput(AudioUI audioUI, string input)
    {
        if (float.TryParse(input, out float value))
        {
            // 최소/최대 값 범위 제한
            value = Mathf.Clamp(value, minSensitivity, maxSensitivity);
            audioUI.slider.value = value; // 슬라이더 값 변경
        }
        else
        {
            // 숫자가 아닐 경우 기본 값으로 복원
            audioUI.inputField.text = audioUI.slider.value.ToString("0");
        }
    }
	#endregion

	#region 사운드 플레이 관련

	public void PlaySound(AudioClip clip)
    {
        if (clip != null && buttonAudioSource != null)
        {
            buttonAudioSource.PlayOneShot(clip);
        }
    }




    #endregion

}



[System.Serializable]
public struct AudioUI
{
    public Slider slider;
    public TMP_InputField inputField;
    public string name;
}

[System.Serializable]
public class AudioSettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float uiVolume;
}