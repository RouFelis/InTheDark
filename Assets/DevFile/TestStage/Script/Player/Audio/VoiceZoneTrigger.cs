using UnityEngine;
using UnityEngine.Audio;

public class VoiceZoneTrigger : MonoBehaviour
{
    public AudioMixerGroup zoneMixerGroup; // 적용할 Audio Mixer Group
    [SerializeField]private VoiceManager manager;


	private void OnTriggerEnter(Collider other)
    {
		if (manager == null)
		{
            manager = FindAnyObjectByType<VoiceManager>();
        }

        if (other.CompareTag("Player"))
        {
            if (manager != null)
            {
                manager.SetAudioZone(zoneMixerGroup);
                Debug.Log($" {other.name}이(가) {zoneMixerGroup.name} 존에 들어옴!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (manager == null)
        {
            manager = FindAnyObjectByType<VoiceManager>();
        }

        if (other.CompareTag("Player"))
        {

            if (manager != null)
            {
                manager.SetAudioZone(manager.defaultMixerGroup); // 기본 효과로 변경
                Debug.Log($" {other.name}이(가) 기본 존으로 복귀!");
            }
        }
    }

}
