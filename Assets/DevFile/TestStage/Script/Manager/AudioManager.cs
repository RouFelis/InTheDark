using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; set; } // 싱글톤 인스턴스

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Button applyButton; // 적용 버튼
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const float MinVolumeDb = -80f; // AudioMixer에서 무음으로 설정되는 dB 값
    private const float MaxVolumeDb = 0f;  // AudioMixer에서 최대 볼륨으로 설정되는 dB 값
    private const string FilePath = "AudioSettings.json"; // 설정 파일 이름

    [SerializeField] private AudioSource buttonAudioSource;

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
        applyButton.onClick.AddListener(SaveAudioSettings); // 적용 버튼에 리스너 추가

        // 초기 슬라이더 값을 AudioMixer에서 가져오기
        masterSlider.value = DbToLinear(GetMixerVolume("MasterVolume"));
        musicSlider.value = DbToLinear(GetMixerVolume("MusicVolume"));
        sfxSlider.value = DbToLinear(GetMixerVolume("SFXVolume"));

        // 슬라이더 이벤트 연결
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

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

    // Master Volume 설정
    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", LinearToDb(value));
    }

    // Music Volume 설정
    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", LinearToDb(value));
    }

    // SFX Volume 설정
    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", LinearToDb(value));
    }


    // JSON 저장
    public void SaveAudioSettings()
    {
        AudioSettingsData settings = new AudioSettingsData
        {
            masterVolume = masterSlider.value,
            musicVolume = musicSlider.value,
            sfxVolume = sfxSlider.value
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

            masterSlider.value = settings.masterVolume;
            musicSlider.value = settings.musicVolume;
            sfxSlider.value = settings.sfxVolume;

            // 불러온 값으로 AudioMixer 업데이트
            SetMasterVolume(masterSlider.value);
            SetMusicVolume(musicSlider.value);
            SetSFXVolume(sfxSlider.value);

            Debug.Log($"Audio settings loaded from {GetFilePath()}");
        }
        else
        {
            Debug.Log("No audio settings file found. Using default values.");
        }
    }

    // 파일 경로 가져오기 (플랫폼 독립적 경로)
    private string GetFilePath()
    {
        Debug.Log(Application.persistentDataPath);
        return Path.Combine(Application.persistentDataPath, FilePath);
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
public class AudioSettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
}