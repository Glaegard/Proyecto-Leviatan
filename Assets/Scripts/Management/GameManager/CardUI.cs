using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class CardUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("UI Minimalista")]
    public TextMeshProUGUI energyCostText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public Image cardImage;

    [HideInInspector] public CardData cardData;
    [HideInInspector] public int handSlotIndex;

    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Coroutine holdCoroutine;
    private bool pointerDown;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(CardData data, int slot)
    {
        cardData = data;
        handSlotIndex = slot;
        energyCostText.text = data.energyCost.ToString();
        attackText.text = data.attack.ToString();
        defenseText.text = data.defense.ToString();
        cardImage.sprite = data.previewSprite;
        originalPosition = transform.position;
    }

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
        if (pointerDown) GameManager.Instance.uiManager.ShowCardDetail(cardData);
    }

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

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        var hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null && hit.collider.TryGetComponent<SpawnZone>(out var zone))
        {
            bool played = GameManager.Instance.TryPlayCard(
                cardData, zone.laneIndex, null, true
            );
            if (played)
            {
                LaneManager.Instance.SpawnShip(
                    cardData.shipPrefab,
                    true,
                    zone.laneIndex,
                    cardData.attack,
                    cardData.defense
                );
                StartCoroutine(ReplaceCardNextFrame());
                Destroy(gameObject);
                return;
            }
        }

        transform.position = originalPosition;
    }

    private IEnumerator ReplaceCardNextFrame()
    {
        yield return null;
        GameManager.Instance.cardManager.RemoveCardFromHand(handSlotIndex);
    }
}
