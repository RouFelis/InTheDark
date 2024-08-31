using System;
using UnityEngine;

namespace InTheDark
{
    // 임시 플레이어 스크립트임. -인코딩 테스트 용 한글 주석-
    public class PMovement : MonoBehaviour
    {
        public float Speed;
        
        private void Update()
        {            
            var speed = Speed * Time.deltaTime;
            
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                transform.position += Vector3.forward * speed;
            }
            
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position += Vector3.left * speed;
            }
            
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                transform.position += Vector3.back * speed;
            }
            
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                transform.position += Vector3.right * speed;
            }
        }
    }
}