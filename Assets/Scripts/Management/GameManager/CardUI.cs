using UnityEngine;                      // Necesario para componentes de Unity.
using UnityEngine.UI;                  // Para trabajar con UI (Image, etc.).
using UnityEngine.EventSystems;        // Para detectar eventos como drag y clics.
using System.Collections;              // Para usar corutinas.
using TMPro;                           // Para usar TextMeshPro.




// BIENVENIDO AL SCRIPT DEL PUTO INFIERNO 👹👹👹👹👹👹👹



/// <summary>
/// Controla la interacción de la carta en la mano (arrastrar, soltar y pulsación prolongada para detalle).
/// </summary>
[RequireComponent(typeof(CanvasGroup))] // Obliga a que el objeto tenga CanvasGroup para gestionar transparencia y eventos.
public class CardUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, // Interfaces para arrastrar
    IPointerClickHandler, IPointerDownHandler, IPointerUpHandler // Interfaces para clics y pulsaciones
{
    //──────────────────────── REFERENCIAS UI ────────────────────────//
    [Header("Referencias UI de la Carta")]
    public TextMeshProUGUI cardNameText;          // Texto para mostrar el nombre de la carta.
    public TextMeshProUGUI energyCostText;        // Texto para mostrar el coste de energía.
    public Image cardImage;                       // Imagen principal de la carta.

    //──────────────────────── DATOS ─────────────────────────────────//
    [Header("Datos de la Carta")]
    public CardData cardData;                     // Referencia a los datos de la carta.
    public int handSlotIndex;                     // Índice del slot en la mano (lo asigna CardManager).

    //──────────────────────── INTERNOS ──────────────────────────────//
    private Vector3 originalPosition;             // Posición original antes del drag.
    private CanvasGroup canvasGroup;              // Para controlar raycasts y opacidad.
    private Camera mainCamera;                    // Cámara principal del juego.
    private Ship highlightedShip;                 // Barco actualmente resaltado bajo el cursor.

    // Pulsación prolongada
    private Coroutine holdDetailCoroutine;        // Corutina activa para mostrar detalle.
    private bool isPointerDown;                   // Indica si el puntero sigue presionado.

    //──────────────────────── INIT ─────────────────────────────────//
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>(); // Obtiene el CanvasGroup (garantizado por RequireComponent).
        mainCamera = Camera.main;                  // Guarda referencia a la cámara principal.
    }

    /// <summary>Inicializa la carta UI con sus datos.</summary>
    public void Initialize(CardData data)
    {
        cardData = data;                             // Asigna los datos.
        cardNameText.text = data.cardName;           // Muestra el nombre.
        energyCostText.text = data.energyCost.ToString(); // Muestra el coste de energía.
        if (data.previewSprite) cardImage.sprite = data.previewSprite; // Si hay sprite, lo asigna.

        originalPosition = transform.position;       // Guarda la posición inicial.
    }

    //──────────────────────── PULSACIÓN PROLONGADA ─────────────────//

    public void OnPointerDown(PointerEventData _) { StartHold(); } // Al pulsar el ratón o dedo, inicia la espera.
    public void OnPointerUp(PointerEventData _) { CancelHold(); }  // Al soltar, cancela si no se completó.

    private void StartHold()
    {
        isPointerDown = true;                                // Marca que el puntero está presionado.
        if (holdDetailCoroutine != null) StopCoroutine(holdDetailCoroutine); // Si ya había corutina, la detiene.
        holdDetailCoroutine = StartCoroutine(HoldAndShowDetail()); // Inicia nueva corutina de espera.
    }

    private void CancelHold()
    {
        isPointerDown = false;                               // El puntero ya no está presionado.
        if (holdDetailCoroutine != null)                     // Si hay corutina activa...
        {
            StopCoroutine(holdDetailCoroutine);              // ...la detiene.
            holdDetailCoroutine = null;
        }
    }

    private IEnumerator HoldAndShowDetail()
    {
        yield return new WaitForSeconds(0.5f);               // Espera medio segundo.
        if (isPointerDown && GameManager.Instance?.uiManager) // Si aún se mantiene la pulsación...
            GameManager.Instance.uiManager.ShowCardDetail(cardData); // Muestra detalle de la carta.
        holdDetailCoroutine = null;                          // Limpia la referencia a la corutina.
    }

    //──────────────────────── DRAG ─────────────────────────────────//

    public void OnBeginDrag(PointerEventData _)
    {
        CancelHold();                                         // Cancela posible pulsación larga.
        var ui = GameManager.Instance?.uiManager;
        if (ui && ui.cardDetailPanel.activeSelf) ui.HideCardDetail(); // Oculta panel de detalle si estaba visible.

        originalPosition = transform.position;                // Guarda la posición inicial para volver si es necesario.
        canvasGroup.blocksRaycasts = false;                   // Permite que objetos debajo reciban eventos de drop.
        highlightedShip = null;                               // Resetea cualquier barco resaltado.
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += (Vector3)eventData.delta;       // Mueve la carta según el desplazamiento del cursor.

        // Detecta si hay un barco debajo del cursor
        Ship shipUnder = null;
        if (mainCamera &&
            Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit, 100f))
            shipUnder = hit.collider.GetComponentInParent<Ship>();

        if (shipUnder != highlightedShip)                    // Si hay un nuevo barco diferente...
        {
            if (highlightedShip) highlightedShip.SetHighlight(false); // Quita highlight al anterior.
            highlightedShip = shipUnder;                     // Actualiza referencia.
            if (highlightedShip) highlightedShip.SetHighlight(true); // Aplica highlight al nuevo.
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;                   // Vuelve a bloquear raycasts.
        if (highlightedShip) { highlightedShip.SetHighlight(false); highlightedShip = null; } // Quita highlight.

        bool played = TryPlayCard(eventData);                // Intenta jugar la carta.

        if (!played)
        {
            transform.position = originalPosition;           // Si no se jugó, vuelve a su lugar.
            return;
        }

        //───────────── Jugada válida ─────────────//
        // if (handSlotIndex >= 0)
        //     StartCoroutine(ReplaceCardNextFrame(handSlotIndex)); // (opcional) reemplazar carta

        Destroy(gameObject);                                 // Destruye la carta de la mano.
    }

    //──────────────────────── HELPERS ──────────────────────────────//

    private bool TryPlayCard(PointerEventData eventData)
    {
        // Si se suelta sobre una DropZone (carril)
        if (eventData.pointerEnter && eventData.pointerEnter.CompareTag("DropZone"))
        {
            if (eventData.pointerEnter.TryGetComponent(out SpawnZone spawn))
                return GameManager.Instance.PlayCard(cardData, spawn.laneIndex, true); // Intenta jugar carta en carril.
        }

        // Si se suelta sobre un barco
        if (mainCamera &&
            Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit, 100f))
        {
            var ship = hit.collider.GetComponentInParent<Ship>();
            if (ship) return GameManager.Instance.PlayCardOnShip(cardData, ship, true); // Intenta jugar carta sobre barco.
        }

        return false; // No se pudo jugar la carta.
    }

    /*
    private IEnumerator ReplaceCardNextFrame(int slot)
    {
        yield return null; // Espera un frame para que Destroy() se procese.
        GameManager.Instance.cardManager.RemoveCardFromHand(slot); // Elimina carta de la mano.
    }
    */

    //──────────────────────── DOBLE CLIC ───────────────────────────//

    public void OnPointerClick(PointerEventData e)
    {
        if (e.clickCount == 2 && GameManager.Instance?.uiManager) // Si se hace doble clic...
            GameManager.Instance.uiManager.ShowCardDetail(cardData); // Muestra detalle de la carta.
    }
}
