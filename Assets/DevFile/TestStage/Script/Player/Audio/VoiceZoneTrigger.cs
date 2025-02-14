using UnityEngine;
using UnityEngine.Audio;

public class VoiceZoneTrigger : MonoBehaviour
{
    public AudioMixerGroup zoneMixerGroup; // ������ Audio Mixer Group
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
                Debug.Log($" {other.name}��(��) {zoneMixerGroup.name} ���� ����!");
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
                manager.SetAudioZone(manager.defaultMixerGroup); // �⺻ ȿ���� ����
                Debug.Log($" {other.name}��(��) �⺻ ������ ����!");
            }
        }
    }

}
