using UnityEngine;

public class Card : MonoBehaviour
{
    public CardData data;

    /// <summary>
    /// Juega la carta como Marinero en el carril indicado.
    /// </summary>
    public void PlayCard(int laneIndex)
    {
        if (GameManager.Instance == null || data == null) return;
        bool ok = GameManager.Instance.TryPlayCard(data, laneIndex, null, true);
        if (ok)
        {
            Destroy(gameObject);
        }
    }
}