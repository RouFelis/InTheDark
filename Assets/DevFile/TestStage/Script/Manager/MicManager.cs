using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.Netcode;
using Dissonance;
using Dissonance.Integrations.Unity_NFGO;
using System.Collections.Generic;


[System.Serializable]
public struct micAudio
{
    public AudioUI micUI;
    public AudioSource audio;
}

public class MicManager : MonoBehaviour
{
 //   [SerializeField] private AudioUI micUI; // ����ũ ���� �����̴�
    [SerializeField] private DissonancePlayerSetup playerSetup;
    [SerializeField] private DissonanceComms comms;
    [SerializeField] public List<micAudio> micUI;

    [SerializeField] private Transform parentsTransform;
    [SerializeField] private GameObject micUIPrefab;
    


    private bool isMuted = false;

	void Start()
	{
        comms = FindAnyObjectByType<DissonanceComms>();

		if (comms == null)
		{
			Debug.LogError("VoiceBroadcastTrigger ������Ʈ�� ã�� �� �����ϴ�. �ش� ��ũ��Ʈ�� Dissonance�� �ִ� GameObject�� ���̰ų�, ���� �Ҵ��ϼ���.");
		}

        StartCoroutine(InitMicUI());

        PlayersManager.Instance.OnPlayerAdded += (clientID) => { StartCoroutine(AddMicUI(clientID)); };
        PlayersManager.Instance.OnPlayerRemoved += (clientID) => { StartCoroutine(RemoveMicUI(clientID)); };
    }

	private void OnDestroy()
	{
        PlayersManager.Instance.OnPlayerAdded += (clientID) => { StartCoroutine(AddMicUI(clientID)); };
    }

	void Update()
    {
        if (Input.GetKeyDown(KeySettingsManager.Instance.MicKey))
        {
            isMuted = !isMuted;

            if (comms != null)
            {
                comms.IsMuted = isMuted;
                Debug.Log("����ũ ���Ұ� ����: " + isMuted);
            }
        }
    }

    /// <summary>
    /// �̸����� micAudio �׸��� ã��
    /// </summary>
    /// <param name="name">ã�� �̸�</param>
    /// <returns>�ش� �̸��� micAudio, ������ null</returns>
    public micAudio? GetMicAudioByName(string name)
    {
        foreach (var mic in micUI)
        {
            if (mic.micUI.name == name)
            {
                return mic;
            }
        }
        return null;
    }

    public IEnumerator InitMicUI()
    {
        while(PlayersManager.Instance.playersList.Count == 0)
		{
            yield return new WaitForSeconds(0.1f);
        }

        foreach (var player in PlayersManager.Instance.playersList)
        {
            string playerName = player.Name;
            
            // �ߺ� üũ
            if (micUI.Exists(m => m.micUI.name == playerName))
            {
                Debug.LogWarning($"�÷��̾� {playerName}�� ����ũ UI�� �̹� �����մϴ�. �ǳʶݴϴ�.");
                continue;
            }

            NfgoPlayer temptNfgo = player.GetComponent<NfgoPlayer>();

            AudioSource audio = null;
            float timer = 0f;

            while (audio == null && timer < 2f)
            {
                GameObject voiceObj = GameObject.Find($"Player {temptNfgo.PlayerId} voice comms");
                if (voiceObj != null)
                {
                    audio = voiceObj.GetComponent<AudioSource>();
                }

                if (audio == null)
                {
                    yield return new WaitForSeconds(0.1f);
                    timer += 0.1f;
                }
            }

            if (audio == null)
            {

                Debug.Log($"Can't find player {temptNfgo.PlayerId} voice comms");

                continue;
            }

            Debug.Log($"find player {temptNfgo.PlayerId} voice comms");
            GameObject uiObject = Instantiate(micUIPrefab, parentsTransform);

            micAudio temp = new micAudio();

            // �ڽĿ��� ������Ʈ���� ã��
            temp.micUI = new AudioUI
            {
                inputField = uiObject.GetComponentInChildren<TMPro.TMP_InputField>(),
                slider = uiObject.GetComponentInChildren<Slider>(),
                name = player.Name
            };

            // TMP_Text�� �ڽĿ��� ã��
            TMPro.TMP_Text nameText = uiObject.GetComponentInChildren<TMPro.TMP_Text>();
            if (nameText != null)
            {
                nameText.text = player.Name;
            }

            temp.audio = audio;
            temp.micUI.slider.onValueChanged.AddListener(value => { setMicVolume(temp, value); Debug.Log("�׽�Ʈ 1"); });
            temp.micUI.slider.onValueChanged.AddListener(value => { Debug.Log("�׽�Ʈ5"); AudioManager.Instance.UpdateInputField(temp.micUI, value); Debug.Log("�׽�Ʈ2"); });
            temp.micUI.inputField.onEndEdit.AddListener(value => { Debug.Log("�׽�Ʈ4"); AudioManager.Instance.UpdateSliderFromInput(temp.micUI, value); Debug.Log("�׽�Ʈ3"); });
            temp.micUI.slider.value = 1.0f;


            Debug.Log($"�׽�Ʈ Add MicUI : {nameText.text}");
            micUI.Add(temp);
        }
        Debug.Log("InitMicUI Complete...");
        yield break;
    }

    private void setMicVolume(micAudio micAudio , float value)
	{
        micAudio.audio.volume = value;
    }

    private IEnumerator AddMicUI(ulong clientID)
    {
        yield return new WaitForSeconds(0.2f);

        Player ConnectedPlayer = new Player();

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientID, out var playerClient))
        {
            ConnectedPlayer = playerClient.PlayerObject.GetComponent<Player>();
        }

        string playerName = ConnectedPlayer.Name;

        // �ߺ� üũ
        if (micUI.Exists(m => m.micUI.name == playerName))
        {
            Debug.LogWarning($"�÷��̾� {playerName}�� ����ũ UI�� �̹� �����մϴ�. �ǳʶݴϴ�.");
            yield break;
        }

        NfgoPlayer temptNfgo = ConnectedPlayer.GetComponent<NfgoPlayer>();

        AudioSource audio = null;
        float timer = 0f;

        while (audio == null && timer < 2f)
        {
            GameObject voiceObj = GameObject.Find($"Player {temptNfgo.PlayerId} voice comms");
            if (voiceObj != null)
            {
                audio = voiceObj.GetComponent<AudioSource>();
            }

            if (audio == null)
            {
                yield return new WaitForSeconds(0.1f);
                timer += 0.1f;
            }
        }

        if (audio == null)
        {

            Debug.Log($"Can't find player {temptNfgo.PlayerId} voice comms");

            yield break;
        }

        Debug.Log($"find player {temptNfgo.PlayerId} voice comms");
        GameObject uiObject = Instantiate(micUIPrefab, parentsTransform);

        micAudio temp = new micAudio();

        // �ڽĿ��� ������Ʈ���� ã��
        temp.micUI = new AudioUI
        {
            inputField = uiObject.GetComponentInChildren<TMPro.TMP_InputField>(),
            slider = uiObject.GetComponentInChildren<Slider>(),
            name = ConnectedPlayer.Name
        };

        // TMP_Text�� �ڽĿ��� ã��
        TMPro.TMP_Text nameText = uiObject.GetComponentInChildren<TMPro.TMP_Text>();
        if (nameText != null)
        {
            nameText.text = ConnectedPlayer.Name;
        }

        temp.audio = audio;
        temp.micUI.slider.onValueChanged.AddListener(value => { setMicVolume(temp, value); Debug.Log("�׽�Ʈ 1"); });
        temp.micUI.slider.onValueChanged.AddListener(value => { Debug.Log("�׽�Ʈ5"); AudioManager.Instance.UpdateInputField(temp.micUI, value); Debug.Log("�׽�Ʈ2"); });
        temp.micUI.inputField.onEndEdit.AddListener(value => { Debug.Log("�׽�Ʈ4"); AudioManager.Instance.UpdateSliderFromInput(temp.micUI, value); Debug.Log("�׽�Ʈ3"); });
        temp.micUI.slider.value = 1.0f;


        Debug.Log($"�׽�Ʈ Add MicUI : {nameText.text}");
        micUI.Add(temp);
    }

    private IEnumerator RemoveMicUI(ulong cliendtID)
    {
        yield return new WaitForSeconds(0.2f);

        for (int i = micUI.Count - 1; i >= 0; i--)
        {
            if (micUI[i].micUI.name == name)
            {
                // UI ������Ʈ ���� (�����̴��� inputField�� ���Ե� �θ� �ı�)
                if (micUI[i].micUI.slider != null)
                {
                    Destroy(micUI[i].micUI.slider.transform.root.gameObject);
                }

                micUI.RemoveAt(i);
                yield break;
            }
        }
    }


    private void ChangeRoom()
	{

	}
}
