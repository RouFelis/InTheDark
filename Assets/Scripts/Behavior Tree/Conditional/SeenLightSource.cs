using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

using InTheDark.Prototypes;

using UnityEngine;

namespace InTheDark
{
    public class SeenLightSource : Conditional
    {
        public float distance;
        public float fieldOfViewAngle;
        
        // The LayerMask of the targets
        public LayerMask targetLayer;
        
        public override TaskStatus OnUpdate()
        {
            var possibleTargets = Physics.OverlapSphere(transform.position, distance, targetLayer);

            // Return success if a target is within sight
            foreach (var element in possibleTargets)
            {
                var lightSource = element.GetComponent<LightSource>();
                
                var direction = element.transform.position - transform.position;
                var angle = Vector3.Angle(direction, transform.forward);
                var current = Vector3.Distance(element.transform.position, transform.position);
                
                var isOccultation = Physics.Raycast(transform.position, direction, out var hit, distance);
                
                var isSight = Mathf.Approximately(Mathf.Min(angle, fieldOfViewAngle, lightSource.Angle), angle);
                var isClose = Mathf.Approximately(Mathf.Min(current, distance, lightSource.Distance), current);
                
                OnDrawRaycastGizmo(element, hit, direction, isSight, isClose);
            
                if (hit.collider == element && isOccultation && isSight && isClose) 
                {
                    return TaskStatus.Success;
                }
            }
        
            return TaskStatus.Failure;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnDrawRaycastGizmo(Collider collider, RaycastHit hit, Vector3 direction, bool isSight, bool isClose)
        {
            var color = hit.collider == collider && isSight && isClose ? Color.green : Color.yellow;
            
            Debug.DrawRay(transform.position, direction, color);
        }
    }
}