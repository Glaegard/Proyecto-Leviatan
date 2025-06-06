﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class CardUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Referencias Minimalistas")]
    [SerializeField] private TextMeshProUGUI energyCostText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private Image cardImage;

    [Header("Datos de la Carta")]
    public CardData cardData;
    public int handSlotIndex;

    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Camera mainCamera;
    private Coroutine holdCoroutine;
    private bool pointerDown;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Inicializa la carta en la mano (minimalista): coste, ataque y defensa.
    /// </summary>
    public void Initialize(CardData data, int slot)
    {
        cardData = data;
        handSlotIndex = slot;

        if (energyCostText != null)
            energyCostText.text = data.energyCost.ToString();
        if (attackText != null)
            attackText.text = data.attack.ToString();
        if (defenseText != null)
            defenseText.text = data.defense.ToString();
        if (cardImage != null && data.previewSprite != null)
            cardImage.sprite = data.previewSprite;

        originalPosition = transform.position;
    }

    // ───────────────── Hold para detalle ─────────────────
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

    // ───────────────── Drag & Drop ───────────────────────
    public void OnBeginDrag(PointerEventData _)
    {
        pointerDown = false;
        if (holdCoroutine != null) StopCoroutine(holdCoroutine);

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

        // Si suelta en DropZone
        if (eventData.pointerEnter != null &&
            eventData.pointerEnter.CompareTag("DropZone") &&
            eventData.pointerEnter.TryGetComponent(out SpawnZone spawn))
        {
            bool played = GameManager.Instance.PlayCard(cardData, spawn.laneIndex, true);
            if (played)
            {
                StartCoroutine(ReplaceCardNextFrame());
                Destroy(gameObject);
                return;
            }
        }

        // Si no jugó, vuelve a su posición
        transform.position = originalPosition;
    }

    private IEnumerator ReplaceCardNextFrame()
    {
        yield return null;
        GameManager.Instance.cardManager.RemoveCardFromHand(handSlotIndex);
    }
}
