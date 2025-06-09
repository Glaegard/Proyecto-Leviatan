using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpawnZone : MonoBehaviour, IDropHandler
{
    [Tooltip("Índice del carril (0, 1, 2, ...).")]
    public int laneIndex;

    [Tooltip("Transform 3D donde aparecerá el barco (usado por el botón de lanzamiento).")]
    public Transform spawnPoint;

    public void OnDrop(PointerEventData eventData)
    {
        var cardUI = eventData.pointerDrag?.GetComponent<MinimalCardUI>();
        if (cardUI == null) return;

        bool played = GameManager.Instance.TryPlayCard(
            cardUI.cardData,
            laneIndex,
            null,
            true
        );
        if (!played)
        {
            cardUI.ReturnToSlot();
            return;
        }

        GameManager.Instance.cardManager.RemoveCardFromHand(cardUI.slotIndex);
        Destroy(cardUI.gameObject);
        // El barco real saldrá sólo cuando pulses el botón LanzarLaneX
    }
}
