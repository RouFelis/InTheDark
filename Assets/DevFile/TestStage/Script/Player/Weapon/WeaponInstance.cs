[System.Serializable]
public class WeaponInstance
{
    public string weaponName;
    public float level;
    public float baseDamage;
    public float baseUpgarde;
    public float zoomDamage;
    public float zoomUpgarde;
    public float batteryCapacity;
    public float upgradeCost;

    public WeaponInstance(WeaponData baseData)
    {
        // ScriptableObject의 기본 데이터를 복사하여 인스턴스 초기화
        weaponName = baseData.weaponName;
        level = baseData.level;
        baseDamage = baseData.baseDamage;
        baseUpgarde = baseData.baseUpgarde;
        zoomDamage = baseData.zoomDamage;
        zoomUpgarde = baseData.zoomUpgarde;
        upgradeCost = baseData.upgradeCost;
        batteryCapacity = baseData.batteryCapacity;
    }
}
