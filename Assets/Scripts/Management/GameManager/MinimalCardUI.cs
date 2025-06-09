// File: MinimalCardUI.cs

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI minimalista de carta: drag & drop + doble-click para mostrar/ocultar detalle.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class MinimalCardUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler
{
    [Header("Referencias UI")]
    public TextMeshProUGUI energyCostText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public Image cardImage;

    [HideInInspector] public CardData cardData;
    [HideInInspector] public int slotIndex;

    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;

    // Para detectar doble-click sobre esta carta
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f; // segundos

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Inicializa la carta en el slot indicado.
    /// </summary>
    public void Initialize(CardData data, int slot)
    {
        cardData = data;
        slotIndex = slot;
        originalPosition = transform.position;

        energyCostText.text = data.energyCost.ToString();
        attackText.text = data.attack.ToString();
        defenseText.text = data.defense.ToString();
        if (data.previewSprite != null)
            cardImage.sprite = data.previewSprite;
    }

    /// <summary>
    /// Devuelve la carta a su posición original si la jugada falla.
    /// </summary>
    public void ReturnToSlot()
    {
        transform.position = originalPosition;
        canvasGroup.blocksRaycasts = true;
    }

    // ─────────────── Drag & Drop ────────────────

    public void OnBeginDrag(PointerEventData _)
    {
        canvasGroup.blocksRaycasts = false; // permitir drop zone
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += (Vector3)eventData.delta;
    }

    public void OnEndDrag(PointerEventData _)
    {
        canvasGroup.blocksRaycasts = true;
        // Si no se jugó, volvemos al slot
        transform.position = originalPosition;
    }

    // ───────────── Detección de Doble-Click ─────────────

    public void OnPointerDown(PointerEventData eventData)
    {
        float timeSinceLast = Time.unscaledTime - lastClickTime;
        if (timeSinceLast <= doubleClickThreshold)
        {
            // Doble-click: alternar panel de detalle
            if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            {
                var uiMgr = GameManager.Instance.uiManager;
                if (uiMgr.cardDetailPanel.activeSelf)
                    uiMgr.HideCardDetail();
                else
                    uiMgr.ShowCardDetail(cardData);
            }
        }
        lastClickTime = Time.unscaledTime;
    }
}
