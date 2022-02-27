using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class I18n : MonoBehaviour
{
    static Dictionary<string, string> dict;
    static List<I18n> i18nComponents = new List<I18n>();
    static string lang = "en";

    [SerializeField] string key;
    Text text;

    void Start()
    {
        i18nComponents.Add(this);
        text = GetComponent<Text>();

        Load(false);
        RefreshText();
    }

    void RefreshText()
    {
        if (text == null) text = GetComponent<Text>();
        string labelText;
        if (dict != null && dict.TryGetValue(key, out labelText)) text.text = labelText;
    }

    void Reset()
    {
        key = GetComponent<Text>().text;
    }

    static void Load(bool refresh)
    {
        if (!refresh && dict != null) return;
        dict = Resources.Load<TextAsset>($"i18n/{lang}").text.Split('\n').Where(row => !string.IsNullOrWhiteSpace(row)).Select(row => row.Trim().Split('=')).Where(elem => elem.Length >= 2).ToDictionary(elem => elem[0], elem => elem[1]);
    }

    public static void ChangeLanguage(string newLang)
    {
        lang = newLang;
        Load(true);
        i18nComponents.ForEach(i18n =>i18n.RefreshText());
    }

}
