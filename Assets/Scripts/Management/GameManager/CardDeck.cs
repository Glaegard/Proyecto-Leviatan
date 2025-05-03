using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que representa un mazo de cartas, permite configurar una lista de cartas desde el Inspector.
/// </summary>
[CreateAssetMenu(fileName = "NewDeck", menuName = "Cards/Deck")]
public class CardDeck : ScriptableObject
{
    public List<CardData> cards;  // Lista de cartas que componen el mazo
}
