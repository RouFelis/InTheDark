using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerLifeCycle : MonoBehaviour
{
    public event Action OnDieLocal;
    public event Action OnDieEffects;
    public event Action OnReviveLocal;
    public static event Action OnDie;

    private PlayerNetworkData networkData;
    private PlayerDamageHandler damageHandler;
    private PlayerUIHandler uiHandler;
    private Player ownerPlayer;

    public void Initialize(Player player, PlayerNetworkData networkData, PlayerDamageHandler damageHandler, PlayerUIHandler uiHandler)
    {
        this.ownerPlayer = player;
        this.networkData = networkData;
        this.damageHandler = damageHandler;
        this.uiHandler = uiHandler;

        // bind to health changes already handled in networkData.Initialize
    }

    public void BindLocalEvents(PlayerMicController micController)
    {
        if (micController == null) return;
        OnDieEffects += ownerPlayer.DieEffect;
        OnDieEffects += micController.Die;
        OnReviveLocal += micController.Revive;
    }

    public void UnbindEvents()
    {
        OnDieEffects -= ownerPlayer.DieEffect;
    }

    public void LocalReviveStart()
    {
        SetDieScripts(true);
        SetPlayerDieView(false);
    }

    private void SetDieScripts(bool value)
    {
        foreach (var mb in ownerPlayer.dieEnableMonoBehaviorScripts) mb.enabled = value;
        foreach (var nb in ownerPlayer.dieEnableNetworkBehaviorScripts) nb.enabled = value;

        ownerPlayer.characterController.enabled = value;
        ownerPlayer.bodyCollider.enabled = value;
    }

    private void SetPlayerDieView(bool value)
    {
        ownerPlayer.firstPersonObject.SetActive(value);
        ownerPlayer.thirdPersonObject.SetActive(!value);
        
        ownerPlayer.camTarget.gameObject.SetActive(value);
        ownerPlayer.spotlightControl.firstPersonWeaponLight.gameObject.SetActive(value);
        ownerPlayer.spotlightControl.thirdPersonWeaponLight.gameObject.SetActive(!value);

        if (value)
        {
            SetLayers(ownerPlayer.thirdPersonObject, 12);
            SetLayers(ownerPlayer.firstPersonObject, 11);
        }
        else
        {
            SetLayers(ownerPlayer.firstPersonObject, 12);
            SetLayers(ownerPlayer.thirdPersonObject, 11);
        }
    }

    private static void SetLayers(GameObject target, int layer)
    {
        if (target == null) return;
        target.layer = layer;
        foreach (Transform child in target.transform) SetLayers(child.gameObject, layer);
    }
}
