using UnityEngine;
using Unity.Netcode;
using System.Collections;


[System.Serializable]
public class StatusEffectData
{
    public bool isPermanent;
    public float duration;
}


public class StatusEffect : NetworkBehaviour
{
    private NetworkVariable<bool> netIsSlowed = new NetworkVariable<bool>(value:false);

    [SerializeField] public StatusEffectData slowEffect;
    [SerializeField] private Player player;

    [SerializeField] private float normalMultiplier = 1f;
    [SerializeField] private float slowedMultiplier = 0.7f;

    public bool IsSlowed => netIsSlowed.Value;
    private void Start()
	{
		player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
            netIsSlowed.OnValueChanged += OnSlowDebuffChanged;
    }

    [ServerRpc]
    public void ApplySlowServerRpc(bool isPermanent = false, float duration = 0f)
    {
        if (netIsSlowed.Value) return;

		if (isPermanent)
		{
            slowEffect.isPermanent = isPermanent;
        }
		else
		{
            slowEffect.duration = duration;
		}

        netIsSlowed.Value = true;

        if (!slowEffect.isPermanent)
            StartCoroutine(RemoveSlowAfterSeconds(slowEffect.duration));
    }

    private IEnumerator RemoveSlowAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        slowEffect.isPermanent = false;

        netIsSlowed.Value = false;
    }

    private void OnSlowDebuffChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"슬로우 상태 {newValue}로 변경됨");

        player.SlowMultiplier = newValue ? slowedMultiplier : normalMultiplier;
    }

    [ServerRpc]
    public void RemoveSlowServerRpc()
    {
        netIsSlowed.Value = false;
    }
}
