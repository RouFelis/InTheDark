using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnorCollision : MonoBehaviour
{
    void Start()
    {
        // ��: ���̾� 8�� ���̾� 9�� �浹�� ����
        Physics.IgnoreLayerCollision(6, 7, true);
    }
}
