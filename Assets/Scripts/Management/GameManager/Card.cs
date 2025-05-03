using UnityEngine;

/// <summary>
/// Lógica de jugar una carta desde la mano (vinculado al objeto de carta UI).
/// </summary>
public class Card : MonoBehaviour
{
    public CardData data;  // Datos de la carta asociados

    /// <summary>
    /// Juega la carta en el carril indicado (solo válido para cartas tipo Marinero).
    /// </summary>
    public void PlayCard(int laneIndex)
    {
        if (GameManager.Instance == null || data == null)
            return;
        // Intentar jugar el Marinero en el carril especificado
        bool ok = GameManager.Instance.PlayCard(data, laneIndex, true);
        if (ok)
        {
            // Si se pudo jugar, remover la carta de la mano y robar reemplazo
            //GameManager.Instance.cardManager.RemoveCardFromHand(data);
            // Destruir la representación visual de la carta (ya no está en la mano)
            Destroy(gameObject);
        }
    }
}
