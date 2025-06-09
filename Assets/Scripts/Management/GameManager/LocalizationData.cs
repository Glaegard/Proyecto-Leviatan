using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TranslationEntry
{
    public string key;
    [TextArea] public string text;
}

[CreateAssetMenu(fileName = "LocalizationData", menuName = "Localization/LanguageData")]
public class LocalizationData : ScriptableObject
{
    public SystemLanguage language;
    public List<TranslationEntry> entries;
}
