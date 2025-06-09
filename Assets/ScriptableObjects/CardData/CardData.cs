using UnityEngine;

public enum CardType
{
    Marinero,
    Capitan,
    Artefacto,
    Maniobra,
    Leviatan
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    [Header("Nombre y Descripciones")]
    public string cardName_ES;
    public string cardName_EN;
    public string abilityText_ES;
    public string abilityText_EN;
    public string loreText_ES;
    public string loreText_EN;

    [Header("Tipo y Coste")]
    public CardType cardType;
    public int energyCost;

    [Header("Estadísticas (Marinero/Capitán)")]
    public int attack;
    public int defense;

    [Header("Arte de la carta")]
    public Sprite previewSprite;
    public Sprite fullSprite;

    [Header("Prefab (Marinero/Capitán/Leviatán)")]
    public GameObject shipPrefab;

    [Header("Efecto Opcional")]
    public CardEffect effect;

    public string GetName()
    {
        return LocalizationManager.Instance.currentLanguage == SystemLanguage.Spanish
            ? cardName_ES : cardName_EN;
    }
    public string GetAbilityText()
    {
        return LocalizationManager.Instance.currentLanguage == SystemLanguage.Spanish
            ? abilityText_ES : abilityText_EN;
    }
    public string GetLoreText()
    {
        return LocalizationManager.Instance.currentLanguage == SystemLanguage.Spanish
            ? loreText_ES : loreText_EN;
    }
}
