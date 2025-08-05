using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class StoreUI : MonoBehaviour
{
    [System.Serializable]
    public struct UIStruct
    {
        public TMP_Text tmp;
        public string tmpContents;
        public string table;
    }

    [System.Serializable]
    public struct AppStruct
    {
        public Button appButton;
        public GameObject appWindow;
        public UIStruct appUI;
    }

    public LocalizedString localizedString;

    [SerializeField] private List<UIStruct> staticUIList; //���� UI.
    [SerializeField] private List<AppStruct> appList;
    [SerializeField] private UIStruct token;
    [SerializeField] private UIStruct rareToken;
    [SerializeField] private Button powerButton;
    [SerializeField] private ShopInteracter shopInteracter;


    void Start()
    {
        StaticTMPSet();
        TokenTMPSet();
        AppButtonSet();
        AppTMPSet();
        SharedData.Instance.Money.OnValueChanged += TokenTMPSet;
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

        powerButton.onClick.AddListener(PowerOff);
    }

    private void OnLanguageChanged(Locale newLocale)
    {
        Debug.Log("�� ����Ǿ����ϴ�.");
        StaticTMPSet();
        TokenTMPSet();
        AppTMPSet();
    }

    private void OnDestroy()
	{
        SharedData.Instance.Money.OnValueChanged -= TokenTMPSet;
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

	public void initObject()
    {
        // App �ؽ�Ʈ ���� �� ���� ����
        foreach (var app in appList)
        {
            app.appWindow.SetActive(true);
        }
    }

    void StaticTMPSet()
	{
        // ���� UI ��ҿ� �ؽ�Ʈ ����
        foreach (var ui in staticUIList)
        {
            localizedString.TableReference = ui.table; // ����ϰ��� �ϴ� ���̺�
            localizedString.TableEntryReference = ui.tmpContents; // ������ �̸�

            ui.tmp.text = localizedString.GetLocalizedString();
        }
    }

    void TokenTMPSet(int olddata = 0, int newdata = 0)
	{
        localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
        localizedString.TableEntryReference = token.tmpContents; // ������ �̸�

        token.tmp.text = localizedString.GetLocalizedString() + ":" + SharedData.Instance.Money.Value;

        localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
        localizedString.TableEntryReference = rareToken.tmpContents; // ������ �̸�

        rareToken.tmp.text = localizedString.GetLocalizedString() + ":" + SharedData.Instance.Money.Value;
    }

    void AppButtonSet()
	{
        // App �ؽ�Ʈ ���� �� ���� ����
        foreach (var app in appList)
        {
            app.appButton.onClick.AddListener(() => { app.appWindow.SetActive(true); });
        }
    }

    void AppTMPSet()
	{
        // App �ؽ�Ʈ ���� �� ���� ����
        foreach (var app in appList)
        {
            localizedString.TableReference = app.appUI.table; // ����ϰ��� �ϴ� ���̺�
            localizedString.TableEntryReference = app.appUI.tmpContents; // ������ �̸�

            app.appUI.tmp.text = localizedString.GetLocalizedString();
        }
    }

    public void PowerOff()
	{
        ulong networkObjectId = NetworkManager.Singleton.LocalClient.PlayerObject.NetworkObjectId;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObject))
        {
            var playerController = networkObject.gameObject.GetComponent<playerMoveController>();
            if (playerController != null)
            {
                playerController.EventToggle(false, this.gameObject);
                //shopInteracter.ChangePowerServerRpc(0, false);

                MenuManager.Instance.IsEvenet = false;
                Debug.Log($"Disabled playerMoveController for {networkObject.name}");
            }
            else
            {
                Debug.LogWarning("playerMoveController component not found on the player's NetworkObject.");
            }
        }
        else
        {
            Debug.LogError("Failed to retrieve the player's NetworkObject using LocalClientId.");
        }
        Cursor.lockState = CursorLockMode.Locked;
    }
}

