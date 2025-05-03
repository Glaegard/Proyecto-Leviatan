using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reparte TODO el mazo entre los slots al empezar.
/// No hay robo ni descarte automático: simplificación total.
/// </summary>
public class CardManager : MonoBehaviour
{
    //────────────────── CONFIG ──────────────────//
    [Header("Mazo del Jugador")]
    public CardDeck deckAsset;                     // ScriptableObject con todas las cartas

    [Header("Slots")]
    public int handSize = 5;                       // Número de paneles-slot
    [Tooltip("Arrastra aquí los paneles-slot en orden 0-N.")]
    public Transform[] slotPanels;                 // Padres visuales

    [Header("Prefabs y UI")]
    public GameObject cardUIPrefab;                // Prefab de carta (con CardUI)

    // Datos en tiempo real (sólo para inspección / debug)
    [SerializeField] private List<CardData>[] cardsBySlot;   // Lista de cartas en cada slot

    //────────────────── INICIO ───────────────────//
    private void Start()
    {
        if (!deckAsset) { Debug.LogError("CardManager: deckAsset no asignado."); return; }
        if (!cardUIPrefab) { Debug.LogError("CardManager: cardUIPrefab no asignado."); return; }
        if (slotPanels == null || slotPanels.Length != handSize)
        {
            Debug.LogError("CardManager: slotPanels incompleto.");
            return;
        }

        // Preparar container para inspección
        cardsBySlot = new List<CardData>[handSize];
        for (int i = 0; i < handSize; i++) cardsBySlot[i] = new List<CardData>();

        DistributeAllCards();
    }

    //──────────────── DISTRIBUCIÓN ───────────────//
    private void DistributeAllCards()
    {
        List<CardData> fullDeck = new List<CardData>(deckAsset.cards);
        int total = fullDeck.Count;

        // Calcula cartas por slot (distribución redonda)
        int baseCount = total / handSize;
        int extraSlots = total % handSize;   // Los primeros 'extraSlots' reciben +1 carta

        int index = 0;
        for (int slot = 0; slot < handSize; slot++)
        {
            int cardsInThisSlot = baseCount + (slot < extraSlots ? 1 : 0);

            for (int j = 0; j < cardsInThisSlot; j++)
            {
                CardData card = fullDeck[index++];
                cardsBySlot[slot].Add(card);
                InstantiateCardUI(card, slot, j);
            }
        }

        Debug.Log($"CardManager: repartidas {total} cartas entre {handSize} slots.");
    }

    //──────────────── INSTANCIAR UI ──────────────//
    private void InstantiateCardUI(CardData data, int slot, int stackIndex)
    {
        Transform parent = slotPanels[slot];
        if (!parent) { Debug.LogError($"Slot {slot} sin Transform."); return; }

        GameObject ui = Instantiate(cardUIPrefab, parent, false);
        ui.name = data.cardName;

        // Apilar ligeramente las cartas con un desplazamiento Y
        float yOffset = -30f * stackIndex;         // Ajusta a tu gusto
        RectTransform rt = ui.GetComponent<RectTransform>();
        if (rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(0, yOffset);
            rt.offsetMax = new Vector2(0, yOffset);
        }

        // Inicializar CardUI
        if (ui.TryGetComponent(out CardUI cui))
        {
            cui.Initialize(data);
            cui.handSlotIndex = slot;
        }
    }

    //──────────────── DEBUG OPCIONAL ─────────────//
    [ContextMenu("Debug Hand Content")]
    public void DebugHand()
    {
        for (int i = 0; i < handSize; i++)
        {
            string list = string.Join(", ", cardsBySlot[i].ConvertAll(c => c.cardName));
            Debug.Log($"slot_{i}: {list}");
        }
    }
}

