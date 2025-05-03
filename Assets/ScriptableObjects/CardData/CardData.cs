using UnityEngine;

/// <summary>
/// Enumeración de tipos de cartas disponibles en el juego.
/// </summary>
public enum CardType
{
    Marinero,   // Tripulante que puede crear o unirse a un barco
    Maquinaria, // Equipamiento para barcos
    Maniobra    // Carta de acción especial (hechizo/movimiento)
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    [Header("Información básica")]
    public string cardName;
    public CardType cardType;
    public int energyCost;
    public string abilityText;   // Descripción o habilidad de la carta (efecto en texto)
    public string loreText;      // Texto de ambientación o historia de la carta

    [Header("Estadísticas (para cartas de tipo Marinero)")]
    public int attack;
    public int defense;          // Puntos de salud/defensa del barco o unidad asociada

    [Header("Arte de la carta")]
    public Sprite previewSprite; // Imagen de vista previa (miniatura) para mostrar en la mano
    public Sprite fullSprite;    // Imagen de arte completo para mostrar en la vista de detalle

    [Header("Prefab asociado")]
    public GameObject shipPrefab; // Prefab del barco generado por cartas tipo Marinero (puede ser null para otros tipos)
}
