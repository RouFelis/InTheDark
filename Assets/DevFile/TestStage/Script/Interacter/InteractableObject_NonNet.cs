using UnityEngine;
using UnityEngine.Localization;

public class InteractableObject_NonNet : MonoBehaviour
{
    public float radius = 0.25f;               // ��ȣ�ۿ��� ���� �󸶳� ������� �ϴ��� ���� (�ݰ�)
    public Transform interactionTransform;     // ��ȣ�ۿ��� ������ ��ġ�� ��Ÿ���� Ʈ������ (��ġ�� �������� �� ���)

    bool isFocus = false;   // ���� �� ��ȣ�ۿ� ������ ������Ʈ�� ���ߵǰ� �ִ��� ����
    Transform player;       // �÷��̾��� Ʈ�������� �����ϱ� ���� ����

    bool hasInteracted = false; // �̹� ������Ʈ�� ��ȣ�ۿ��ߴ��� ����
    public string objectName;
    public LocalizedString localizedString;
    public bool IsDragable = false;


    public void SetObjectName()
    {
        localizedString.TableEntryReference = "InteractTable";
        localizedString.TableEntryReference = "";
        objectName = localizedString.GetLocalizedString();
    }


    // ��ȣ�ۿ� �޼���� �ڽ� Ŭ�������� �����ǵǵ��� �����
    public virtual void Interact(ulong userId, Transform interactingObjectTransform)
    {
        Debug.Log(interactingObjectTransform.name + " has interacted with " + transform.name);
    }

    // ������Ʈ�� �÷��̾��� ���� ����� �Ǿ��� �� ȣ���
    public void OnFocused(Transform playerTransform)
    {
        isFocus = true;
        player = playerTransform;
        hasInteracted = false;
        //EnableOutlines();  // �ܰ��� Ȱ��ȭ
    }

    // ������Ʈ�� �� �̻� �÷��̾��� ���� ����� �ƴϰ� �Ǿ��� �� ȣ���
    public void OnDefocused()
    {
        isFocus = false;
        player = null;
        hasInteracted = false;
        //DisableOutlines();  // �ܰ��� ��Ȱ��ȭ
    }

    void OnDrawGizmosSelected()
    {
        if (interactionTransform == null)
            interactionTransform = transform;

        Gizmos.color = Color.yellow;  // ������ ��������� ����
        Gizmos.DrawWireSphere(interactionTransform.position, radius);  // ��ȣ�ۿ� �ݰ��� �׷���
    }

}
