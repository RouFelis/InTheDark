using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; set; } // �̱��� �ν��Ͻ�

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Button applyButton; // ���� ��ư
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    private const float MinVolumeDb = -80f; // AudioMixer���� �������� �����Ǵ� dB ��
    private const float MaxVolumeDb = 0f;  // AudioMixer���� �ִ� �������� �����Ǵ� dB ��
    private const string FilePath = "AudioSettings.json"; // ���� ���� �̸�

    [SerializeField] private AudioSource buttonAudioSource;

    private void Awake()
    {
        // �̱��� ���� ����
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
        applyButton.onClick.AddListener(SaveAudioSettings); // ���� ��ư�� ������ �߰�

        // �ʱ� �����̴� ���� AudioMixer���� ��������
        masterSlider.value = DbToLinear(GetMixerVolume("MasterVolume"));
        musicSlider.value = DbToLinear(GetMixerVolume("MusicVolume"));
        sfxSlider.value = DbToLinear(GetMixerVolume("SFXVolume"));
        uiSlider.value = DbToLinear(GetMixerVolume("UIVolume"));

        // �����̴� �̺�Ʈ ����
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        uiSlider.onValueChanged.AddListener(SetUIVolume);

        LoadAudioSettings();
    }

    public void SetbuttonSorce(AudioSource source)
	{
        buttonAudioSource = source;
    }

	#region ���� ���� ����
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

    // Master Volume ����
    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", LinearToDb(value));
    }

    // Music Volume ����
    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", LinearToDb(value));
    }

    // SFX Volume ����
    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", LinearToDb(value));
    }

    // UI Volume ����
    public void SetUIVolume(float value)
    {
        audioMixer.SetFloat("UIVolume", LinearToDb(value));
    }


    // JSON ����
    public void SaveAudioSettings()
    {
        AudioSettingsData settings = new AudioSettingsData
        {
            masterVolume = masterSlider.value,
            musicVolume = musicSlider.value,
            sfxVolume = sfxSlider.value,
            uiVolume = uiSlider.value
        };

        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(GetFilePath(), json);
        Debug.Log($"Audio settings saved to {GetFilePath()}");
    }

    // JSON �ҷ�����
    public void LoadAudioSettings()
    {
        if (File.Exists(GetFilePath()))
        {
            string json = File.ReadAllText(GetFilePath());
            AudioSettingsData settings = JsonUtility.FromJson<AudioSettingsData>(json);

            masterSlider.value = settings?.masterVolume ?? 1.0f;
            musicSlider.value = settings?.musicVolume ?? 1.0f;
            sfxSlider.value = settings?.sfxVolume ?? 1.0f;
            uiSlider.value = settings?.uiVolume ?? 1.0f;

            // �ҷ��� ������ AudioMixer ������Ʈ
            SetMasterVolume(masterSlider.value);
            SetMusicVolume(musicSlider.value);
            SetSFXVolume(sfxSlider.value);
            SetUIVolume(uiSlider.value);

            Debug.Log($"Audio settings loaded from {GetFilePath()}");
        }
        else
        {
            // JSON ������ ���� ��� �⺻�� 1�� ����
            masterSlider.value = 1.0f;
            musicSlider.value = 1.0f;
            sfxSlider.value = 1.0f;
            uiSlider.value = 1.0f;

            SetMasterVolume(masterSlider.value);
            SetMusicVolume(musicSlider.value);
            SetSFXVolume(sfxSlider.value);
            SetUIVolume(uiSlider.value);

            Debug.Log("No audio settings file found. Using default values.");
        }
    }

    // ���� ��� �������� (�÷��� ������ ���)
    private string GetFilePath()
    {
        Debug.Log(Application.persistentDataPath);
        return Path.Combine(Application.persistentDataPath, FilePath);
    }

    #endregion


    #region ���� �÷��� ����

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
    public float uiVolume;
}