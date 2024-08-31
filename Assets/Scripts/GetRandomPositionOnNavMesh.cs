using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

using UnityEngine;
using UnityEngine.AI;

public class GetRandomPositionOnNavMesh : Action
{
    public float radius = 20.0f;
    
    // The transform that the object is moving towards
    public SharedVector3 target;

    public override TaskStatus OnUpdate()
    {
        var randomDirection = Random.insideUnitSphere * radius + transform.position; // 원하는 범위 내의 랜덤한 방향 벡터를 생성합니다.
        var isOnNavMesh = NavMesh.SamplePosition(randomDirection, out var hit, radius, NavMesh.AllAreas); // 랜덤 위치가 NavMesh 위에 있는지 확인합니다.
        var result = isOnNavMesh ? TaskStatus.Success : TaskStatus.Failure;
        
        target.Value = isOnNavMesh ? hit.position : transform.position;

        return result;
    }
}