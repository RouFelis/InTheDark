using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;



public class NetworkInventoryController : NetworkBehaviour
{
    private RectTransform[] slots;
    private Image[] slotImages;
    public List<InventoryItem> items = new List<InventoryItem>();  // ������ ����Ʈ
    public NetworkList<InventoryItemData> networkItems = new NetworkList<InventoryItemData>(); // ��Ʈ��ũ ����ȭ ����Ʈ

    public KeyCode[] slotKeys = new KeyCode[] {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5
    };
    public TextMeshProUGUI pickupText; // TextMeshPro ��ü �߰�

    private NetworkVariable<int> selectedSlot = new NetworkVariable<int>(0);

    // Key customization variables
    public KeySettingsManager keySettingsManager; // Ű ���� �Ŵ���
    [SerializeField] private PlaceableItemManager currentPlaceableItemManager;
    [SerializeField] private bool isPlacingItem = false;

    public bool IsPlacingItem { get { return isPlacingItem; } set { isPlacingItem = value; } }

    private InventoryItem currentSelectedItem = null;

    void Start()
    {
        Transform inventoryTransform = GameObject.Find("Inventory").transform;
        int numberOfSlots = inventoryTransform.childCount;
        slots = new RectTransform[numberOfSlots];
        slotImages = new Image[numberOfSlots];

        for (int i = 0; i < numberOfSlots; i++)
        {
            Transform slotTransform = inventoryTransform.GetChild(i);
            slots[i] = slotTransform.GetComponent<RectTransform>();
            slotImages[i] = slotTransform.GetComponent<Image>();
        }     

        // �ڷ�ƾ�� ���� �ʿ��� ������Ʈ���� ã��
        StartCoroutine(InitializeUIElements());
    }
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

        if (IsOwner&& IsClient)
            InitializeSlotImages();
    }

    private IEnumerator InitializeUIElements()
    {
        // �ʿ��� ������Ʈ���� ã��
        while (pickupText == null)
        {
            GameObject pickupTextObject = GameObject.Find("PickupText");
            if (pickupTextObject != null)
            {
                pickupText = pickupTextObject.GetComponent<TextMeshProUGUI>();
            }
            yield return null;
        }

        while (keySettingsManager == null)
        {
            GameObject keySettingsManagerObject = GameObject.Find("KeySettingsManager");
            if (keySettingsManagerObject != null)
            {
                keySettingsManager = keySettingsManagerObject.GetComponent<KeySettingsManager>();
            }
            yield return null;
        }

        while (currentPlaceableItemManager == null)
        {
            GameObject placeableItemManagerObject = GameObject.Find("PlaceableItemManager");
            if (placeableItemManagerObject != null)
            {
                currentPlaceableItemManager = placeableItemManagerObject.GetComponent<PlaceableItemManager>();
            }
            yield return null;
        }

        InventoryServerRpc();

        pickupText.gameObject.SetActive(false); // �ʱ� ���´� ��Ȱ��ȭ
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && IsOwner)
        {
            selectedSlot.OnValueChanged += OnSlotChanged;           
        }

        networkItems.OnListChanged += OnNetworkItemsChanged; // ����Ʈ ���� �� �̺�Ʈ �ڵ鷯 �߰�
        InitializeSlotImages();
        UpdateSlotSelection();
    }

    void Update()
    {    
        if (IsClient && IsOwner)
        {
            HandleKeyboardInput();
            HandleMouseWheelInput();
            HandleInteract();
            HandleDropItem();
            HandleRaycast();
            HandleUseItem(); // �߰�: HandleUseItem �޼��� ȣ��
        }
        if (isPlacingItem)
        {
            currentPlaceableItemManager.UpdatePreviewObject();
            currentPlaceableItemManager.HandleRotation(ref isPlacingItem);
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
                // ���� �̸����� ������Ʈ�� ���� ��� ����
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
                // ���� �̸����� ������Ʈ�� ���� ��� ����
                currentPlaceableItemManager.CancelPreview();
                isPlacingItem = false;
                currentSelectedItem = null;
            }
            int newSlot = (selectedSlot.Value - 1 + slots.Length) % slots.Length;
            RequestSlotChangeServerRpc(newSlot);
        }
    }


    [ServerRpc]
    private void RequestSlotChangeServerRpc(int newSlot)
    {
        selectedSlot.Value = newSlot;
        // Ŭ���̾�Ʈ �������� ���� ������ ó���� �� �ֵ��� ������Ʈ�մϴ�.
        UpdateSlotSelectionClientRpc(newSlot);
    }

    [ClientRpc]
    private void UpdateSlotSelectionClientRpc(int newSlot)
    {
        if (isPlacingItem)
        {
            // ���� ���� �� �̸����� ������Ʈ�� ���� ��� ����
            currentPlaceableItemManager.CancelPreview();
            isPlacingItem = false;
            currentSelectedItem = null;
        }   
        // Ŭ���̾�Ʈ���� ���� NetworkVariable�� �������� �ʵ��� ����
        if (!IsOwner)
        {
            selectedSlot.Value = newSlot;
        }
        UpdateSlotSelection();
    }

    private void HandleUseItem()
    {
        InventoryItem currentItem = items[selectedSlot.Value];
        if (currentItem != null && currentItem.isPlaceable)
        {
            if (isPlacingItem)
            {
                // ���� �̸����� ������Ʈ�� ���� ��� ����
                currentPlaceableItemManager.CancelPreview();
                isPlacingItem = false;
                currentSelectedItem = null;
            }

            // ���ο� �÷��̽� �������� ���
            currentSelectedItem = currentItem;

            // PlaceableItemManager �ʱ�ȭ
            currentPlaceableItemManager.previewPrefab = currentItem.PreviewPrefab;
            currentPlaceableItemManager.objectPrefab = currentItem.ObjectPrefab;
            currentPlaceableItemManager.InitializePreviewObject(currentItem.PreviewPrefab); // �̸����� ������Ʈ �ʱ�ȭ
            if (isPlacingItem == false)
            {
                isPlacingItem = true; // ��ġ ��� Ȱ��ȭ
            }

        }
        else if (currentItem == null || !currentItem.isPlaceable)
        {
            currentPlaceableItemManager.PreviewDestroy();//�̸����� �����ϱ�.
            isPlacingItem = false; // ��ġ ��� ��Ȱ��ȭ            
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
                    // �̸����� ������Ʈ ����
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
                NetworkObject networkObject = droppedItem.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Spawn();
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
                    pickupText.text = $"{item.inventoryItem.itemName} ���({interactKey})";
                    pickupText.gameObject.SetActive(true);
                    return;
                }
            }
        }
        pickupText.gameObject.SetActive(false);
    }

    [ServerRpc]
    private void RequestPickupItemServerRpc(ulong itemNetworkObjectId, ulong clientId, int slotIndex)
    {
        PickupItemClientRpc(itemNetworkObjectId, clientId, slotIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InventoryServerRpc() 
    {

        // Ensure networkItems is initialized
        if (networkItems == null)
        {
            networkItems = new NetworkList<InventoryItemData>();
            Debug.Log("networkItems initialized");
        }

        if (networkItems.Count == 0)
        {
            var test = items.Count;
            // Initialize networkItems with default items
            for (int i = 0; i < test; i++)
            {
                var defaultItem = new InventoryItemData(new FixedString128Bytes(), new FixedString128Bytes(), new FixedString128Bytes(), new FixedString128Bytes(), new FixedString128Bytes(), false);
                Debug.Log("Adding default item to networkItems");
                networkItems.Add(defaultItem);
                Debug.Log("Default item added to networkItems");
            }
        }
        Debug.Log($"networkItems.Count: {networkItems.Count}, items.Count: {items.Count}");
    }


    [ServerRpc(RequireOwnership =false)]
    private void ItemInputRequestServerRpc(int slotIndex , ulong itemNetworkObjectId)
	{
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkObjectId, out NetworkObject itemNetworkObject))
		{           
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
                    Debug.Log("No empty slot found");
                    return;
                }
            }

            PickupItem item = itemNetworkObject.GetComponent<PickupItem>();

            InventoryItemData newItem = new InventoryItemData(                
                new FixedString128Bytes(item.inventoryItem.itemName.ToString()),
                new FixedString128Bytes(item.inventoryItem.itemSpritePath.ToString()),
                new FixedString128Bytes(item.inventoryItem.previewPrefabPath.ToString()),
                new FixedString128Bytes(item.inventoryItem.objectPrefabPath.ToString()),
                new FixedString128Bytes(item.inventoryItem.dropPrefabPath.ToString()),
                item.inventoryItem.isPlaceable
            );

            networkItems[slotIndex] = newItem;

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
                Debug.Log($"Sprite loaded successfully from {newItem.itemSpritePath.ToString()}");
            }
            else
            {
                slotImages[slotIndex].sprite = null;
                slotImages[slotIndex].enabled = false;
                Debug.LogError($"Failed to load sprite from {newItem.itemSpritePath.ToString()}");
            }
        }
    }

    [ClientRpc]
    private void PickupItemClientRpc(ulong itemNetworkObjectId, ulong clientId, int slotIndex)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId)
        {
            Debug.Log("Client ID does not match");
            return;
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkObjectId, out NetworkObject itemNetworkObject))
        {
            ItemInputRequestServerRpc(slotIndex , itemNetworkObjectId);           
            RequestDespawnItemServerRpc(itemNetworkObjectId);
            UpdateSlotSelection();
        }
        else
        {
            Debug.Log("Item NetworkObject not found");
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


    public void UseItem(InventoryItem item)
    {
        Debug.Log("Use item: " + item.ItemSprite.name);

        // ������ ��� �� �κ��丮���� ����
        RequestRemoveItemFromInventoryServerRpc(selectedSlot.Value);
    }

    public void UseCurrentSelectedItem()
    {
        InventoryItem currentItem = items[selectedSlot.Value];
        if (currentItem != null)
        {
            Debug.Log("Use current selected item: " + currentItem.ItemSprite.name);

            // ������ ��� �� �κ��丮���� ����
            RequestRemoveItemFromInventoryServerRpc(selectedSlot.Value);

            // ��ġ ��� ��Ȱ��ȭ
            isPlacingItem = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRemoveItemFromInventoryServerRpc(int slotIndex)
    {
        networkItems[slotIndex] = new InventoryItemData(); // �� �����͸� �Ҵ��Ͽ� �ʱ�ȭ
        RemoveItemFromInventoryClientRpc(slotIndex);
    }

    [ClientRpc]
    private void RemoveItemFromInventoryClientRpc(int slotIndex)
    {        
        UpdateSlotSelection();
    }

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

    private void OnSlotChanged(int oldSlot, int newSlot)
    {
        if (isPlacingItem)
        {
            // ���� ���� �� �̸����� ������Ʈ�� ���� ��� ����
            currentPlaceableItemManager.CancelPreview();
            isPlacingItem = false;
            currentSelectedItem = null;
        }
        UpdateSlotSelection();
    }
}
