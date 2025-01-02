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

	#region 저장해야할 정보
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

	public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
	public NetworkVariable<int> experience = new NetworkVariable<int>();
	public NetworkVariable<int> level = new NetworkVariable<int>();

	public GameObject playerLight;
	private bool lightState = true;

	[SerializeField] private SaveSystem saveSystem;
	[SerializeField] private AudioSource audioSource;

	public  void Start()
	{
		if (IsOwner)
		{
			StartCoroutine(InitSaveSystem());
			playerName.OnValueChanged += (oldData, newdata) => saveSystem.SavePlayerData(this);
			experience.OnValueChanged += (oldData, newdata) => saveSystem.SavePlayerData(this);
			level.OnValueChanged += (oldData, newdata) => saveSystem.SavePlayerData(this);
		}
		//AudioManager.Instance.SetbuttonSorce(audioSource);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeySettingsManager.Instance.LightKey))
		{
			LightOnOff();
		}

		if (lightState)
		{

		}
	}

	private IEnumerator InitSaveSystem()
	{
		// PlaceableItemManager 오브젝트 찾기
		while (saveSystem == null)
		{			
			if(saveSystem = FindAnyObjectByType<SaveSystem>())
			{
				if (IsOwner)
					LoadPlayerDataServerRPC();
			}
			yield return null;
		}
	}

	public void Die()
	{
	}

	public void TakeDamage(int amount)
	{
	}

	private void LightOnOff()
	{
		playerLight.SetActive(!playerLight.activeInHierarchy);
		lightState = playerLight.activeInHierarchy;
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
struct PlayerData
{
	public string playerName;
	public int experience;
	public int level;
}
