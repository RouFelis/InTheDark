using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

public class PlayerNetworkData : MonoBehaviour
{
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Experience = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Level = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<float> Health = new NetworkVariable<float>(100f, writePerm: NetworkVariableWritePermission.Server);

    public bool IsDead => Health.Value <= 0f;

    private Player ownerPlayer;

    public void Initialize(Player player)
    {
        ownerPlayer = player;
        PlayerName.OnValueChanged += (oldVal, newVal) => player?.NotifyDataChanged();
        Experience.OnValueChanged += (oldVal, newVal) => player?.NotifyDataChanged();
        Level.OnValueChanged += (oldVal, newVal) => player?.NotifyDataChanged();
        Health.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(float oldVal, float newVal)
    {
        Debug.Log($"Health changed: {oldVal} -> {newVal}");
        if (newVal <= 0f) ownerPlayer?.Die();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetNameServerRpc(string name) => PlayerName.Value = name;

    [ServerRpc]
    public void TakeDamageServerRpc(float amount)
    {
        if (Health.Value <= 0f) return;
        Health.Value = Mathf.Max(0f, Health.Value - amount);
    }

    [ServerRpc]
    public void SetHealthServerRpc(float value)
    {
        Health.Value = Mathf.Clamp(value, 0f, float.MaxValue);
    }
}
