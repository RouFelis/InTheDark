using UnityEngine;
using UnityEngine.Localization;

public class InteractableObject_NonNet : MonoBehaviour
{
    public float radius = 0.25f;               // 상호작용을 위해 얼마나 가까워야 하는지 설정 (반경)
    public Transform interactionTransform;     // 상호작용을 수행할 위치를 나타내는 트랜스폼 (위치를 오프셋할 때 사용)

    bool isFocus = false;   // 현재 이 상호작용 가능한 오브젝트가 집중되고 있는지 여부
    Transform player;       // 플레이어의 트랜스폼을 참조하기 위한 변수

    bool hasInteracted = false; // 이미 오브젝트와 상호작용했는지 여부
    public string objectName;
    public LocalizedString localizedString;
    public bool IsDragable = false;


    public void SetObjectName()
    {
        localizedString.TableEntryReference = "InteractTable";
        localizedString.TableEntryReference = "";
        objectName = localizedString.GetLocalizedString();
    }


    // 상호작용 메서드는 자식 클래스에서 재정의되도록 설계됨
    public virtual void Interact(ulong userId, Transform interactingObjectTransform)
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

    void OnDrawGizmosSelected()
    {
        if (interactionTransform == null)
            interactionTransform = transform;

        Gizmos.color = Color.yellow;  // 색상을 노란색으로 설정
        Gizmos.DrawWireSphere(interactionTransform.position, radius);  // 상호작용 반경을 그려줌
    }

}
