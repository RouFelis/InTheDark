using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Netcode;

// �� ��ũ��Ʈ�� ��Ʈ��ũ ���� ȯ�濡�� �κ��丮�� �����մϴ�.
public class NetworkInventoryController : NetworkBehaviour
{
    // �κ��丮 ���Կ� ���� UI ���
    private RectTransform[] slots;
    private Image[] slotImages;
    private NetworkObject spawnedObjectParent;
    public List<InventoryItem> items = new List<InventoryItem>();  // ���� �κ��丮 ������ ����Ʈ
    public NetworkList<InventoryItemData> networkItems; // ��Ʈ��ũ ����ȭ �κ��丮 ������ ����Ʈ

    // �κ��丮 ���� Ű ���ε�
    public KeyCode[] slotKeys = new KeyCode[] {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5
    };
    private KeyCode interactKey;
    private KeyCode dropKey;
    private KeyCode useItemKey;

    private NetworkVariable<int> selectedSlot = new NetworkVariable<int>(0); // ���� ���õ� ����


    [SerializeField] private PlaceableItemManager currentPlaceableItemManager; // ��ġ ������ ������ �Ŵ���
    [SerializeField] private bool isPlacingItem = false; // ������ ��ġ ���� �÷���

    public bool IsPlacingItem { get { return isPlacingItem; } set { isPlacingItem = value; } }

    private InventoryItem currentSelectedItem = null; // ���� ���õ� ������
    bool invenLoading = false;

    #region �ʱ�ȭ    

    private void Awake()
	{
        networkItems = new NetworkList<InventoryItemData>();
    }

	void Start()
    {
        Transform inventoryTransform = GameObject.Find("Inventory").transform;
        int numberOfSlots = inventoryTransform.childCount;
        slots = new RectTransform[numberOfSlots];
        slotImages = new Image[numberOfSlots];
        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;

        // ���� �� ���� �̹����� �ʱ�ȭ
        for (int i = 0; i < numberOfSlots; i++)
        {
            Transform slotTransform = inventoryTransform.GetChild(i);
            slots[i] = slotTransform.GetComponent<RectTransform>();
            slotImages[i] = slotTransform.GetComponent<Image>();
        }

        if (!IsOwner)
		{
            GetComponent<Interacter>().enabled = false;
        }

		if (IsOwner)
        {      
            // �ʿ��� UI ��Ҹ� ã�� ���� �ڷ�ƾ ����
            StartCoroutine(InitializeUIElements());
            // Ű�ڵ� ���� �� ȣ�� �̺�Ʈ �߰�.
            KeySettingsManager.Instance.KeyCodeChanged += SetKeys;
            SetKeys();
        }



    }

    //�� ��ȯ�� �̺�Ʈ
    public void OnSceneEvent(SceneEvent sceneEvent)
	{
        if (IsOwner)
        {
            StartCoroutine(InitializeUIElements());
        }
    }

    private IEnumerator InitializeUIElements()
    {
        // PlaceableItemManager ������Ʈ ã��
        while (currentPlaceableItemManager == null)
        {
            GameObject placeableItemManagerObject = GameObject.Find("PlaceableItemManager");
            if (placeableItemManagerObject != null)
            {
                currentPlaceableItemManager = placeableItemManagerObject.GetComponent<PlaceableItemManager>();
            }
            yield return null;
        }

        // SpawnedObjects �θ� ������Ʈ ã��
        while (spawnedObjectParent == null)
        {
            GameObject spawnedObjectParentObject = GameObject.Find("MapSpawnedObjects");
            if (spawnedObjectParentObject != null)
            {
                spawnedObjectParent = spawnedObjectParentObject.GetComponent<NetworkObject>();
            }
            yield return null;
        }

        // ���� �κ��丮 �ʱ�ȭ
        InventoryServerRpc();
    }

    private void SetKeys()
	{
        try
        {
            interactKey = KeySettingsManager.Instance.GetKey("Interact");
            dropKey = KeySettingsManager.Instance.GetKey("Drop");
            useItemKey = KeySettingsManager.Instance.GetKey("UseItem");
            Debug.Log("KeySetting �� ã�ҽ��ϴ�.");
        }
        catch
        {
            Debug.Log("KeySetting �� ã�� ���߽��ϴ�...");
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && IsOwner)
        {
            selectedSlot.OnValueChanged += OnSlotChanged; // ���� ���� �̺�Ʈ ����
        }

        networkItems.OnListChanged += OnNetworkItemsChanged; // ��Ʈ��ũ ������ ����Ʈ ���� �̺�Ʈ ����
        InitializeSlotImages();
        UpdateSlotSelection();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    #endregion

    #region ��Ʈ��ũ ������ ó��

    private void OnNetworkItemsChanged(NetworkListEvent<InventoryItemData> changeEvent)
    {
        // ��Ʈ��ũ ������ ����Ʈ�� ����� �� ���� ������ ����Ʈ ������Ʈ
        items.Clear();
        foreach (var data in networkItems)
        {
            var item = ScriptableObject.CreateInstance<InventoryItem>();
            item.CopyDataFrom(data);
            items.Add(item);
        }

        if (IsOwner && IsClient)
		{
            InitializeSlotImages();
            invenLoading = false;
        }            
    }

    [ServerRpc(RequireOwnership = false)]
    private void InventoryServerRpc()
    {
        // ��Ʈ��ũ �������� �ʱ�ȭ���� ���� ��� �ʱ�ȭ
        if (networkItems == null)
        {
            networkItems = new NetworkList<InventoryItemData>();
            Debug.Log("networkItems �ʱ�ȭ��");
        }

        if (networkItems.Count == 0)
        {
            var test = items.Count;
            // �⺻ ���������� ��Ʈ��ũ ������ �ʱ�ȭ
            for (int i = 0; i < test; i++)
            {
                var defaultItem = new InventoryItemData(new FixedString128Bytes(), new FixedString128Bytes(), new FixedString128Bytes(), new FixedString128Bytes(), new FixedString128Bytes(), false , false, 0 ,0 ,0);
                Debug.Log("�⺻ �������� networkItems�� �߰�");
                networkItems.Add(defaultItem);
                Debug.Log("�⺻ �������� networkItems�� �߰���");
            }
        }
        Debug.Log($"networkItems.Count: {networkItems.Count}, items.Count: {items.Count}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ItemInputRequestServerRpc(int slotIndex, ulong itemNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkObjectId, out NetworkObject itemNetworkObject))
        {
            // ���ο� �������� �Ҵ��ϱ� ���� ������ ��� �ִ��� Ȯ��
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
                    Debug.Log("�� ������ ã�� �� ����");
                    return;
                }
            }

            PickupItem item = itemNetworkObject.GetComponent<PickupItem>();

            // ���ο� �κ��丮 ������ �����͸� �����ϰ� ��Ʈ��ũ ����Ʈ�� �Ҵ�
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

            // Ŭ���̾�Ʈ ������ ������ �ε�
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
                Debug.Log($"{newItem.itemSpritePath.ToString()}���� ��������Ʈ�� ���������� �ε��");
            }
            else
            {
                slotImages[slotIndex].sprite = null;
                slotImages[slotIndex].enabled = false;
                Debug.LogError($"{newItem.itemSpritePath.ToString()}���� ��������Ʈ �ε� ����");
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
            Debug.Log("Ŭ���̾�Ʈ ID�� ��ġ���� ����");
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
            Debug.Log("������ NetworkObject�� ã�� �� ����");
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
            currentPlaceableItemManager.PreviewDestroy();
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

    #region ������Ʈ �� �Է� ó��

    void Update()
    {
        if (IsClient && IsOwner)
        {
            HandleKeyboardInput();
            HandleMouseWheelInput();
            HandleInteract();
            HandleDropItem();
            HandleUseItem(); // ������ ��� ó�� �߰�
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
                    // ���� �̸����� ������Ʈ�� ���� ��� ����
                    currentPlaceableItemManager.PreviewDestroy();
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
                // ���� �̸����� ������Ʈ�� ���� ��� ����
                currentPlaceableItemManager.PreviewDestroy();
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
                // ���� �̸����� ������Ʈ�� ���� ��� ����
                currentPlaceableItemManager.PreviewDestroy();
                isPlacingItem = false;
                currentSelectedItem = null;
            }
            int newSlot = (selectedSlot.Value - 1 + slots.Length) % slots.Length;
            RequestSlotChangeServerRpc(newSlot);
        }
    }

    private void HandleUseItem()
    {
        if (invenLoading)
		{
            return;
        }
            
        InventoryItem currentItem = items[selectedSlot.Value];
        if (currentItem != null && currentItem.isPlaceable)
        {
            if (isPlacingItem)
            {
                // ���� �̸����� ������Ʈ�� ���� ��� ����
                isPlacingItem = false;
                currentSelectedItem = null;
            }

            // ���ο� �÷��̽� �������� ���
            currentSelectedItem = currentItem;

            if (currentPlaceableItemManager.previewObject == null)
            {
                // PlaceableItemManager �ʱ�ȭ
                currentPlaceableItemManager.previewPrefab = currentItem.PreviewPrefab;
                currentPlaceableItemManager.objectPrefab = currentItem.ObjectPrefab;
                currentPlaceableItemManager.InitializePreviewObject(currentItem.PreviewPrefab); // �̸����� ������Ʈ �ʱ�ȭ
            }
            if (isPlacingItem == false)
            {
                isPlacingItem = true; // ��ġ ��� Ȱ��ȭ
            }

        }
        else if (currentItem == null || !currentItem.isPlaceable)
        {
            currentPlaceableItemManager.PreviewDestroy(); // �̸����� �����ϱ�
            isPlacingItem = false; // ��ġ ��� ��Ȱ��ȭ
        }
    }

    private void HandleInteract()
    {
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
        if (Input.GetKeyDown(dropKey))
        {
            InventoryItem currentItem = items[selectedSlot.Value];
            if (currentItem != null)
            {
                if (isPlacingItem)
                {
                    // �̸����� ������Ʈ ����
                    currentPlaceableItemManager.PreviewDestroy();
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
                    // �ٴ��� �߰ߵ��� ������ �÷��̾� �տ� �α�
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
            GameObject itemPrefab = itemToDrop.DropPrefab; // ��θ� ���� ������ �ε�
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
                    itemToDrop.price, // ���⼭ ���ݸ� ����
                    itemToDrop.maxPrice,
                    itemToDrop.minPrice
                );

                temptItem.networkInventoryItemData.Value = updatedItemData;

                NetworkObject networkObject = droppedItem.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Spawn();
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


    #endregion

    #region �κ��丮 ������ ��� �� ����

    public void UseItem(InventoryItem item)
    {
        Debug.Log("Use item: " + item.ItemSprite.name);

        // ������ ��� �� �κ��丮���� ����
        RequestRemoveItemFromInventoryServerRpc(selectedSlot.Value);
    }

    public void UseCurrentSelectedItem(ulong objectID)
    {
        InventoryItem currentItem = items[selectedSlot.Value];
        if (currentItem != null)
        {
            invenLoading = true;
            // ������ ��� �� �κ��丮���� ����
            RequestItemDataUpdataServerRpc(selectedSlot.Value , objectID);
            // ��ġ ��� ��Ȱ��ȭ
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
                  currentItem.price, // ���⼭ ���ݸ� ����
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
        networkItems[slotIndex] = new InventoryItemData(); // �� �����͸� �Ҵ��Ͽ� �ʱ�ȭ
        RemoveItemFromInventoryClientRpc(slotIndex);
    }

    [ClientRpc]
    private void RemoveItemFromInventoryClientRpc(int slotIndex)
    {
        UpdateSlotSelection();
    }

    #endregion

    #region ���� ���� ������Ʈ

    private void UpdateSlotSelection()
    {
		if (slots == null)
		{
            Debug.Log("slots setting...");
            return;
		}
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
            // ���� ���� �� �̸����� ������Ʈ�� ���� ��� ����
            currentPlaceableItemManager.PreviewDestroy();
            isPlacingItem = false;
            currentSelectedItem = null;
        }
        UpdateSlotSelection();
    }

    #endregion
}
