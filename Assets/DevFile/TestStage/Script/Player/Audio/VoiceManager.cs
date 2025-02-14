using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Unity.Netcode;
using Dissonance.Audio.Playback;

public class VoiceManager : NetworkBehaviour
{
    public AudioMixerGroup defaultMixerGroup; // 기본 Mixer
/*    public AudioMixerGroup indoorMixerGroup;  // 실내 효과
    public AudioMixerGroup outdoorMixerGroup; // 야외 효과
    public AudioMixerGroup hallMixerGroup;    // 홀 효과*/

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
        if (IsServer) // 서버에서만 관리
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
        }
    }*/

    private void OnPlayerConnected(ulong clientId)
    {
        Debug.Log($"플레이어 {clientId}가 접속했습니다.");
        UpdateVoicePlaybackList();
    }

    private void OnPlayerDisconnected(ulong clientId)
    {
        Debug.Log($"플레이어 {clientId}가 접속 해제되었습니다.");
        UpdateVoicePlaybackList();
    }


    public void UpdateVoicePlaybackList()
    {        
        allVoiceSources.Clear();
        VoicePlayback[] voicePlaybacks = FindObjectsByType<VoicePlayback>(FindObjectsSortMode.None); // 모든 VoicePlayback 찾기

        foreach (var playback in voicePlaybacks)
        {
            AudioSource source = playback.GetComponent<AudioSource>();
            if (source != null)
            {
                allVoiceSources.Add(source);
                source.outputAudioMixerGroup = defaultMixerGroup; // 기본 설정
            }
        }

        Debug.Log($"VoiceManager: {allVoiceSources.Count}개의 VoicePlayback을 업데이트함.");
    }

    public void SetAudioZone(AudioMixerGroup zoneMixerGroup)
    {
        foreach (var source in allVoiceSources)
        {
            source.outputAudioMixerGroup = zoneMixerGroup;
        }

        Debug.Log($"모든 VoicePlayback을 {zoneMixerGroup.name}로 변경 완료!");
    }
}
