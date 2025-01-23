using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.IO;

public class Player : playerMoveController , IDamaged, ICharacter
{
	public event Action OnDataChanged;

	#region �����ؾ��� ����
	public string Name 
	{ 
		get => playerName.Value.ToString(); set
		{
			if (playerName.Value != value)
			{
				playerName.Value = value;
				OnDataChanged?.Invoke();
			}
		}
	}
	public int Level
	{
		get => experience.Value; set
		{
			if (experience.Value != value)
			{
				experience.Value = value;
				OnDataChanged?.Invoke();
			}
		}
	}
	public int Experience
	{
		get => level.Value; set
		{
			if (level.Value != value)
			{
				level.Value = value;
				OnDataChanged?.Invoke();
			}
		}
	}
	#endregion


	public int Health { get; set; }
	public int Damage { get; set; }


	[Header("PlayerState")]
	public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
	public NetworkVariable<int> experience = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
	public NetworkVariable<int> level = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

	[SerializeField] private SaveSystem saveSystem;
	[SerializeField] private AudioSource audioSource;

	[Header("Player Layer Object")]
	[SerializeField] private GameObject firstPersonObject;
	[SerializeField] private GameObject thirdPersonObject;


	public override void Start()
	{
		base.Start();
		if (IsOwner)
		{
			StartCoroutine(InitSaveSystem());
			ChangeLayer(firstPersonObject , 11);
			ChangeLayer(thirdPersonObject, 12);
		}
		else
		{
			ChangeLayer(firstPersonObject, 12);
			ChangeLayer(thirdPersonObject , 11);
		}
		//AudioManager.Instance.SetbuttonSorce(audioSource);
	}

	private void Update()
	{

	}

	private IEnumerator InitSaveSystem()
	{
		// PlaceableItemManager ������Ʈ ã��
		while (saveSystem == null)
		{			
			if(saveSystem = FindAnyObjectByType<SaveSystem>())
			{
				if (IsOwner)
					LoadPlayerDataServerRPC();
			}
			yield return null;
		}

		playerName.OnValueChanged += (oldData, newdata) => saveSystem.SavePlayerData(this);
		experience.OnValueChanged += (oldData, newdata) => saveSystem.SavePlayerData(this);
		level.OnValueChanged += (oldData, newdata) => saveSystem.SavePlayerData(this);
	}


	// Ư�� ������Ʈ�� �ڽ� ������Ʈ�� ���̾ �����ϴ� �Լ�
	public void ChangeLayer(GameObject parentObject, int newLayer)
	{
		// �θ� ������Ʈ�� ���̾ ����
		parentObject.layer = newLayer;

		// ��� �ڽ� ������Ʈ�� ���̾ ����
		foreach (Transform child in parentObject.transform)
		{
			ChangeLayer(child.gameObject, newLayer);
		}
	}


	public void Die()
	{
	}

	public void TakeDamage(int amount)
	{
	}

	[ServerRpc(RequireOwnership =false)]
	public void LoadPlayerDataServerRPC()
	{
		string path = Application.persistentDataPath + "/playerdata.json";
		if (File.Exists(path))
		{
			string json = File.ReadAllText(path);

			if (saveSystem.useEncryption)
			{
				json = saveSystem.EncryptDecrypt(json);
			}
			PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);

			this.playerName.Value = playerData.playerName;
			this.experience.Value = playerData.experience;
			this.level.Value = playerData.level;
		}
	}

}

[System.Serializable]
public struct PlayerData
{
	public string playerName;
	public int experience;
	public int level;
	public WeaponInstance weaponInstance; // �÷��̾��� ���� ������
}
