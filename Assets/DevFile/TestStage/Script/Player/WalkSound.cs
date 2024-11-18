using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WalkSound : MonoBehaviour
{
    public AudioSource footstepSource; // �߼Ҹ��� ����� AudioSource
    public FootstepSoundData footstepSoundData; // �ٴں� �߼Ҹ� ������


    public int pos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StepSound()
	{
        AudioClip clipToPlay = GetFootstepSound();
        if (clipToPlay != null && footstepSource != null)
        {
            footstepSource.PlayOneShot(clipToPlay);
        }
    }

    // ���� �ٴ��� �����ϰ� �ش� �ٴڿ� �´� �߼Ҹ��� �������� �Լ�
    private AudioClip GetFootstepSound()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            string surfaceTag = hit.collider.tag;

            // �ٴ� �±׿� ���� ������ �Ҹ��� ��ȯ
            switch (surfaceTag)
            {
                case "IronFloor":
                    Debug.Log("Iron Playing");
                    return footstepSoundData.ironFootstep;
                default:
                    return null; // �±װ� ������ �Ҹ� ����
            }
        }
        return null; // �ٴ��� �������� ���� ���
    }
}
