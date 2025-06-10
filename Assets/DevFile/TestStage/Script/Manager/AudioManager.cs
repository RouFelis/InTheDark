using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using TMPro;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; set; } // �̱��� �ν��Ͻ�

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioUI masterUI;
    [SerializeField] private AudioUI musicUI;
    [SerializeField] private AudioUI sfxUI;
    [SerializeField] private AudioUI uiUI;

    private const float MinVolumeDb = -80f; // AudioMixer���� �������� �����Ǵ� dB ��
    private const float MaxVolumeDb = 0f;  // AudioMixer���� �ִ� �������� �����Ǵ� dB ��
    private const float minSensitivity = 0f;  // ���� �ּҰ�
    private const float maxSensitivity = 1f;  // ���� �ִ밪
    private const string FilePath = "AudioSettings.json"; // ���� ���� �̸�

    [SerializeField] private AudioSource buttonAudioSource;


    [Header("����� Ŭ��")]
    public AudioClip fallDamged;

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
        KeySettingsManager.Instance.applyButton.onClick.AddListener(SaveAudioSettings); // ���� ��ư�� ������ �߰�

        // �ʱ� �����̴� ���� AudioMixer���� ��������
        masterUI.slider.value = DbToLinear(GetMixerVolume("MasterVolume"));
        musicUI.slider.value = DbToLinear(GetMixerVolume("MusicVolume"));
        sfxUI.slider.value = DbToLinear(GetMixerVolume("SFXVolume"));
        uiUI.slider.value = DbToLinear(GetMixerVolume("UIVolume"));

        // �����̴� �̺�Ʈ ����
        masterUI.slider.onValueChanged.AddListener(value => SetVolume(masterUI));
        musicUI.slider.onValueChanged.AddListener(value => SetVolume(musicUI));
        sfxUI.slider.onValueChanged.AddListener(value => SetVolume(sfxUI));
        uiUI.slider.onValueChanged.AddListener(value => SetVolume(uiUI));

        //������Ʈ �̺�Ʈ ����....
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
      
    public void SetVolume(AudioUI audioUI)
    {
        audioMixer.SetFloat(audioUI.name, LinearToDb(audioUI.slider.value));
    }

    // JSON ����
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

    // JSON �ҷ�����
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

            // �ҷ��� ������ AudioMixer ������Ʈ
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
            // JSON ������ ���� ��� �⺻�� 1�� ����
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

    // ���� ��� �������� (�÷��� ������ ���)
    private string GetFilePath()
    {
        Debug.Log(Application.persistentDataPath);
        return Path.Combine(Application.persistentDataPath, FilePath);
    }


    // �����̴� ���� ����Ǹ� InputField ������Ʈ
    public void UpdateInputField(AudioUI audioUI, float value)
    {
        audioUI.inputField.text = value.ToString("0.0"); // �Ҽ��� �� �ڸ����� ǥ��
    }

    // InputField ���� ����Ǹ� �����̴� �� ������Ʈ
    public void UpdateSliderFromInput(AudioUI audioUI, string input)
    {
        if (float.TryParse(input, out float value))
        {
            // �ּ�/�ִ� �� ���� ����
            value = Mathf.Clamp(value, minSensitivity, maxSensitivity);
            audioUI.slider.value = value; // �����̴� �� ����
        }
        else
        {
            // ���ڰ� �ƴ� ��� �⺻ ������ ����
            audioUI.inputField.text = audioUI.slider.value.ToString("0");
        }
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