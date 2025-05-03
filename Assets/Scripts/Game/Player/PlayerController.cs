using UnityEngine;

/// <summary>
/// Controlador del jugador, almacena datos espec�ficos del jugador (por ejemplo, puntos de abordaje logrados).
/// </summary>
public class PlayerController : MonoBehaviour
{
    public int boardingCount = 0;  // Cantidad de abordajes realizados por el jugador

    // Nota: La energ�a del jugador se maneja en GameManager.playerEnergy.
    // Tambi�n podr�amos manejar aqu� otras propiedades del jugador si fuera necesario.
}
