using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Configuración del Mazo (IA)")]
    public CardDeck deckAsset;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    public int boardingCount = 0;

    private Coroutine aiPlayCoroutine;

    private void Awake()
    {
        if (deckAsset != null)
            deck = new List<CardData>(deckAsset.cards);
    }

    public void StartAI()
    {
        ShuffleDeck();
        if (aiPlayCoroutine == null)
            aiPlayCoroutine = StartCoroutine(AIPlayRoutine());
    }

    private IEnumerator AIPlayRoutine()
    {
        yield return new WaitForSeconds(2f);

        while (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Playing)
        {
            yield return new WaitForSeconds(Random.Range(2f, 4f));

            // Lanzar barco desde buffer si está listo
            int laneToLaunch = GetRandomBufferedLane();
            if (laneToLaunch != -1 && Random.value < 0.6f)
            {
                bool launched = GameManager.Instance.LaunchShipFromLane(laneToLaunch, false);
                if (launched)
                {
                    Debug.Log($"IA lanza barco en carril {laneToLaunch}");
                    continue; // omitir jugar carta este ciclo
                }
            }

            // Si el mazo está vacío, reciclar descarte
            if (deck.Count == 0 && discardPile.Count > 0)
            {
                deck.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck();
            }

            if (GameManager.Instance.aiEnergy > 0 && deck.Count > 0)
            {
                CardData chosenCard = null;
                int targetLane = -1;
                Ship targetShip = null;

                // Jugar Marinero en carril sin buffer
                int freeLane = GetFreeBufferLane();
                if (freeLane != -1)
                {
                    foreach (CardData card in deck)
                    {
                        if (card.cardType == CardType.Marinero && card.energyCost <= GameManager.Instance.aiEnergy)
                        {
                            chosenCard = card;
                            targetLane = freeLane;
                            break;
                        }
                    }
                }

                // Añadir Marinero a buffer existente si no encontró libre
                if (chosenCard == null)
                {
                    int laneWithBuffer = GetLaneWithActiveBuffer();
                    if (laneWithBuffer != -1)
                    {
                        foreach (CardData card in deck)
                        {
                            if (card.cardType == CardType.Marinero && card.energyCost <= GameManager.Instance.aiEnergy)
                            {
                                chosenCard = card;
                                targetLane = laneWithBuffer;
                                break;
                            }
                        }
                    }
                }

                // Jugar carta sobre barco existente (Maquinaria)
                if (chosenCard == null)
                {
                    List<Ship> targets = GetAIShipsInPlay();
                    if (targets.Count > 0)
                    {
                        foreach (CardData card in deck)
                        {
                            if (card.cardType == CardType.Maquinaria && card.energyCost <= GameManager.Instance.aiEnergy)
                            {
                                chosenCard = card;
                                targetShip = targets[Random.Range(0, targets.Count)];
                                break;
                            }
                        }
                    }
                }

                // Jugar Maniobra
                if (chosenCard == null)
                {
                    foreach (CardData card in deck)
                    {
                        if (card.cardType == CardType.Maniobra && card.energyCost <= GameManager.Instance.aiEnergy)
                        {
                            chosenCard = card;
                            break;
                        }
                    }
                }

                if (chosenCard != null)
                {
                    deck.Remove(chosenCard);
                    discardPile.Add(chosenCard);

                    if (targetLane != -1)
                    {
                        GameManager.Instance.PlayCard(chosenCard, targetLane, false);
                        Debug.Log("IA juega '" + chosenCard.cardName + "' en carril " + targetLane);
                    }
                    else if (targetShip != null)
                    {
                        GameManager.Instance.PlayCardOnShip(chosenCard, targetShip, false);
                        Debug.Log("IA usa '" + chosenCard.cardName + "' sobre barco en carril " + targetShip.laneIndex);
                    }
                    else
                    {
                        GameManager.Instance.PlayCard(chosenCard, 0, false);
                        Debug.Log("IA juega maniobra '" + chosenCard.cardName + "'");
                    }
                }
            }
        }
    }

    private List<Ship> GetAIShipsInPlay()
    {
        List<Ship> ships = new List<Ship>();
        foreach (var lane in LaneManager.Instance.enemyShips)
        {
            ships.AddRange(lane);
        }
        return ships;
    }

    private int GetFreeBufferLane()
    {
        for (int i = 0; i < LaneManager.Instance.laneCount; i++)
        {
            if (GameManager.Instance.enemySpawnBuffers[i].IsEmpty &&
                Time.time >= GameManager.Instance.lastPlayTimePerLane[i] + GameManager.Instance.marinerCooldown)
            {
                return i;
            }
        }
        return -1;
    }

    private int GetLaneWithActiveBuffer()
    {
        for (int i = 0; i < LaneManager.Instance.laneCount; i++)
        {
            if (!GameManager.Instance.enemySpawnBuffers[i].IsEmpty)
                return i;
        }
        return -1;
    }

    private int GetRandomBufferedLane()
    {
        List<int> lanes = new List<int>();
        for (int i = 0; i < LaneManager.Instance.laneCount; i++)
        {
            if (!GameManager.Instance.enemySpawnBuffers[i].IsEmpty &&
                Time.time >= GameManager.Instance.lastPlayTimePerLane[i] + GameManager.Instance.marinerCooldown)
            {
                lanes.Add(i);
            }
        }

        if (lanes.Count == 0) return -1;
        return lanes[Random.Range(0, lanes.Count)];
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardData temp = deck[i];
            int r = Random.Range(i, deck.Count);
            deck[i] = deck[r];
            deck[r] = temp;
        }
    }
}
