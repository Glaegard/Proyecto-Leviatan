using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Mazo del Jugador")]
    public CardDeck deckAsset;

    [Header("Slots (4)")]
    [Range(1, 8)]
    public int handSize = 4;
    public Transform[] slotPanels;          // 4 paneles en Canvas

    [Header("Prefab Minimalista")]
    public GameObject minimalCardPrefab;    // Prefab con script MinimalCardUI

    private Queue<CardData> deckQueue;      // Mazo de robo
    private CardData[] currentCards;        // Carta actual en cada slot

    private void Start()
    {
        // Validaciones
        if (deckAsset == null || minimalCardPrefab == null ||
            slotPanels == null || slotPanels.Length != handSize)
        {
            Debug.LogError("CardManager: config slots o prefabs incorrectos.");
            return;
        }

        // Construir cola de cartas
        deckQueue = new Queue<CardData>(deckAsset.cards);
        currentCards = new CardData[handSize];

        // Reparto inicial
        for (int i = 0; i < handSize; i++)
            DrawToSlot(i);
    }

    /// <summary>
    /// Roba del mazo y crea MinimalCardUI en slot dado
    /// </summary>
    private void DrawToSlot(int slot)
    {
        // Borrar lo que hubiera
        foreach (Transform child in slotPanels[slot])
            Destroy(child.gameObject);

        if (deckQueue.Count == 0)
        {
            currentCards[slot] = null;
            return;
        }

        // Robar y mostrar
        CardData data = deckQueue.Dequeue();
        currentCards[slot] = data;
        GameObject go = Instantiate(minimalCardPrefab, slotPanels[slot], false);
        var mc = go.GetComponent<MinimalCardUI>();
        mc.Initialize(data, slot);
    }

    /// <summary>
    /// Llamado tras jugar carta; roba nueva en mismo slot.
    /// </summary>
    public void RemoveCardFromHand(int slot)
    {
        // Marcar vacío
        currentCards[slot] = null;
        // Robar siguiente
        DrawToSlot(slot);
    }
}
