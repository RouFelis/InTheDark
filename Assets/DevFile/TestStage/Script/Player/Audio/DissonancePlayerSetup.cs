using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Dissonance;
using Dissonance.Audio.Playback;
using DilmerGames.Core.Singletons;

public class DissonancePlayerSetup : MonoBehaviour
{
    private DissonanceComms comms;
    private List<VoicePlayback> playback = new List<VoicePlayback>();

	private void Start()
    {
        comms = FindAnyObjectByType<DissonanceComms>();

        SetDissonanceName();
        //StartCoroutine(SetDissonanceNameWhenReady());
    }

    private void SetDissonanceName()
	{
        //comms.LocalPlayerName = FindAnyObjectByType<PlayerIDManager>().PlayerName;
        comms.enabled = true;
    }


}
