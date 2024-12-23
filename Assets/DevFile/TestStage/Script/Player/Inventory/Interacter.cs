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


    //아이템 오브젝트 확인 (Item Object Check)
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
                    localizedString.TableReference = "ItemTable"; // 사용하고자 하는 테이블
                    localizedString.TableEntryReference = item.networkInventoryItemData.Value.itemName.ToString(); // 아이템 이름
                    infoText.text = $"{localizedString.GetLocalizedString()} \n";
                    localizedString.TableReference = "InteractTable"; // 사용하고자 하는 테이블
                    localizedString.TableEntryReference = "Grab"; // 사용하고자 하는 키
                    infoText.text += $"{localizedString.GetLocalizedString()} ({KeySettingsManager.Instance.InteractKey}) \n";
                    localizedString.TableEntryReference = "Energy"; // 사용하고자 하는 키
                    infoText.text += $"{localizedString.GetLocalizedString()} ({item.networkInventoryItemData.Value.price})";
                    infoText.gameObject.SetActive(true);
                    return;
                }
            }
            if (hit.transform.CompareTag("InteractableObject"))
            {
                localizedString.TableReference = "InteractTable"; // 사용하고자 하는 테이블
                localizedString.TableEntryReference = hit.transform.name; // 사용하고자 하는 키
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
        // PickupText 오브젝트 찾기
        while (infoText == null)
        {
            GameObject pickupTextObject = GameObject.Find("PickupText");
            Debug.Log("infoText 를 찾는중 입니다.");
            if (pickupTextObject != null)
            {
                infoText = pickupTextObject.GetComponent<TextMeshProUGUI>();
            }
            yield return null;
        }     
        
        while (netobject == null)
        {
            netobject = this.gameObject.GetComponent<NetworkObject>();
            Debug.Log("netobject 를 찾는중 입니다.");
            yield return null;
        }

        // 초기에는 픽업 텍스트 숨기기
        infoText.gameObject.SetActive(false);
    }

}
