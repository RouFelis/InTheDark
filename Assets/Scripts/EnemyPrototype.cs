using System;
using UnityEngine;

namespace InTheDark
{
    public class EnemyPrototype : MonoBehaviour
    {
        private void Update()
        {
            Debug.DrawRay(transform.position, transform.forward * 5.0f, Color.magenta);
        }
    }
}