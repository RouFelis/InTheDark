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
 //   [SerializeField] private AudioUI micUI; // 마이크 감도 슬라이더
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
			Debug.LogError("VoiceBroadcastTrigger 컴포넌트를 찾을 수 없습니다. 해당 스크립트를 Dissonance가 있는 GameObject에 붙이거나, 직접 할당하세요.");
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
                Debug.Log("마이크 음소거 상태: " + isMuted);
            }
        }
    }

    /// <summary>
    /// 이름으로 micAudio 항목을 찾음
    /// </summary>
    /// <param name="name">찾을 이름</param>
    /// <returns>해당 이름의 micAudio, 없으면 null</returns>
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
            
            // 중복 체크
            if (micUI.Exists(m => m.micUI.name == playerName))
            {
                Debug.LogWarning($"플레이어 {playerName}의 마이크 UI가 이미 존재합니다. 건너뜁니다.");
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

            // 자식에서 컴포넌트들을 찾음
            temp.micUI = new AudioUI
            {
                inputField = uiObject.GetComponentInChildren<TMPro.TMP_InputField>(),
                slider = uiObject.GetComponentInChildren<Slider>(),
                name = player.Name
            };

            // TMP_Text도 자식에서 찾음
            TMPro.TMP_Text nameText = uiObject.GetComponentInChildren<TMPro.TMP_Text>();
            if (nameText != null)
            {
                nameText.text = player.Name;
            }

            temp.audio = audio;
            temp.micUI.slider.onValueChanged.AddListener(value => { setMicVolume(temp, value); Debug.Log("테스트 1"); });
            temp.micUI.slider.onValueChanged.AddListener(value => { Debug.Log("테스트5"); AudioManager.Instance.UpdateInputField(temp.micUI, value); Debug.Log("테스트2"); });
            temp.micUI.inputField.onEndEdit.AddListener(value => { Debug.Log("테스트4"); AudioManager.Instance.UpdateSliderFromInput(temp.micUI, value); Debug.Log("테스트3"); });
            temp.micUI.slider.value = 1.0f;


            Debug.Log($"테스트 Add MicUI : {nameText.text}");
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

        // 중복 체크
        if (micUI.Exists(m => m.micUI.name == playerName))
        {
            Debug.LogWarning($"플레이어 {playerName}의 마이크 UI가 이미 존재합니다. 건너뜁니다.");
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

        // 자식에서 컴포넌트들을 찾음
        temp.micUI = new AudioUI
        {
            inputField = uiObject.GetComponentInChildren<TMPro.TMP_InputField>(),
            slider = uiObject.GetComponentInChildren<Slider>(),
            name = ConnectedPlayer.Name
        };

        // TMP_Text도 자식에서 찾음
        TMPro.TMP_Text nameText = uiObject.GetComponentInChildren<TMPro.TMP_Text>();
        if (nameText != null)
        {
            nameText.text = ConnectedPlayer.Name;
        }

        temp.audio = audio;
        temp.micUI.slider.onValueChanged.AddListener(value => { setMicVolume(temp, value); Debug.Log("테스트 1"); });
        temp.micUI.slider.onValueChanged.AddListener(value => { Debug.Log("테스트5"); AudioManager.Instance.UpdateInputField(temp.micUI, value); Debug.Log("테스트2"); });
        temp.micUI.inputField.onEndEdit.AddListener(value => { Debug.Log("테스트4"); AudioManager.Instance.UpdateSliderFromInput(temp.micUI, value); Debug.Log("테스트3"); });
        temp.micUI.slider.value = 1.0f;


        Debug.Log($"테스트 Add MicUI : {nameText.text}");
        micUI.Add(temp);
    }

    private IEnumerator RemoveMicUI(ulong cliendtID)
    {
        yield return new WaitForSeconds(0.2f);

        for (int i = micUI.Count - 1; i >= 0; i--)
        {
            if (micUI[i].micUI.name == name)
            {
                // UI 오브젝트 제거 (슬라이더나 inputField가 포함된 부모를 파괴)
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
