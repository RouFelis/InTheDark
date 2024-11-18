using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Localization;

public class InteractableObject : NetworkBehaviour
{
    public float radius = 0.25f;               // 상호작용을 위해 얼마나 가까워야 하는지 설정 (반경)
    public Transform interactionTransform;     // 상호작용을 수행할 위치를 나타내는 트랜스폼 (위치를 오프셋할 때 사용)

//    LayerMask interactableMask;  // 상호작용 가능한 오브젝트를 위한 레이어 마스크 (안씀)

    bool isFocus = false;   // 현재 이 상호작용 가능한 오브젝트가 집중되고 있는지 여부
    Transform player;       // 플레이어의 트랜스폼을 참조하기 위한 변수

    bool hasInteracted = false; // 이미 오브젝트와 상호작용했는지 여부
    public string objectName;
    public LocalizedString localizedString;

    public virtual void Start()
    {
     //   gameObject.layer = LayerMask.NameToLayer("InteractableObject");  // 오브젝트의 레이어를 'InteractableObject'로 설정
       // SetObjectName();
    }


    public void SetObjectName()
	{
        localizedString.TableEntryReference = "InteractTable";
        localizedString.TableEntryReference = "";
        objectName = localizedString.GetLocalizedString();
    }


    // 상호작용 메서드는 자식 클래스에서 재정의되도록 설계됨
    public virtual void Interact(Transform interactingObjectTransform)
    {
        Debug.Log(interactingObjectTransform.name + " has interacted with " + transform.name);
    }    

    // 오브젝트가 플레이어의 집중 대상이 되었을 때 호출됨
    public void OnFocused(Transform playerTransform)
    {
        isFocus = true;
        player = playerTransform;
        hasInteracted = false;
        //EnableOutlines();  // 외곽선 활성화
    }

    // 오브젝트가 더 이상 플레이어의 집중 대상이 아니게 되었을 때 호출됨
    public void OnDefocused()
    {
        isFocus = false;
        player = null;
        hasInteracted = false;
        //DisableOutlines();  // 외곽선 비활성화
    }

    /// <summary>
    /// Update 메서드 내에서 플레이어와 상호작용 가능한 오브젝트 사이의 거리를 체크함.
    /// 이 메서드 내에서 상호작용 메서드가 호출됨.
    /// </summary>
    public void checkDistance()
    {
        // 현재 이 오브젝트가 집중되고 있고
        // 아직 상호작용하지 않은 경우에만 실행
        if (isFocus && !hasInteracted)
        {
            // 플레이어와 충분히 가까운지 확인
            float distance = Vector3.Distance(player.position, interactionTransform.position);
            Vector3 direction = (transform.position - player.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
            float angle = Mathf.Abs(player.rotation.eulerAngles.y - lookRotation.eulerAngles.y);

            // 거리와 각도가 적절하면 상호작용 실행
            if ((distance <= radius) && (angle <= 5))
            {
                // 오브젝트와 상호작용
                Interact(player);
                hasInteracted = true;  // 상호작용 완료 표시
            }
        }
    }

    // 에디터에서 반경을 시각적으로 그려줌
    void OnDrawGizmosSelected()
    {
        if (interactionTransform == null)
            interactionTransform = transform;

        Gizmos.color = Color.yellow;  // 색상을 노란색으로 설정
        Gizmos.DrawWireSphere(interactionTransform.position, radius);  // 상호작용 반경을 그려줌
    }

}
