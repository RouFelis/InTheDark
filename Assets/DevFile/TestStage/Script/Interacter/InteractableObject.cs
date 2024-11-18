using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Localization;

public class InteractableObject : NetworkBehaviour
{
    public float radius = 0.25f;               // ��ȣ�ۿ��� ���� �󸶳� ������� �ϴ��� ���� (�ݰ�)
    public Transform interactionTransform;     // ��ȣ�ۿ��� ������ ��ġ�� ��Ÿ���� Ʈ������ (��ġ�� �������� �� ���)

//    LayerMask interactableMask;  // ��ȣ�ۿ� ������ ������Ʈ�� ���� ���̾� ����ũ (�Ⱦ�)

    bool isFocus = false;   // ���� �� ��ȣ�ۿ� ������ ������Ʈ�� ���ߵǰ� �ִ��� ����
    Transform player;       // �÷��̾��� Ʈ�������� �����ϱ� ���� ����

    bool hasInteracted = false; // �̹� ������Ʈ�� ��ȣ�ۿ��ߴ��� ����
    public string objectName;
    public LocalizedString localizedString;

    public virtual void Start()
    {
     //   gameObject.layer = LayerMask.NameToLayer("InteractableObject");  // ������Ʈ�� ���̾ 'InteractableObject'�� ����
       // SetObjectName();
    }


    public void SetObjectName()
	{
        localizedString.TableEntryReference = "InteractTable";
        localizedString.TableEntryReference = "";
        objectName = localizedString.GetLocalizedString();
    }


    // ��ȣ�ۿ� �޼���� �ڽ� Ŭ�������� �����ǵǵ��� �����
    public virtual void Interact(Transform interactingObjectTransform)
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

    /// <summary>
    /// Update �޼��� ������ �÷��̾�� ��ȣ�ۿ� ������ ������Ʈ ������ �Ÿ��� üũ��.
    /// �� �޼��� ������ ��ȣ�ۿ� �޼��尡 ȣ���.
    /// </summary>
    public void checkDistance()
    {
        // ���� �� ������Ʈ�� ���ߵǰ� �ְ�
        // ���� ��ȣ�ۿ����� ���� ��쿡�� ����
        if (isFocus && !hasInteracted)
        {
            // �÷��̾�� ����� ������� Ȯ��
            float distance = Vector3.Distance(player.position, interactionTransform.position);
            Vector3 direction = (transform.position - player.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
            float angle = Mathf.Abs(player.rotation.eulerAngles.y - lookRotation.eulerAngles.y);

            // �Ÿ��� ������ �����ϸ� ��ȣ�ۿ� ����
            if ((distance <= radius) && (angle <= 5))
            {
                // ������Ʈ�� ��ȣ�ۿ�
                Interact(player);
                hasInteracted = true;  // ��ȣ�ۿ� �Ϸ� ǥ��
            }
        }
    }

    // �����Ϳ��� �ݰ��� �ð������� �׷���
    void OnDrawGizmosSelected()
    {
        if (interactionTransform == null)
            interactionTransform = transform;

        Gizmos.color = Color.yellow;  // ������ ��������� ����
        Gizmos.DrawWireSphere(interactionTransform.position, radius);  // ��ȣ�ۿ� �ݰ��� �׷���
    }

}
