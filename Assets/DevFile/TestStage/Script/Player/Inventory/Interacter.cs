using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;


public class Interacter : MonoBehaviour
{
    public LocalizedString localizedString;
    public int interactDistance = 2;
    private TextMeshProUGUI infoText;
    private NetworkObject netobject;

    void Update()
	{
        HandleRaycast();
    }

	public void Start()
	{
        StartCoroutine(InitializeUIElements());
    }


    //������ ������Ʈ Ȯ�� (Item Object Check)
	public void HandleRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, interactDistance))
        {
            if (hit.transform.CompareTag("Item"))
            {
                PickupItem item = hit.transform.GetComponent<PickupItem>();
                if (item != null)
                {
                    localizedString.TableReference = "ItemTable"; // ����ϰ��� �ϴ� ���̺�
                    localizedString.TableEntryReference = item.networkInventoryItemData.Value.itemName.ToString(); // ������ �̸�
                    infoText.text = $"{localizedString.GetLocalizedString()} \n";
                    localizedString.TableReference = "InteractTable"; // ����ϰ��� �ϴ� ���̺�
                    localizedString.TableEntryReference = "Grab"; // ����ϰ��� �ϴ� Ű
                    infoText.text += $"{localizedString.GetLocalizedString()} ({KeySettingsManager.Instance.InteractKey}) \n";
                    localizedString.TableEntryReference = "Energy"; // ����ϰ��� �ϴ� Ű
                    infoText.text += $"{localizedString.GetLocalizedString()} ({item.networkInventoryItemData.Value.price})";
                    infoText.gameObject.SetActive(true);
                    return;
                }
            }
            if (hit.transform.CompareTag("InteractableObject"))
            {
                localizedString.TableReference = "InteractTable"; // ����ϰ��� �ϴ� ���̺�
                localizedString.TableEntryReference = hit.transform.name; // ����ϰ��� �ϴ� Ű
                infoText.text = $"{localizedString.GetLocalizedString()} ({KeySettingsManager.Instance.InteractKey}) \n";
                
				if (Input.GetKeyDown(KeySettingsManager.Instance.InteractKey))
				{
                    hit.transform.gameObject.GetComponent<InteractableObject>().Interact(netobject.OwnerClientId , this.transform);
				}
                infoText.gameObject.SetActive(true);
                return;
            }
        }
        infoText.gameObject.SetActive(false);
    }

    private IEnumerator InitializeUIElements()
    {
        // PickupText ������Ʈ ã��
        while (infoText == null)
        {
            GameObject pickupTextObject = GameObject.Find("PickupText");
            Debug.Log("infoText �� ã���� �Դϴ�.");
            if (pickupTextObject != null)
            {
                infoText = pickupTextObject.GetComponent<TextMeshProUGUI>();
            }
            yield return null;
        }     
        
        while (netobject == null)
        {
            netobject = this.gameObject.GetComponent<NetworkObject>();
            Debug.Log("netobject �� ã���� �Դϴ�.");
            yield return null;
        }

        // �ʱ⿡�� �Ⱦ� �ؽ�Ʈ �����
        infoText.gameObject.SetActive(false);
    }

}
