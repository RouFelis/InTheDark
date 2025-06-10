using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private TMP_Text areaTMP;
    [SerializeField] private TMP_Text questQuotaTMP;
    [SerializeField] private TMP_Text moneyQuotaTMP;


    void Start()
    {
        SharedData.Instance.area.OnValueChanged += SetAreaTMP;
        SharedData.Instance.questQuota.OnValueChanged += SetquestQuotaTMP;
        SharedData.Instance.moneyQuota.OnValueChanged += SetmoneyQuotaTMP;
    }

    private void SetAreaTMP(int oldValue, int newValue)
	{
        areaTMP.text = newValue.ToString();
	}
    private void SetquestQuotaTMP(int oldValue, int newValue)
    {
        questQuotaTMP.text = newValue.ToString();
    }
    private void SetmoneyQuotaTMP(int oldValue, int newValue)
    {
        moneyQuotaTMP.text = newValue.ToString();
    }

    [ServerRpc]
    public void GameClearServerRPC()
	{
        SharedData.Instance.area.Value += 1;
        SharedData.Instance.questQuota.Value = 0;
        SharedData.Instance.moneyQuota.Value = 0;
    }



}
