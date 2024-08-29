using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

// 이 스크립트는 네트워크 게임 환경에서 인벤토리를 관리합니다.
public class NetworkInventoryController : NetworkBehaviour
{
    // 인벤토리 슬롯에 대한 UI 요소
    private RectTransform[] slots;
    private Image[] slotImages;
    private NetworkObject spawnedObjectParent;
    public List<InventoryItem> items = new List<InventoryItem>();  // 로컬 인벤토리 아이템 리스트
    public NetworkList<InventoryItemData> networkItems = new NetworkList<InventoryItemData>(); // 네트워크 동기화 인벤토리 아이템 리스트

    public LocalizedString localizedString;

    // 인벤토리 슬롯 키 바인딩
    public KeyCode[] slotKeys = new KeyCode[] {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5
    };

    public TextMeshProUGUI pickupText; // 아이템 픽업 알림 텍스트

    private NetworkVariable<int> selectedSlot = new NetworkVariable<int>(0); // 현재 선택된 슬롯

    // 키 설정 변수
    public KeySettingsManager keySettingsManager; // 키 설정 매니저
    [SerializeField] private PlaceableItemManager currentPlaceableItemManager; // 배치 가능한 아이템 매니저
    [SerializeField] private bool isPlacingItem = false; // 아이템 배치 여부 플래그

    public bool IsPlacingItem { get { return isPlacingItem; } set { isPlacingItem = value; } }

    private InventoryItem currentSelectedItem = null; // 현재 선택된 아이템

    #region 초기화

    void Start()
    {
        Transform inventoryTransform = GameObject.Find("Inventory").transform;
        int numberOfSlots = inventoryTransform.childCount;
        slots = new RectTransform[numberOfSlots];
        slotImages = new Image[numberOfSlots];

        // 슬롯 및 슬롯 이미지를 초기화
        for (int i = 0; i < numberOfSlots; i++)
        {
            Transform slotTransform = inventoryTransform.GetChild(i);
            slots[i] = slotTransform.GetComponent<RectTransform>();
            slotImages[i] = slotTransform.GetComponent<Image>();
        }

        // 필요한 UI 요소를 찾기 위한 코루틴 시작
        StartCoroutine(InitializeUIElements());
    }

    private IEnumerator InitializeUIElements()
    {
        // PickupText 오브젝트 찾기
        while (pickupText == null)
        {
            GameObject pickupTextObject = GameObject.Find("PickupText");
            if (pickupTextObject != null)
            {
                pickupText = pickupTextObject.GetComponent<TextMeshProUGUI>();
            }
            yield return null;
        }

        // KeySettingsManager 오브젝트 찾기
        while (keySettingsManager == null)
        {
            GameObject keySettingsManagerObject = GameObject.Find("KeySettingsManager");
            if (keySettingsManagerObject != null)
            {
                keySettingsManager = keySettingsManagerObject.GetComponent<KeySettingsManager>();
            }
            yield return null;
        }

        // PlaceableItemManager 오브젝트 찾기
        while (currentPlaceableItemManager == null)
        {
            GameObject placeableItemManagerObject = GameObject.Find("PlaceableItemManager");
            if (placeableItemManagerObject != null)
            {
                currentPlaceableItemManager = placeableItemManagerObject.GetComponent<PlaceableItemManager>();
            }
            yield return null;
        }

        // SpawnedObjects 부모 오브젝트 찾기
        while (spawnedObjectParent == null)
        {
            GameObject spawnedObjectParentObject = GameObject.Find("SpawnedObjects");
            if (spawnedObjectParentObject != null)
            {
                spawnedObjectParent = spawnedObjectParentObject.GetComponent<NetworkObject>();
            }
            yield return null;
        }

        // 서버 인벤토리 초기화
        InventoryServerRpc();

        // 초기에는 픽업 텍스트 숨기기
        pickupText.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && IsOwner)
        {
            selectedSlot.OnValueChanged += OnSlotChanged; // 슬롯 변경 이벤트 구독
        }

        networkItems.OnListChanged += OnNetworkItemsChanged; // 네트워크 아이템 리스트 변경 이벤트 구독
        InitializeSlotImages();
        UpdateSlotSelection();
    }

    #endregion

    #region 네트워크 아이템 처리

    private void OnNetworkItemsChanged(NetworkListEvent<InventoryItemData> changeEvent)
    {
        // 네트워크 아이템 리스트가 변경될 때 로컬 아이템 리스트 업데이트
        items.Clear();
        foreach (var data in networkItems)
        {
            var item = ScriptableObject.CreateInstance<InventoryItem>();
            item.CopyDataFrom(data);
            items.Add(item);
        }

        if (IsOwner && IsClient)
            InitializeSlotImages();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InventoryServerRpc()
    {
        // 네트워크 아이템이 초기화되지 않은 경우 초기화
        if (networkItems == null)
        {
            networkItems = new NetworkList<InventoryItemData>();
            Debug.Log("networkItems 초기화됨");
        }

        if (networkItems.Count == 0)
        {
            var test = items.Count;
            // 기본 아이템으로 네트워크 아이템 초기화
            for (int i = 0; i < test; i++)
            {
                var defaultItem = new InventoryItemData(new FixedString128Bytes(), new FixedString128Bytes(), new FixedString128Bytes(), new FixedString128Bytes(), new FixedString128Bytes(), false , false, 0 ,0 ,0);
                Debug.Log("기본 아이템을 networkItems에 추가");
                networkItems.Add(defaultItem);
                Debug.Log("기본 아이템이 networkItems에 추가됨");
            }
        }
        Debug.Log($"networkItems.Count: {networkItems.Count}, items.Count: {items.Count}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ItemInputRequestServerRpc(int slotIndex, ulong itemNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkObjectId, out NetworkObject itemNetworkObject))
        {
            // 새로운 아이템을 할당하기 전에 슬롯이 비어 있는지 확인
            if (!EqualityComparer<InventoryItemData>.Default.Equals(networkItems[slotIndex], default(InventoryItemData)))
            {
                bool slotFound = false;
                for (int i = 0; i < networkItems.Count; i++)
                {
                    if (EqualityComparer<InventoryItemData>.Default.Equals(networkItems[i], default(InventoryItemData)))
                    {
                        slotIndex = i;
                        slotFound = true;
                        break;
                    }
                }

                if (!slotFound)
                {
                    Debug.Log("빈 슬롯을 찾을 수 없음");
                    return;
                }
            }

            PickupItem item = itemNetworkObject.GetComponent<PickupItem>();

            // 새로운 인벤토리 아이템 데이터를 생성하고 네트워크 리스트에 할당
            InventoryItemData newItem = new InventoryItemData(
                new FixedString128Bytes(item.networkInventoryItemData.Value.itemName.ToString()),
                new FixedString128Bytes(item.networkInventoryItemData.Value.itemSpritePath.ToString()),
                new FixedString128Bytes(item.networkInventoryItemData.Value.previewPrefabPath.ToString()),
                new FixedString128Bytes(item.networkInventoryItemData.Value.objectPrefabPath.ToString()),
                new FixedString128Bytes(item.networkInventoryItemData.Value.dropPrefabPath.ToString()),
                item.networkInventoryItemData.Value.isPlaceable,
                item.networkInventoryItemData.Value.isUsable,
                item.networkInventoryItemData.Value.price,
                item.networkInventoryItemData.Value.maxPrice,
                item.networkInventoryItemData.Value.minPrice
            );

            networkItems[slotIndex] = newItem;

            // 클라이언트 측에서 아이템 로드
            LoadItemClientRpc(slotIndex, newItem);
        }
    }

    [ClientRpc]
    private void LoadItemClientRpc(int slotIndex, InventoryItemData newItem)
    {
        if (IsOwner && IsClient)
        {
            Sprite loadedSprite = Resources.Load<Sprite>(newItem.itemSpritePath.ToString());
            if (loadedSprite != null)
            {
                slotImages[slotIndex].sprite = loadedSprite;
                slotImages[slotIndex].enabled = true;
                Debug.Log($"{newItem.itemSpritePath.ToString()}에서 스프라이트가 성공적으로 로드됨");
            }
            else
            {
                slotImages[slotIndex].sprite = null;
                slotImages[slotIndex].enabled = false;
                Debug.LogError($"{newItem.itemSpritePath.ToString()}에서 스프라이트 로드 실패");
            }
        }
    }

    [ServerRpc]
    private void RequestPickupItemServerRpc(ulong itemNetworkObjectId, ulong clientId, int slotIndex)
    {
        PickupItemClientRpc(itemNetworkObjectId, clientId, slotIndex);
    }

    [ClientRpc]
    private void PickupItemClientRpc(ulong itemNetworkObjectId, ulong clientId, int slotIndex)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId)
        {
            Debug.Log("클라이언트 ID가 일치하지 않음");
            return;
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkObjectId, out NetworkObject itemNetworkObject))
        {
            ItemInputRequestServerRpc(slotIndex, itemNetworkObjectId);
            RequestDespawnItemServerRpc(itemNetworkObjectId);
            UpdateSlotSelection();
        }
        else
        {
            Debug.Log("아이템 NetworkObject를 찾을 수 없음");
        }
    }

    [ServerRpc]
    private void RequestDespawnItemServerRpc(ulong itemNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkObjectId, out NetworkObject itemNetworkObject))
        {
            itemNetworkObject.Despawn();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSlotChangeServerRpc(int newSlot)
    {
        selectedSlot.Value = newSlot;
        UpdateSlotSelectionClientRpc(newSlot);
    }

    [ClientRpc]
    private void UpdateSlotSelectionClientRpc(int newSlot)
    {
        if (isPlacingItem)
        {
            currentPlaceableItemManager.CancelPreview();
            isPlacingItem = false;
            currentSelectedItem = null;
        }
        if (!IsOwner)
        {
            selectedSlot.Value = newSlot;
        }
        UpdateSlotSelection();
    }

    #endregion

    #region 업데이트 및 입력 처리

    void Update()
    {
        if (IsClient && IsOwner)
        {
            HandleKeyboardInput();
            HandleMouseWheelInput();
            HandleInteract();
            HandleDropItem();
            HandleRaycast();
            HandleUseItem(); // 아이템 사용 처리 추가
        }
        if (isPlacingItem)
        {
            currentPlaceableItemManager.UpdatePreviewObject();
            currentPlaceableItemManager.HandleRotation(ref isPlacingItem , currentSelectedItem.objectPrefabPath );
        }
    }

    private void HandleKeyboardInput()
    {
        for (int i = 0; i < slotKeys.Length; i++)
        {
            if (Input.GetKeyDown(slotKeys[i]))
            {
                if (isPlacingItem)
                {
                    // 이전 미리보기 오브젝트가 있을 경우 삭제
                    currentPlaceableItemManager.CancelPreview();
                    isPlacingItem = false;
                    currentSelectedItem = null;
                }
                RequestSlotChangeServerRpc(i);
            }
        }
    }

    private void HandleMouseWheelInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (isPlacingItem)
            {
                // 이전 미리보기 오브젝트가 있을 경우 삭제
                currentPlaceableItemManager.CancelPreview();
                isPlacingItem = false;
                currentSelectedItem = null;
            }
            int newSlot = (selectedSlot.Value + 1) % slots.Length;
            RequestSlotChangeServerRpc(newSlot);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (isPlacingItem)
            {
                // 이전 미리보기 오브젝트가 있을 경우 삭제
                currentPlaceableItemManager.CancelPreview();
                isPlacingItem = false;
                currentSelectedItem = null;
            }
            int newSlot = (selectedSlot.Value - 1 + slots.Length) % slots.Length;
            RequestSlotChangeServerRpc(newSlot);
        }
    }

    private void HandleUseItem()
    {
        InventoryItem currentItem = items[selectedSlot.Value];
        if (currentItem != null && currentItem.isPlaceable)
        {
            if (isPlacingItem)
            {
                // 이전 미리보기 오브젝트가 있을 경우 삭제
                currentPlaceableItemManager.CancelPreview();
                isPlacingItem = false;
                currentSelectedItem = null;
            }

            // 새로운 플레이스 아이템을 사용
            currentSelectedItem = currentItem;

            // PlaceableItemManager 초기화
            currentPlaceableItemManager.previewPrefab = currentItem.PreviewPrefab;
            currentPlaceableItemManager.objectPrefab = currentItem.ObjectPrefab;
            currentPlaceableItemManager.InitializePreviewObject(currentItem.PreviewPrefab); // 미리보기 오브젝트 초기화
            if (isPlacingItem == false)
            {
                isPlacingItem = true; // 배치 모드 활성화
            }

        }
        else if (currentItem == null || !currentItem.isPlaceable)
        {
            currentPlaceableItemManager.PreviewDestroy(); // 미리보기 삭제하기
            isPlacingItem = false; // 배치 모드 비활성화
        }
    }

    private void HandleInteract()
    {
        KeyCode interactKey = keySettingsManager.GetKey("Interact");
        if (Input.GetKeyDown(interactKey))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f))
            {
                if (hit.transform.CompareTag("Item"))
                {
                    PickupItem item = hit.transform.GetComponent<PickupItem>();
                    if (item != null && item.GetComponent<NetworkObject>() != null)
                    {
                        ulong itemNetworkObjectId = item.GetComponent<NetworkObject>().NetworkObjectId;
                        RequestPickupItemServerRpc(itemNetworkObjectId, NetworkManager.Singleton.LocalClientId, selectedSlot.Value);
                    }
                }
            }
        }
    }

    private void HandleDropItem()
    {
        KeyCode dropKey = keySettingsManager.GetKey("Drop");
        if (Input.GetKeyDown(dropKey))
        {
            InventoryItem currentItem = items[selectedSlot.Value];
            if (currentItem != null)
            {
                if (isPlacingItem)
                {
                    // 미리보기 오브젝트 삭제
                    currentPlaceableItemManager.CancelPreview();
                    isPlacingItem = false;
                    currentSelectedItem = null;
                }

                Vector3 dropPosition = transform.position + transform.forward * 2;
                Quaternion dropRotation = Quaternion.LookRotation(transform.forward);
                if (Physics.Raycast(transform.position + transform.forward, Vector3.down, out RaycastHit hit, 2.0f))
                {
                    dropPosition = hit.point;
                }
                else
                {
                    // 바닥이 발견되지 않으면 플레이어 앞에 두기
                    dropPosition = transform.position + transform.forward * 2;
                }

                DropItemServerRpc(currentItem.itemName, dropPosition, dropRotation, selectedSlot.Value);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropItemServerRpc(string itemName, Vector3 position, Quaternion rotation, int slotIndex)
    {
        InventoryItem itemToDrop = items[slotIndex];
        if (itemToDrop != null)
        {
            GameObject itemPrefab = itemToDrop.DropPrefab; // 경로를 통한 프리팹 로드
            if (itemPrefab != null)
            {
                GameObject droppedItem = Instantiate(itemPrefab, position, rotation);
                PickupItem temptItem = droppedItem.GetComponent<PickupItem>();


                var updatedItemData = new InventoryItemData(
                    itemToDrop.itemName,
                    itemToDrop.itemSpritePath,
                    itemToDrop.previewPrefabPath,
                    itemToDrop.objectPrefabPath,
                    itemToDrop.dropPrefabPath,
                    itemToDrop.isPlaceable,
                    itemToDrop.isUsable,
                    itemToDrop.price, // 여기서 가격만 변경
                    itemToDrop.maxPrice,
                    itemToDrop.minPrice
                );

                temptItem.networkInventoryItemData.Value = updatedItemData;

                NetworkObject networkObject = droppedItem.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Spawn();
                    NetworkObjectChangeParentsClientRpc(networkObject.NetworkObjectId);
                    if (IsOwner)
                    {
                        NetworkObject temptNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObject.NetworkObjectId];
                        NetworkObject parentObject = NetworkManager.SpawnManager.SpawnedObjects[spawnedObjectParent.NetworkObjectId];
                        temptNetworkObject.transform.SetParent(parentObject.transform, true);
                    }
                  
                }
                RequestRemoveItemFromInventoryServerRpc(slotIndex);
            }
            else
            {
                Debug.LogError("Item prefab is null for item: " + itemName);
            }
        }
        else
        {
            Debug.LogError("Item to drop is null at slot: " + slotIndex);
        }
    }

    [ClientRpc]
    void NetworkObjectChangeParentsClientRpc(ulong objectId)
    {

    }

    private void HandleRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f))
        {
            if (hit.transform.CompareTag("Item"))
            {
                PickupItem item = hit.transform.GetComponent<PickupItem>();
                if (item != null)
                {
                    KeyCode interactKey = keySettingsManager.GetKey("Interact");
                    localizedString.TableReference = "InteractTable"; // 사용하고자 하는 테이블
                    localizedString.TableEntryReference = "Grab"; // 사용하고자 하는 키
                    pickupText.text = $"{item.networkInventoryItemData.Value.itemName} \n {localizedString.GetLocalizedString()} ({interactKey}) \n ";
                    localizedString.TableEntryReference = "Energy"; // 사용하고자 하는 키
                    pickupText.text += $"{localizedString.GetLocalizedString()} ({item.networkInventoryItemData.Value.price})";
                    pickupText.gameObject.SetActive(true);
                    return;
                }
            }
        }
        pickupText.gameObject.SetActive(false);
    }

    #endregion

    #region 인벤토리 아이템 사용 및 제거

    public void UseItem(InventoryItem item)
    {
        Debug.Log("Use item: " + item.ItemSprite.name);

        // 아이템 사용 후 인벤토리에서 제거
        RequestRemoveItemFromInventoryServerRpc(selectedSlot.Value);
    }

    public void UseCurrentSelectedItem(ulong objectID)
    {
        InventoryItem currentItem = items[selectedSlot.Value];
        if (currentItem != null)
        {
            // 아이템 사용 후 인벤토리에서 제거
            RequestItemDataUpdataServerRpc(selectedSlot.Value , objectID);
            // 배치 모드 비활성화
            isPlacingItem = false;
            currentPlaceableItemManager.clientRpcCompleted = false;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestItemDataUpdataServerRpc(int slotIndex, ulong objectID, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        if (GetComponent<NetworkObject>().OwnerClientId == senderClientId)
        {
            Debug.Log("ddddd");
            InventoryItem currentItem = items[selectedSlot.Value];
            Debug.Log("ccccc");
            var updatedItemData = new InventoryItemData(
                  currentItem.itemName,
                  currentItem.itemSpritePath,
                  currentItem.previewPrefabPath,
                  currentItem.objectPrefabPath,
                  currentItem.dropPrefabPath,
                  currentItem.isPlaceable,
                  currentItem.isUsable,
                  currentItem.price, // 여기서 가격만 변경
                  currentItem.maxPrice,
                  currentItem.minPrice
              );

            NetworkManager.SpawnManager.SpawnedObjects[objectID].gameObject.GetComponent<PickupItem>().networkInventoryItemData.Value = updatedItemData;

            CompleteClientRpc(slotIndex);
        }      
    }
    [ClientRpc]
    private void CompleteClientRpc(int slotIndex)
	{
        RequestRemoveItemFromInventoryServerRpc(slotIndex);
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestRemoveItemFromInventoryServerRpc(int slotIndex , ServerRpcParams rpcParams = default)
    {
        networkItems[slotIndex] = new InventoryItemData(); // 빈 데이터를 할당하여 초기화
        RemoveItemFromInventoryClientRpc(slotIndex);
    }

    [ClientRpc]
    private void RemoveItemFromInventoryClientRpc(int slotIndex)
    {
        UpdateSlotSelection();
    }

    #endregion

    #region 슬롯 선택 업데이트

    private void UpdateSlotSelection()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == selectedSlot.Value)
            {
                slots[i].localScale = Vector3.one * 1.2f;
            }
            else
            {
                slots[i].localScale = Vector3.one;
            }
        }
    }

    private void InitializeSlotImages()
    {
		try
		{
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ItemSprite != null)
                {
                    slotImages[i].sprite = items[i].ItemSprite;
                    slotImages[i].enabled = true;
                }
                else
                {
                    slotImages[i].enabled = false;
                }
            }
        }
		catch
		{
            Debug.Log("ItemSprite serching ");
		}     
    }

    void OnNetworkDestroy()
    {
        if (networkItems != null)
        {
            networkItems.Dispose();
        }
    }

    private void OnSlotChanged(int oldSlot, int newSlot)
    {
        if (isPlacingItem)
        {
            // 슬롯 변경 시 미리보기 오브젝트가 있을 경우 삭제
            currentPlaceableItemManager.CancelPreview();
            isPlacingItem = false;
            currentSelectedItem = null;
        }
        UpdateSlotSelection();
    }

    #endregion
}
