using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class TestAction : Action
{
    public SharedFloat wanderRadius = 10f; // 배회 반경
    public SharedFloat wanderInterval = 3f; // 목표 갱신 간격
    public LayerMask roomLayerMask; // 방 감지에 사용할 레이어
    private NavMeshAgent agent; // NavMeshAgent 참조
    private float timer; // 시간 누적 변수
    private bool isMoving = false; // 이동 상태 플래그
    private Vector3 roomCenter; // 현재 방의 중심

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderInterval.Value; // 초기화
        UpdateRoomCenter(); // 방 중심 업데이트
    }

    public override TaskStatus OnUpdate()
    {
        // 현재 이동 상태 확인
        if (isMoving && agent.remainingDistance > agent.stoppingDistance)
        {
            return TaskStatus.Running; // 목표로 이동 중
        }

        // 도달했으면 이동 상태 초기화
        if (isMoving && agent.remainingDistance <= agent.stoppingDistance)
        {
            isMoving = false;
            return TaskStatus.Success; // 이동 완료
        }

        timer += Time.deltaTime;

        // 지정된 간격마다 새로운 목표 생성
        if (timer >= wanderInterval.Value)
        {
            // 랜덤하게 방을 변경하거나 현재 방에서 이동
            Vector3 newTarget = ShouldChangeRoom() ? GetNewRoomPosition() : GetWeightedRandomPoint(transform.position, wanderRadius.Value);

            if (newTarget != Vector3.zero)
            {
                agent.SetDestination(newTarget);
                Debug.Log($"[Wander Task] 새로운 목표 위치: {newTarget}");
                timer = 0f; // 타이머 초기화
                return TaskStatus.Running;
            }
            else
            {
                Debug.LogWarning("[Wander Task] 유효한 목표 위치를 찾지 못했습니다.");
                return TaskStatus.Failure;
            }
        }

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            return TaskStatus.Running;
        }

        return TaskStatus.Success;
    }
    private void UpdateRoomCenter()
    {
        Collider[] rooms = Physics.OverlapSphere(transform.position, wanderRadius.Value, roomLayerMask);
        if (rooms.Length > 0)
        {
            // 가장 가까운 방을 기준으로 중심 업데이트
            roomCenter = rooms[0].bounds.center;
            Debug.Log($"[Wander Task] 방 중심 업데이트: {roomCenter}");
        }
        else
        {
            Debug.LogWarning("[Wander Task] 방 중심을 찾지 못했습니다.");
            roomCenter = transform.position; // 기본값: 현재 위치
        }
    }

    private bool ShouldChangeRoom()
    {
        // 방을 변경할 확률 (20% 확률로 방 이동)
        return Random.value < 0.2f;
    }

    private Vector3 GetNewRoomPosition()
    {
        // 근처 방을 감지
        Collider[] rooms = Physics.OverlapSphere(transform.position, wanderRadius.Value, roomLayerMask);
        if (rooms.Length > 0)
        {
            // 랜덤한 방 선택
            Collider selectedRoom = rooms[Random.Range(0, rooms.Length)];
            isMoving = true; // 이동 상태 설정
            return selectedRoom.transform.position; // 방의 중심으로 이동
        }
        return Vector3.zero; // 방을 찾지 못한 경우
    }

    private Vector3 GetWeightedRandomPoint(Vector3 center, float radius)
    {
        const int maxAttempts = 30; // 최대 시도 횟수
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += center;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            {
                // 중앙에 가까운 위치를 선호
                float distanceToCenter = Vector3.Distance(center, hit.position);
                float weight = Mathf.InverseLerp(radius, 0, distanceToCenter);

                if (Random.value < weight)
                {
                    return hit.position;
                }
            }
        }

        return Vector3.zero;
    }

    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius.Value);
    }
}
