using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageSwitcher : MonoBehaviour
{
    public TMP_Dropdown languageDropdown; // TMP ��Ӵٿ� ���

    private void Start()
    {
        // TMP ��Ӵٿ� �ʱ�ȭ (������ ������ ����� ��Ӵٿ �߰�)
        languageDropdown.options.Clear();
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(locale.Identifier.CultureInfo.NativeName));
        }

        // ���� �� ���� ��Ӵٿ� ���� �ʱ�ȭ
        languageDropdown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        languageDropdown.onValueChanged.AddListener(ChangeLanguage);
    }

    public void ChangeLanguage(int index)
    {
        // ���õ� �ε����� ���� ��� ����
        Locale selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        StartCoroutine(SetLocale(selectedLocale));
    }

    IEnumerator SetLocale(Locale locale)
    {
        yield return LocalizationSettings.InitializationOperation; // ������ �ʱ�ȭ ���
        LocalizationSettings.SelectedLocale = locale; // ������ �����Ϸ� ����
    }
}
