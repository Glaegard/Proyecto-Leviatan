using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Mazo (Scriptable)")]
    public CardDeck deckAsset;

    [Header("Slots Mano")]
    public int handSize = 4;
    public Transform[] slotPanels;
    public GameObject minimalCardPrefab;

    private Queue<CardData> deckQueue;
    private CardData[] currentCards;

    private void Start()
    {
        if (deckAsset == null)
        {
            Debug.LogError("CardManager: deckAsset no asignado.");
            return;
        }
        deckQueue = new Queue<CardData>(deckAsset.cards);
        currentCards = new CardData[handSize];
        for (int i = 0; i < handSize; i++)
            DrawToSlot(i);
    }

    private void DrawToSlot(int slot)
    {
        foreach (Transform c in slotPanels[slot])
            Destroy(c.gameObject);

        if (deckQueue.Count == 0)
        {
            currentCards[slot] = null;
            return;
        }

        var data = deckQueue.Dequeue();
        currentCards[slot] = data;
        var go = Instantiate(minimalCardPrefab, slotPanels[slot], false);
        var mc = go.GetComponent<MinimalCardUI>();
        mc.Initialize(data, slot);
        AudioManager.Instance.Play("draw"); // añadimos sonido de robar carta
    }

    public void RemoveCardFromHand(int slot)
    {
        currentCards[slot] = null;
        DrawToSlot(slot);
    }
}
