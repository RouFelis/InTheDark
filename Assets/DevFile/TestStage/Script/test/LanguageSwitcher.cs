using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageSwitcher : MonoBehaviour
{
    public TMP_Dropdown languageDropdown; // TMP 드롭다운 사용

    private void Start()
    {
        // TMP 드롭다운 초기화 (설정된 로케일 목록을 드롭다운에 추가)
        languageDropdown.options.Clear();
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(locale.Identifier.CultureInfo.NativeName));
        }

        // 현재 언어에 맞춰 드롭다운 선택 초기화
        languageDropdown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        languageDropdown.onValueChanged.AddListener(ChangeLanguage);
    }

    public void ChangeLanguage(int index)
    {
        // 선택된 인덱스에 따라 언어 변경
        Locale selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        StartCoroutine(SetLocale(selectedLocale));
    }

    IEnumerator SetLocale(Locale locale)
    {
        yield return LocalizationSettings.InitializationOperation; // 로케일 초기화 대기
        LocalizationSettings.SelectedLocale = locale; // 선택한 로케일로 변경
    }
}
