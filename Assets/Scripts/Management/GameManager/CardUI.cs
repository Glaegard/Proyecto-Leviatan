using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

/// <summary>
/// Controla la interacción de la carta en la mano (arrastrar, soltar y pulsación prolongada para detalle).
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class CardUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    //──────────────────────── REFERENCIAS UI ────────────────────────//
    [Header("Referencias UI de la Carta")]
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI energyCostText;
    public Image cardImage;

    //──────────────────────── DATOS ─────────────────────────────────//
    [Header("Datos de la Carta")]
    public CardData cardData;
    public int handSlotIndex;                 // lo asigna CardManager

    //──────────────────────── INTERNOS ──────────────────────────────//
    private Vector3 originalPosition;
    private CanvasGroup canvasGroup;
    private Camera mainCamera;
    private Ship highlightedShip;

    // Pulsación prolongada
    private Coroutine holdDetailCoroutine;
    private bool isPointerDown;

    //──────────────────────── INIT ─────────────────────────────────//
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();    // garantizado por RequireComponent
        mainCamera = Camera.main;
    }

    /// <summary>Inicializa la carta UI con sus datos.</summary>
    public void Initialize(CardData data)
    {
        cardData = data;
        cardNameText.text = data.cardName;
        energyCostText.text = data.energyCost.ToString();
        if (data.previewSprite) cardImage.sprite = data.previewSprite;

        originalPosition = transform.position;
    }

    //──────────────────────── PULSACIÓN PROLONGADA ─────────────────//
    public void OnPointerDown(PointerEventData _) { StartHold(); }
    public void OnPointerUp(PointerEventData _) { CancelHold(); }

    private void StartHold()
    {
        isPointerDown = true;
        if (holdDetailCoroutine != null) StopCoroutine(holdDetailCoroutine);
        holdDetailCoroutine = StartCoroutine(HoldAndShowDetail());
    }

    private void CancelHold()
    {
        isPointerDown = false;
        if (holdDetailCoroutine != null) { StopCoroutine(holdDetailCoroutine); holdDetailCoroutine = null; }
    }

    private IEnumerator HoldAndShowDetail()
    {
        yield return new WaitForSeconds(0.5f);
        if (isPointerDown && GameManager.Instance?.uiManager)
            GameManager.Instance.uiManager.ShowCardDetail(cardData);
        holdDetailCoroutine = null;
    }

    //──────────────────────── DRAG ─────────────────────────────────//
    public void OnBeginDrag(PointerEventData _)
    {
        CancelHold();

        var ui = GameManager.Instance?.uiManager;
        if (ui && ui.cardDetailPanel.activeSelf) ui.HideCardDetail();

        originalPosition = transform.position;
        canvasGroup.blocksRaycasts = false;          // para que DropZones reciban eventos
        highlightedShip = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += (Vector3)eventData.delta;

        // ¿nuevo barco bajo el cursor?
        Ship shipUnder = null;
        if (mainCamera &&
            Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit, 100f))
            shipUnder = hit.collider.GetComponentInParent<Ship>();

        if (shipUnder != highlightedShip)
        {
            if (highlightedShip) highlightedShip.SetHighlight(false);
            highlightedShip = shipUnder;
            if (highlightedShip) highlightedShip.SetHighlight(true);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        if (highlightedShip) { highlightedShip.SetHighlight(false); highlightedShip = null; }

        bool played = TryPlayCard(eventData);

        if (!played)
        {
            transform.position = originalPosition;    // volver a mano
            return;
        }

        //───────────── Jugada válida ─────────────//
        //if (handSlotIndex >= 0)
            //StartCoroutine(ReplaceCardNextFrame(handSlotIndex));

        Destroy(gameObject);                         // destruir carta flotante
    }

    //──────────────────────── HELPERS ──────────────────────────────//
    private bool TryPlayCard(PointerEventData eventData)
    {
        // Drop sobre SpawnZone
        if (eventData.pointerEnter && eventData.pointerEnter.CompareTag("DropZone"))
        {
            if (eventData.pointerEnter.TryGetComponent(out SpawnZone spawn))
                return GameManager.Instance.PlayCard(cardData, spawn.laneIndex, true);
        }
        // Drop sobre barco
        if (mainCamera &&
            Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit, 100f))
        {
            var ship = hit.collider.GetComponentInParent<Ship>();
            if (ship) return GameManager.Instance.PlayCardOnShip(cardData, ship, true);
        }
        return false;
    }

    /*
    private IEnumerator ReplaceCardNextFrame(int slot)
    {
        yield return null;                            // esperar un frame para que Destroy() se procese
        GameManager.Instance.cardManager.RemoveCardFromHand(slot);
    }*/

    //──────────────────────── DOBLE CLIC ───────────────────────────//
    public void OnPointerClick(PointerEventData e)
    {
        if (e.clickCount == 2 && GameManager.Instance?.uiManager)
            GameManager.Instance.uiManager.ShowCardDetail(cardData);
    }
}
