using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Unity.Netcode;
using Dissonance.Audio.Playback;

public class VoiceManager : NetworkBehaviour
{
    public AudioMixerGroup defaultMixerGroup; // �⺻ Mixer
/*    public AudioMixerGroup indoorMixerGroup;  // �ǳ� ȿ��
    public AudioMixerGroup outdoorMixerGroup; // �߿� ȿ��
    public AudioMixerGroup hallMixerGroup;    // Ȧ ȿ��*/

   [SerializeField] private List<AudioSource> allVoiceSources = new List<AudioSource>();

    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
        }
        UpdateVoicePlaybackList();
    }

/*    public override void OnNetworkSpawn()
    {
        if (IsServer) // ���������� ����
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
        }
    }*/

    private void OnPlayerConnected(ulong clientId)
    {
        Debug.Log($"�÷��̾� {clientId}�� �����߽��ϴ�.");
        UpdateVoicePlaybackList();
    }

    private void OnPlayerDisconnected(ulong clientId)
    {
        Debug.Log($"�÷��̾� {clientId}�� ���� �����Ǿ����ϴ�.");
        UpdateVoicePlaybackList();
    }


    public void UpdateVoicePlaybackList()
    {        
        allVoiceSources.Clear();
        VoicePlayback[] voicePlaybacks = FindObjectsByType<VoicePlayback>(FindObjectsSortMode.None); // ��� VoicePlayback ã��

        foreach (var playback in voicePlaybacks)
        {
            AudioSource source = playback.GetComponent<AudioSource>();
            if (source != null)
            {
                allVoiceSources.Add(source);
                source.outputAudioMixerGroup = defaultMixerGroup; // �⺻ ����
            }
        }

        Debug.Log($"VoiceManager: {allVoiceSources.Count}���� VoicePlayback�� ������Ʈ��.");
    }

    public void SetAudioZone(AudioMixerGroup zoneMixerGroup)
    {
        foreach (var source in allVoiceSources)
        {
            source.outputAudioMixerGroup = zoneMixerGroup;
        }

        Debug.Log($"��� VoicePlayback�� {zoneMixerGroup.name}�� ���� �Ϸ�!");
    }
}
