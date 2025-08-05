using System.Collections;
using Unity.Netcode;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class EnemyRandomBox : InteractableObject
{
    [SerializeField]
    private string _hostileGameObjectTag;

    [SerializeField]
    private string _friendlyGameObjectTag;

	[SerializeField]
	private float _time;

	[SerializeField]
	private Vector3 _offset;

	[SerializeField]
	private string[] _itemPathTable;

    [SerializeField]
    private LayerMask _hostileLayerMask;

    [SerializeField]
	private LayerMask _friendlyLayerMask;

	[SerializeField]
	private AudioClip _insertCoinAudioClip;

	[SerializeField]
	private AudioClip _warningAudioClip;

	[SerializeField]
	private AudioSource _audioSource;

	private NetworkVariable<bool> _isEnemy = new();

	private void Awake()
	{
        tag = _friendlyGameObjectTag;
        gameObject.layer = _friendlyLayerMask;
	}

	public override bool Interact(ulong userId, Transform interactingObjectTransform)
	{
		var result = false;

		if (!_isEnemy.Value)
		{
			result = base.Interact(userId, interactingObjectTransform);

			_audioSource.PlayOneShot(_insertCoinAudioClip);

			InternalOnInteractServerRPC();
		}

		return result;
	}

	// �ڵ� �ܾ�� �� ����;;;
	[Rpc(SendTo.Server)]
	private void InternalOnInteractServerRPC()
	{
		var randomIndex = Random.Range(0, _itemPathTable.Length);

		//var path = _itemPath;
		var path = _itemPathTable[randomIndex];
		var spawnedObjectParent = GameObject.Find("SpawnedObjects").GetComponent<NetworkObject>();

		var loadObject = Resources.Load<GameObject>(path);

		var spawnPosition = Random.insideUnitSphere + _offset + transform.position;

		//���� ȸ����. (������Ʈ ������ �پ��ϰ� ���̷���)
		var randomX = Random.value > 0.5f ? 90f : 0f;
		var randomZ = Random.value > 0.5f ? 90f : 0f;
		var randomY = Random.Range(0f, 360f);

		var spawnRotation = new Vector3(randomX, randomY, randomZ);

		// ��� Ŭ���̾�Ʈ���� ������Ʈ�� ��ġ�ϴ� ClientRpc ȣ��
		var placedObject = Instantiate(loadObject, spawnPosition, Quaternion.Euler(spawnRotation));

		var networkObject = placedObject.GetComponent<NetworkObject>();
		var temptItem = placedObject.GetComponent<PickupItem>();

		var parentObject = NetworkManager.SpawnManager.SpawnedObjects[spawnedObjectParent.NetworkObjectId];

		var updatedItemData = new InventoryItemData(
			temptItem.inventoryItem.itemName,
			temptItem.inventoryItem.itemSpritePath,
			temptItem.inventoryItem.previewPrefabPath,
			temptItem.inventoryItem.objectPrefabPath,
			temptItem.inventoryItem.dropPrefabPath,
			temptItem.inventoryItem.isPlaceable,
			temptItem.inventoryItem.isUsable,
			0,
			temptItem.inventoryItem.maxPrice,
			temptItem.inventoryItem.minPrice,
			temptItem.inventoryItem.batteryLevel,
			temptItem.inventoryItem.batteryEfficiency
		);

		networkObject.Spawn();

		temptItem.networkInventoryItemData.Value = updatedItemData;

		networkObject.transform.SetParent(parentObject.transform, true);

		Debug.Log($"������ ���� �Ǿ����!");
	}

	[Rpc(SendTo.Server)]
	public void ChangeEntityIdentityServerRPC(bool isEnemy)
	{
		_isEnemy.Value = isEnemy;

		if (_isEnemy.Value)
		{
			tag = _hostileGameObjectTag;
			gameObject.layer = _hostileLayerMask;

			ChangeEntityIdentityClientRPC(_isEnemy.Value);

			StartCoroutine(ReturnInteractableObjectTimer());
		}
		else
		{
			tag = _friendlyGameObjectTag;
			gameObject.layer = _friendlyLayerMask;
		}
	}

	[Rpc(SendTo.Everyone)]
	public void ChangeEntityIdentityClientRPC(bool isEnemy)
	{
		StartCoroutine(OnChangeEntityIdentity());
	}

	private IEnumerator ReturnInteractableObjectTimer()
	{
		yield return new WaitForSeconds(_time);

		ChangeEntityIdentityClientRPC(false);

		yield return null;
	}

	private IEnumerator OnChangeEntityIdentity()
	{
		var waits = new WaitForSeconds(1.0F);

		while (_isEnemy.Value)
		{
			_audioSource.PlayOneShot(_warningAudioClip);

			yield return waits;
		}
	}
}
