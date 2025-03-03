using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.IO;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class Player : playerMoveController , IHealth , ICharacter
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
	

	[Header("PlayerState")]
	public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
	public NetworkVariable<int> experience = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
	public NetworkVariable<int> level = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
	[SerializeField] private float maxHealth = 100;
	public NetworkVariable<float> currentHealth = new NetworkVariable<float>(value:100, writePerm: NetworkVariableWritePermission.Server);

	public float Health => currentHealth.Value; // 체력 값은 외부에서 수정 불가
	public bool IsDead => currentHealth.Value <= 0;


	public delegate void DieEventHandler();
	public static event DieEventHandler OnDie;


	[SerializeField] private SaveSystem saveSystem;
	[SerializeField] private AudioSource audioSource;

	[Header("Player Layer Object")]
	[SerializeField] private GameObject firstPersonObject;
	[SerializeField] private GameObject thirdPersonObject;

	[Header("DieTarget")]
	[SerializeField] private GameObject DieTargetGameObject;
	public Image healthBar;

	private SpotlightControl spotlightControl;

	//Hit Volume
	private Volume volume;
	private Vignette vignette;

	private float defaultVignette = 0f;
	private Vector3 originalCameraPosition;

	private Coroutine hitEffectCoroutine; // 실행 중인 코루틴 저장
	private float mag = 0.00015f;
	private float dur = 0.3f;




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

	public override void LateUpdate()
	{
		if(!IsDead)
			base.LateUpdate();
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

		volume = GameObject.Find("Sky and Fog Global Volume").GetComponent<Volume>();


		if (volume.profile.TryGet(out vignette))
		{
			defaultVignette = vignette.intensity.value;
		}

		spotlightControl = GetComponent<SpotlightControl>();
		originalCameraPosition = playerCamera.transform.localPosition;
		KeySettingsManager.Instance.localPlayer = this;
		playerName.OnValueChanged += (oldData, newdata) => saveSystem.SavePlayerData(this);
		experience.OnValueChanged += (oldData, newdata) => saveSystem.SavePlayerData(this);
		level.OnValueChanged += (oldData, newdata) => saveSystem.SavePlayerData(this);
	}

	// 특정 오브젝트와 자식 오브젝트의 레이어를 변경하는 함수
	public void ChangeLayer(GameObject parentObject, int newLayer)
	{
		// 부모 오브젝트의 레이어를 변경
		parentObject.layer = newLayer;

		// 모든 자식 오브젝트의 레이어를 변경
		foreach (Transform child in parentObject.transform)
		{
			ChangeLayer(child.gameObject, newLayer);
		}
	}

	public void TakeDamage(float amount , AudioClip hitSound)
	{
		if (IsDead) return;

		currentHealth.Value -= amount;
		Debug.Log($"플레이어 체력: {currentHealth.Value}");

		StartCoroutine(CameraShake(dur, mag)); // 흔들림 효과
		if (hitEffectCoroutine != null)
		{
			StopCoroutine(hitEffectCoroutine); // 코루틴 실행중인거 종료
		}

		hitEffectCoroutine = StartCoroutine(HitEffectCoroutine()); // 화면 벌겋게

		if (hitSound != null)
		{
			audioSource.PlayOneShot(hitSound);  // 몬스터의 타격음 재생
		}

		UpdateHealthBar();

		if (IsDead)
		{
			Die();
		}
	}
	private void UpdateHealthBar()
	{
		if (healthBar == null)
		{
			healthBar = GameObject.Find("HealthBar").GetComponent<Image>();
		}
		float healthRatio = currentHealth.Value / maxHealth; // 0 ~ 1
		healthBar.fillAmount = healthRatio * 0.5f; // 0 ~ 0.5로 변환
	}

	private IEnumerator HitEffectCoroutine()
	{
		float duration_first = 0.1f; // 변화 속도
		float duration_second = 1f; // 변화 속도
		float holdTime = 2f; // 유지 시간
		float targetVignette = 0.6f; // 피격 시 비네팅 증가

		float elapsed = 0f;

		// 피격 순간: 화면 밝아지고 비네팅 증가
		while (elapsed < duration_first)
		{
			elapsed += Time.deltaTime;
			vignette.intensity.value = Mathf.Lerp(defaultVignette, targetVignette, elapsed / duration_first);
			yield return null;
		}

		// 잠시 유지
		yield return new WaitForSeconds(holdTime);

		elapsed = 0f;

		// 원래대로 복귀
		while (elapsed < duration_second)
		{
			elapsed += Time.deltaTime;		
			vignette.intensity.value = Mathf.Lerp(targetVignette, defaultVignette, elapsed / duration_second);
			yield return null;
		}

		// 값 원래대로 설정
		vignette.intensity.value = defaultVignette;
	}

	private IEnumerator CameraShake(float duration, float magnitude)
	{
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
			float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
			float z = UnityEngine.Random.Range(-1f, 1f) * magnitude;

			playerCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, z);
			yield return null;
		}

		playerCamera.transform.localPosition = originalCameraPosition; // 원래 위치로 복귀
	}

	public void Die()
	{
		Debug.Log("플레이어 사망!");
		// 기존 애니메이션 정지
		if (animator != null)
		{
			animator.enabled = false;
		}

		// Ragdoll 본들의 Rigidbody 활성화
		Rigidbody[] ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rb in ragdollRigidbodies)
		{
			rb.isKinematic = false;
			rb.useGravity = true;
		}

		firstPersonObject.gameObject.SetActive(false);
		ChangeLayer(thirdPersonObject, 11);
		SetAimMode(true, DieTargetGameObject);
		spotlightControl.ToogleLight();
		OnDie();
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
	public WeaponInstance weaponInstance; // 플레이어의 무기 데이터
}
