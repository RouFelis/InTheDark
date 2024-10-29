using System;
using System.Collections.Generic;
using UnityEngine;

namespace InTheDark
{
    // 안 씀, 그냥 나중에 작성할 시야 클래스 구조 잡는 용도
    [CreateAssetMenu(fileName = FILE_NAME, menuName = MENU_NAME)]
    public class ScriptableFieldOfView : ScriptableObject
    {
        [Serializable]
        public class Instance : IDisposable
        {
            [SerializeField] 
            private int _index;
            
            [SerializeField]
            private Collider[] _colliders;
            
            [SerializeField]
            private Transform _transform;

            public void Dispose()
            {
                // TODO release managed resources here

                _index = default;

                _colliders = default;
                _transform = default;
            }
        }
        
        private const string FILE_NAME = "FieldOfView";
        private const string MENU_NAME = "Field Of View";
        
        private float _angle;
        private float _distance;

        private int _maxCount;
        
        private LayerMask _layerMask;
        
        // Template, Settings

        // public override TaskStatus OnUpdate()
        // {
        //     for (var i = 0; i < size; i++)
        //     {
        //         colliders[i] = default;
        //     }
        //
        //     size = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, targetLayer);
        //
        //     for (var i = 0; i < size; i++)
        //     {
        //         var element = colliders[i];
        //
        //         var direction = element.transform.position - transform.position;
        //         var isOccultation = Physics.Raycast(transform.position, direction, out var hit, distance);
        //         var isSight = Vector3.Angle(direction, transform.forward) < fieldOfViewAngle;
        //     
        //         OnDrawRaycastGizmo(element, hit, direction, isSight);
        //     
        //         if (hit.collider == element && isOccultation && isSight) 
        //         {
        //             // Set the target so other tasks will know which transform is within sight
        //             target.Value = element.transform.position;
        //                 
        //             return TaskStatus.Success;
        //         }
        //     }
        //
        //     return TaskStatus.Failure;
        // }
    }
}