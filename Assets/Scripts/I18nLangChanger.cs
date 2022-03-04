using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class I18nLangChanger : MonoBehaviour
{
    Dropdown dropdown;
    public string language
    {
        get
        {
            return dropdown.itemText.text;
        }
        set
        {
            var index = dropdown.options.FindIndex(e => GetKey(e) == value);

            if (index < 0)
            {
                index = dropdown.options.FindIndex(e => GetKey(e) == GetSystemLanguage());
            }
            if (index < 0) return;

            dropdown.value = index;
        }
    }

    void Start()
    {
        dropdown = GetComponent<Dropdown>();
    }
    public void OnLangChanged(int newLangIndex)
    {
        I18n.ChangeLanguage(GetKey(dropdown.options[newLangIndex]));
    }

    string GetKey(Dropdown.OptionData option)
    {
        return option.text.Split(' ')[0];
    }

    public static string GetSystemLanguage()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Japanese:
                return "ja";
            case SystemLanguage.Korean:
                return "ko";
            case SystemLanguage.Chinese:
                return "zh-hans";
            default:
                return "en";
        }
    }
}
