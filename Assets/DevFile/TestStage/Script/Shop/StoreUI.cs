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

    [SerializeField] private List<UIStruct> staticUIList; //정적 UI.
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
        Debug.Log("언어가 변경되었습니다.");
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
        // App 텍스트 설정 및 게임 설정
        foreach (var app in appList)
        {
            app.appWindow.SetActive(true);
        }
    }

    void StaticTMPSet()
	{
        // 정적 UI 요소에 텍스트 설정
        foreach (var ui in staticUIList)
        {
            localizedString.TableReference = ui.table; // 사용하고자 하는 테이블
            localizedString.TableEntryReference = ui.tmpContents; // 아이템 이름

            ui.tmp.text = localizedString.GetLocalizedString();
        }
    }

    void TokenTMPSet(int olddata = 0, int newdata = 0)
	{
        localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
        localizedString.TableEntryReference = token.tmpContents; // 아이템 이름

        token.tmp.text = localizedString.GetLocalizedString() + ":" + SharedData.Instance.Money.Value;

        localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
        localizedString.TableEntryReference = rareToken.tmpContents; // 아이템 이름

        rareToken.tmp.text = localizedString.GetLocalizedString() + ":" + SharedData.Instance.Money.Value;
    }

    void AppButtonSet()
	{
        // App 텍스트 설정 및 게임 설정
        foreach (var app in appList)
        {
            app.appButton.onClick.AddListener(() => { app.appWindow.SetActive(true); });
        }
    }

    void AppTMPSet()
	{
        // App 텍스트 설정 및 게임 설정
        foreach (var app in appList)
        {
            localizedString.TableReference = app.appUI.table; // 사용하고자 하는 테이블
            localizedString.TableEntryReference = app.appUI.tmpContents; // 아이템 이름

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

