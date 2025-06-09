using UnityEngine;

public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddCrew(Ship ship, CardData crewCard)
    {
        if (ship == null || crewCard == null) return;
        ship.attack += crewCard.attack;
        ship.maxHealth += crewCard.defense;
        ship.currentHealth += crewCard.defense;
        ship.crewCount++;
        ship.UpdateUI();
        Debug.Log($"Tripulante '{crewCard.GetName()}' añadido a barco en carril {ship.laneIndex}. Stats: {ship.attack}/{ship.currentHealth}");
    }

    public void AddEquipment(Ship ship, CardData equipCard)
    {
        if (ship == null || equipCard == null) return;
        ship.attack += equipCard.attack;
        ship.maxHealth += equipCard.defense;
        ship.currentHealth += equipCard.defense;
        ship.UpdateUI();
        Debug.Log($"Artefacto '{equipCard.GetName()}' equipado en carril {ship.laneIndex}. Stats: {ship.attack}/{ship.currentHealth}");
    }
}