using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Assets de Traducción")]
    public LocalizationData spanishData;
    public LocalizationData englishData;

    public SystemLanguage currentLanguage = SystemLanguage.Spanish;
    private Dictionary<string, string> translations = new Dictionary<string, string>();
    public event Action OnLanguageChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadTranslations(currentLanguage);
    }

    public void SetLanguage(SystemLanguage newLang)
    {
        if (newLang == currentLanguage) return;
        LoadTranslations(newLang);
        OnLanguageChanged?.Invoke();
    }

    private void LoadTranslations(SystemLanguage lang)
    {
        currentLanguage = lang;
        translations.Clear();
        var data = (lang == SystemLanguage.Spanish ? spanishData : englishData);
        if (data != null)
        {
            foreach (var e in data.entries) translations[e.key] = e.text;
        }
        else
        {
            Debug.LogWarning($"No hay datos de localización para {lang}");
        }
    }

    public string Translate(string key)
    {
        return translations.TryGetValue(key, out var txt) ? txt : key;
    }
}
