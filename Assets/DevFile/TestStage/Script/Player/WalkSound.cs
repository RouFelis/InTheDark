using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WalkSound : MonoBehaviour
{
    public AudioSource footstepSource; // 발소리를 재생할 AudioSource
    public FootstepSoundData footstepSoundData; // 바닥별 발소리 데이터


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

    // 현재 바닥을 감지하고 해당 바닥에 맞는 발소리를 가져오는 함수
    private AudioClip GetFootstepSound()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            string surfaceTag = hit.collider.tag;

            // 바닥 태그에 따라 적절한 소리를 반환
            switch (surfaceTag)
            {
                case "IronFloor":
                    Debug.Log("Iron Playing");
                    return footstepSoundData.ironFootstep;
                default:
                    return null; // 태그가 없으면 소리 없음
            }
        }
        return null; // 바닥이 감지되지 않은 경우
    }
}
