using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class MinimalCardUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("UI Minimalista")]
    public TextMeshProUGUI energyCostText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public Image cardImage;

    [HideInInspector] public CardData cardData;
    [HideInInspector] public int slotIndex;

    private Vector3 originalPosition;
    private CanvasGroup canvasGroup;
    private Coroutine holdCoroutine;
    private bool pointerDown;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Asigna datos y slot, actualiza textos e imagen.
    /// </summary>
    public void Initialize(CardData data, int slot)
    {
        cardData = data;
        slotIndex = slot;

        energyCostText.text = data.energyCost.ToString();
        attackText.text = data.attack.ToString();
        defenseText.text = data.defense.ToString();
        if (cardImage != null && data.previewSprite != null)
            cardImage.sprite = data.previewSprite;

        originalPosition = transform.position;
    }

    // ─ Hold para detalle ──────────────────
    public void OnPointerDown(PointerEventData _)
    {
        pointerDown = true;
        holdCoroutine = StartCoroutine(ShowDetailAfterDelay());
    }
    public void OnPointerUp(PointerEventData _)
    {
        pointerDown = false;
        if (holdCoroutine != null) StopCoroutine(holdCoroutine);
    }

    private IEnumerator ShowDetailAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (pointerDown && GameManager.Instance?.uiManager != null)
            GameManager.Instance.uiManager.ShowCardDetail(cardData);
    }

    // ─ Drag & Drop ───────────────────────
    public void OnBeginDrag(PointerEventData _)
    {
        pointerDown = false;
        if (GameManager.Instance?.uiManager?.cardDetailPanel.activeSelf == true)
            GameManager.Instance.uiManager.HideCardDetail();

        originalPosition = transform.position;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += (Vector3)eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Si suelta en zona de spawn:
        if (eventData.pointerEnter != null &&
            eventData.pointerEnter.CompareTag("DropZone") &&
            eventData.pointerEnter.TryGetComponent(out SpawnZone spawn))
        {
            bool played = GameManager.Instance.PlayCard(cardData, spawn.laneIndex, true);
            if (played)
            {
                // Rojo automática y destrucción
                GameManager.Instance.cardManager.RemoveCardFromHand(slotIndex);
                Destroy(gameObject);
                return;
            }
        }

        // Sino, vuelve a posición
        transform.position = originalPosition;
    }
}
