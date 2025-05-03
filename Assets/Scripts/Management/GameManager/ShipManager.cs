using UnityEngine;

/// <summary>
/// Gestiona operaciones sobre barcos activos (agregar tripulación, equipar maquinaria, etc.).
/// </summary>
public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    /// <summary>
    /// Agrega un nuevo tripulante a un barco existente, incrementando sus estadísticas.
    /// </summary>
    public void AddCrew(Ship ship, CardData crewCard)
    {
        if (ship == null || crewCard == null) return;
        // Sumar los valores de ataque y defensa del nuevo tripulante al barco
        ship.attack += crewCard.attack;
        ship.maxHealth += crewCard.defense;
        ship.currentHealth += crewCard.defense;
        ship.crewCount++;
        ship.UpdateUI();
        Debug.Log($"ShipManager: Tripulante añadido a barco en carril {ship.laneIndex}. Nuevo ATQ {ship.attack}, DEF {ship.currentHealth}/{ship.maxHealth}.");
    }

    /// <summary>
    /// Equipa una carta de maquinaria a un barco, mejorando sus estadísticas.
    /// </summary>
    public void AddEquipment(Ship ship, CardData equipCard)
    {
        if (ship == null || equipCard == null) return;
        // Sumar los valores de ataque y defensa otorgados por el equipamiento
        ship.attack += equipCard.attack;
        ship.maxHealth += equipCard.defense;
        ship.currentHealth += equipCard.defense;
        // (Equipar maquinaria no cambia la cantidad de tripulación)
        ship.UpdateUI();
        Debug.Log($"ShipManager: Equipamiento \"{equipCard.cardName}\" añadido a barco en carril {ship.laneIndex}. Stats ahora: {ship.attack} ATQ, {ship.currentHealth}/{ship.maxHealth} DEF.");
    }
}
