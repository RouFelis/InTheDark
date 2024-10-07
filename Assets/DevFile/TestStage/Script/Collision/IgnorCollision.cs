using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnorCollision : MonoBehaviour
{
    void Start()
    {
        // 예: 레이어 8과 레이어 9의 충돌을 무시
        Physics.IgnoreLayerCollision(6, 7, true);
    }
}
