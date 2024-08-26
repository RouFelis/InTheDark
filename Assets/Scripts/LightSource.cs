using UnityEngine;

namespace InTheDark
{
    public class LightSource : MonoBehaviour
    {
        [SerializeField] 
        private float _angle;

        [SerializeField] 
        private float _distance;

        public float Angle => _angle;
        public float Distance => _distance;
    }
}