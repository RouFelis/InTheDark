using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float level = 1;

    public float baseDamage = 1;
    public float zoomDamage = 8;

    public float baseUpgarde = 1.1f;
    public float zoomUpgarde = 1.1f;

    public float batteryCapacity = 3f;
    public int upgradeCost = 100; // 강화 비용
}
