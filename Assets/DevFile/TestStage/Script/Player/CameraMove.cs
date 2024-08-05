using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraMove : NetworkBehaviour
{
    public float speed = 5.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    void Start()
    {
        if (!IsOwner) return;
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서를 중앙에 고정
    }

    void Update()
    {
        if (!IsOwner) return;

        // 이동
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float curSpeedX = Input.GetAxis("Vertical") * speed;
        float curSpeedY = Input.GetAxis("Horizontal") * speed;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // 중력 적용
        if (!characterController.isGrounded)
        {
            moveDirection.y -= 9.8f * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // 카메라 회전
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        transform.localEulerAngles = new Vector3(rotationX, transform.localEulerAngles.y + Input.GetAxis("Mouse X") * lookSpeed, 0);
    }
}
