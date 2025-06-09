using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Configuración del Mazo (IA)")]
    public CardDeck deckAsset;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    [HideInInspector]
    public int boardingCount = 0;  // Ahora existe para Trackear abordajes

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

        while (GameManager.Instance.currentGameState == GameState.Playing)
        {
            yield return new WaitForSeconds(Random.Range(2f, 4f));

            // 1) Intentar lanzamiento automático
            int laneToLaunch = GetRandomBufferedLane();
            if (laneToLaunch != -1 && Random.value < 0.6f)
            {
                bool launched = GameManager.Instance.LaunchShipFromLane(laneToLaunch, false);
                if (launched) continue;
            }

            // 2) Reciclar mazo si está vacío
            if (deck.Count == 0 && discardPile.Count > 0)
            {
                deck.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck();
            }

            // 3) Jugar carta
            if (GameManager.Instance.aiEnergy > 0 && deck.Count > 0)
            {
                CardData chosenCard = null;
                int targetLane = -1;
                Ship targetShip = null;

                // 3.1) Marinero a buffer libre
                int freeLane = GetFreeBufferLane();
                if (freeLane != -1)
                {
                    foreach (var c in deck)
                        if (c.cardType == CardType.Marinero && c.energyCost <= GameManager.Instance.aiEnergy)
                        {
                            chosenCard = c;
                            targetLane = freeLane;
                            break;
                        }
                }

                // 3.2) Marinero a buffer existente
                if (chosenCard == null)
                {
                    int buffered = GetLaneWithActiveBuffer();
                    if (buffered != -1)
                    {
                        foreach (var c in deck)
                            if (c.cardType == CardType.Marinero && c.energyCost <= GameManager.Instance.aiEnergy)
                            {
                                chosenCard = c;
                                targetLane = buffered;
                                break;
                            }
                    }
                }

                // 3.3) Artefacto sobre barco
                if (chosenCard == null)
                {
                    var ships = GetAIShipsInPlay();
                    if (ships.Count > 0)
                    {
                        foreach (var c in deck)
                            if (c.cardType == CardType.Artefacto && c.energyCost <= GameManager.Instance.aiEnergy)
                            {
                                chosenCard = c;
                                targetShip = ships[Random.Range(0, ships.Count)];
                                break;
                            }
                    }
                }

                // 3.4) Maniobra
                if (chosenCard == null)
                {
                    foreach (var c in deck)
                        if (c.cardType == CardType.Maniobra && c.energyCost <= GameManager.Instance.aiEnergy)
                        {
                            chosenCard = c;
                            break;
                        }
                }

                // 3.5) Ejecutar jugada
                if (chosenCard != null)
                {
                    deck.Remove(chosenCard);
                    discardPile.Add(chosenCard);

                    GameManager.Instance.TryPlayCard(
                        chosenCard,
                        targetLane,
                        targetShip,
                        false
                    );
                }
            }
        }
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            var temp = deck[i];
            int r = Random.Range(i, deck.Count);
            deck[i] = deck[r];
            deck[r] = temp;
        }
    }

    private int GetFreeBufferLane()
    {
        for (int i = 0; i < LaneManager.Instance.laneCount; i++)
        {
            var buf = GameManager.Instance.enemySpawnBuffers[i];
            if (buf.IsEmpty &&
                Time.time >= GameManager.Instance.lastPlayTimePerLane[i] + GameManager.Instance.marinerCooldown)
                return i;
        }
        return -1;
    }

    private int GetLaneWithActiveBuffer()
    {
        for (int i = 0; i < LaneManager.Instance.laneCount; i++)
            if (!GameManager.Instance.enemySpawnBuffers[i].IsEmpty)
                return i;
        return -1;
    }

    private int GetRandomBufferedLane()
    {
        var lanes = new List<int>();
        for (int i = 0; i < LaneManager.Instance.laneCount; i++)
        {
            var buf = GameManager.Instance.enemySpawnBuffers[i];
            if (!buf.IsEmpty &&
                Time.time >= GameManager.Instance.lastPlayTimePerLane[i] + GameManager.Instance.marinerCooldown)
                lanes.Add(i);
        }
        if (lanes.Count == 0) return -1;
        return lanes[Random.Range(0, lanes.Count)];
    }

    private List<Ship> GetAIShipsInPlay()
    {
        var list = new List<Ship>();
        foreach (var lane in LaneManager.Instance.enemyShips)
            list.AddRange(lane);
        return list;
    }
}
