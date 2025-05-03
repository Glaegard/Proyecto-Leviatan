using UnityEngine;

/// <summary>
/// Controlador del jugador, almacena datos específicos del jugador (por ejemplo, puntos de abordaje logrados).
/// </summary>
public class PlayerController : MonoBehaviour
{
    public int boardingCount = 0;  // Cantidad de abordajes realizados por el jugador

    // Nota: La energía del jugador se maneja en GameManager.playerEnergy.
    // También podríamos manejar aquí otras propiedades del jugador si fuera necesario.
}
