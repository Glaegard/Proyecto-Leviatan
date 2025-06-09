using UnityEngine;

/// <summary>
/// Base para efectos de carta, implementados como ScriptableObjects.
/// </summary>
public abstract class CardEffect : ScriptableObject
{
    /// <summary>
    /// Ejecuta el efecto de la carta en el contexto dado.
    /// </summary>
    public abstract void ApplyEffect(
        GameManager game,
        Ship targetShip = null,
        int targetLane = -1,
        bool isPlayer = true
    );
}
