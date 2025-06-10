using UnityEngine;
using Dissonance;
using Dissonance.Audio.Playback;

public class DissonancePlayerParse : MonoBehaviour
{
    string playerName;
	private DissonanceComms disso;
	private VoicePlayback playback;

	private void Start()
	{
		playback = GetComponent<VoicePlayback>();

	}



}
