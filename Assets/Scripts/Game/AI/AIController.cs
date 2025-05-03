using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador de la IA, gestiona el mazo de la IA y la lógica para jugar cartas automáticamente.
/// </summary>
public class AIController : MonoBehaviour
{
    [Header("Configuración del Mazo (IA)")]
    public CardDeck deckAsset;  // Mazo de la IA configurado en el Inspector
    private List<CardData> deck = new List<CardData>();       // Lista interna de cartas disponibles para la IA
    private List<CardData> discardPile = new List<CardData>(); // Lista de cartas ya jugadas por la IA (para reciclar si se agota el mazo)

    public int boardingCount = 0;  // Cantidad de abordajes realizados por la IA

    private Coroutine aiPlayCoroutine;

    private void Awake()
    {
        // Inicializar mazo interno de la IA a partir del asset
        if (deckAsset != null)
        {
            deck = new List<CardData>(deckAsset.cards);
        }
    }

    /// <summary>
    /// Inicia la rutina de turnos automáticos de la IA.
    /// </summary>
    public void StartAI()
    {
        // Barajar el mazo de la IA antes de empezar
        ShuffleDeck();
        // Comenzar la corrutina de juego automático si no ha sido iniciada
        if (aiPlayCoroutine == null)
        {
            aiPlayCoroutine = StartCoroutine(AIPlayRoutine());
        }
    }

    private IEnumerator AIPlayRoutine()
    {
        // Pequeña espera inicial antes de que la IA realice su primera acción
        yield return new WaitForSeconds(2f);

        // Loop principal de la IA mientras el juego está en curso
        while (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Playing)
        {
            // Esperar un intervalo aleatorio entre acciones de la IA para simular tiempo de "pensar"
            yield return new WaitForSeconds(Random.Range(2f, 5f));

            // Si el mazo está vacío, reciclar la pila de descarte
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

                // 1. Prioridad: jugar un Marinero en un carril libre (para generar un nuevo barco)
                int freeLaneIndex = GetFreeLaneIndex();
                if (freeLaneIndex != -1)
                {
                    foreach (CardData card in deck)
                    {
                        if (card.cardType == CardType.Marinero && card.energyCost <= GameManager.Instance.aiEnergy)
                        {
                            chosenCard = card;
                            targetLane = freeLaneIndex;
                            break;
                        }
                    }
                }

                // 2. Si no va a jugar Marinero, intentar jugar Maquinaria en un barco existente
                if (chosenCard == null)
                {
                    // Verificar si la IA tiene al menos un barco en juego
                    bool hasShipInPlay = false;
                    foreach (Ship s in LaneManager.Instance.enemyShips)
                    {
                        if (s != null)
                        {
                            hasShipInPlay = true;
                            break;
                        }
                    }
                    if (hasShipInPlay)
                    {
                        foreach (CardData card in deck)
                        {
                            if (card.cardType == CardType.Maquinaria && card.energyCost <= GameManager.Instance.aiEnergy)
                            {
                                chosenCard = card;
                                // Elegir aleatoriamente uno de los barcos de la IA para equipar
                                List<Ship> possibleTargets = new List<Ship>();
                                foreach (Ship s in LaneManager.Instance.enemyShips)
                                {
                                    if (s != null) possibleTargets.Add(s);
                                }
                                if (possibleTargets.Count > 0)
                                {
                                    targetShip = possibleTargets[Random.Range(0, possibleTargets.Count)];
                                }
                                break;
                            }
                        }
                    }
                }

                // 3. Si no se eligió nada aún, considerar jugar una Maniobra (accion especial)
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

                // Ejecutar la jugada elegida, si hay una carta seleccionada
                if (chosenCard != null)
                {
                    // Remover la carta del mazo de la IA y pasarla a descarte
                    deck.Remove(chosenCard);
                    discardPile.Add(chosenCard);

                    // Realizar la acción dependiendo de si requiere carril o barco objetivo
                    if (targetLane != -1)
                    {
                        // Jugar carta Marinero en carril libre
                        GameManager.Instance.PlayCard(chosenCard, targetLane, false);
                        Debug.Log("IA juega '" + chosenCard.cardName + "' en el carril " + targetLane);
                    }
                    else if (targetShip != null)
                    {
                        // Jugar carta sobre un barco existente (tripulante extra o equipamiento)
                        GameManager.Instance.PlayCardOnShip(chosenCard, targetShip, false);
                        Debug.Log("IA usa '" + chosenCard.cardName + "' sobre su barco en carril " + targetShip.laneIndex);
                    }
                    else
                    {
                        // Jugar carta de Maniobra (acción especial) sin objetivo específico
                        GameManager.Instance.PlayCard(chosenCard, 0, false);
                        Debug.Log("IA juega maniobra '" + chosenCard.cardName + "'");
                    }
                }
            }
            // (Si no había cartas jugables o energía suficiente, la IA simplemente espera al siguiente ciclo)
        }
    }

    /// <summary>
    /// Busca un carril libre del lado de la IA donde no haya un barco actualmente.
    /// </summary>
    private int GetFreeLaneIndex()
    {
        for (int i = 0; i < LaneManager.Instance.laneCount; i++)
        {
            if (LaneManager.Instance.enemyShips[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Baraja el mazo interno de la IA.
    /// </summary>
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
