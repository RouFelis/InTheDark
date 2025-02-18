using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class WeaponSystem : NetworkBehaviour
{
    [Header("Weapon Data")]
    public WeaponData baseWeaponData; // ScriptableObject (������ �⺻��)
    public WeaponInstance weaponInstance; // �÷��̾ ���� ���� �ν��Ͻ�
 

    [Header("Weapon Net Data")]
    public NetworkVariable<float> currentLevel = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> baseDamage = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> zoomDamage = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> batteryCapacity = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    [Header("needDatas")]
    protected SaveSystem saveSystem;
    protected Player player;


    public delegate void OnWeaponUpgraded(float _baseDamage, float _zoomDamage);
    public event OnWeaponUpgraded WeaponUpgraded;


	public virtual void Start()
    {
        if (baseWeaponData == null)
        {
            Debug.LogError("Weapon Data�� ������� �ʾҽ��ϴ�!");
            return;
        }
        if (IsOwner)
        {
            StartCoroutine(Initialize());
        }

    }

    private IEnumerator Initialize()
    {
        // PlaceableItemManager ������Ʈ ã��
        while (saveSystem == null)
        {
            saveSystem = FindAnyObjectByType<SaveSystem>();
            Debug.Log("saveSystem serch....");
            yield return null;
        }
        player = GetComponent<Player>();

        baseDamage.OnValueChanged += (oldData, newdata) => BaseDamageValueChaged();
        zoomDamage.OnValueChanged += (oldData, newdata) => ZoomDamageValueChaged();
        batteryCapacity.OnValueChanged += (oldData, newdata) => BatteryCapacityValueChaged();

        initWeaponInstance(saveSystem.LoadWeaponData(player.playerName.Value.ToString()));
        Debug.Log("WeaponSystem Init complete");
    }

    private void initWeaponInstance(WeaponInstance weaponInstance)
	{
        this.weaponInstance = weaponInstance;
        baseDamage.Value = weaponInstance.baseDamage;
        zoomDamage.Value = weaponInstance.zoomDamage;
        batteryCapacity.Value = weaponInstance.batteryCapacity;
        currentLevel.Value = weaponInstance.level;
        Debug.Log("initWeaponInstance....");
    }

    private void BaseDamageValueChaged()
	{
        weaponInstance.baseDamage = baseDamage.Value;
        saveSystem.SaveWeaponData(weaponInstance , player.playerName.Value.ToString());
    }
    private void ZoomDamageValueChaged()
    {
        weaponInstance.baseDamage = baseDamage.Value;
        saveSystem.SaveWeaponData(weaponInstance, player.playerName.Value.ToString());
    }
    private void BatteryCapacityValueChaged()
    {
        weaponInstance.baseDamage = baseDamage.Value;
        saveSystem.SaveWeaponData(weaponInstance, player.playerName.Value.ToString());
    }


    public void UpgradeWeapon()
    {
        float cost = weaponInstance.upgradeCost * (1 + (currentLevel.Value * 0.1f));

        Debug.Log("weaponInstance.upgradeCost : " + weaponInstance.upgradeCost);
        Debug.Log("cost : " + cost);
        Debug.Log("currentLevel.Value : " + currentLevel.Value);
        Debug.Log("test : " + (1 + (currentLevel.Value * 0.1f)));

        if (SharedData.Instance.Money.Value < cost)
        {
            Debug.Log("��ȭ�� �ʿ��� ��ȭ�� �����մϴ�.");
            return;
        }

        SharedData.Instance.Money.Value -= (int)cost; // ��� ����

        weaponInstance.level++;
        currentLevel.Value = weaponInstance.level;
        baseDamage.Value *= weaponInstance.baseUpgarde;
        zoomDamage.Value *= weaponInstance.zoomUpgarde;

        // ��ȭ�� ���� ����
        WeaponUpgraded?.Invoke(baseDamage.Value, zoomDamage.Value);
        Debug.Log($"���Ⱑ {currentLevel.Value} ������ ��ȭ�Ǿ����ϴ�!");

    }

}
