using System.Collections.Generic;       // Importa el uso de listas genéricas.
using UnityEngine;                      // Necesario para clases y funciones de Unity.

//────────────────────────────────────────────────────────────//
/// <summary>
/// Reparte TODO el mazo entre los slots al empezar.
/// No hay robo ni descarte automático: simplificación total.
/// </summary>
public class CardManager : MonoBehaviour // Clase principal que gestiona la repartición de cartas al iniciar.
{
    //────────────────── CONFIG ──────────────────//

    [Header("Mazo del Jugador")]
    public CardDeck deckAsset;           // ScriptableObject que contiene todas las cartas del mazo.

    [Header("Slots")]
    public int handSize = 5;             // Número de ranuras (slots) en la mano.

    [Tooltip("Arrastra aquí los paneles-slot en orden 0-N.")]
    public Transform[] slotPanels;       // Array de paneles visuales donde se colocan las cartas.

    [Header("Prefabs y UI")]
    public GameObject cardUIPrefab;      // Prefab de la carta en la interfaz, debe tener un componente CardUI.

    // Datos en tiempo real (sólo para inspección / debug)
    [SerializeField] private List<CardData>[] cardsBySlot; // Lista de cartas actual en cada slot.

    //────────────────── INICIO ───────────────────//

    private void Start() // Función que se ejecuta al iniciar el juego.
    {
        // Validaciones iniciales
        if (!deckAsset) { Debug.LogError("CardManager: deckAsset no asignado."); return; } // Error si no hay mazo.
        if (!cardUIPrefab) { Debug.LogError("CardManager: cardUIPrefab no asignado."); return; } // Error si no hay prefab.
        if (slotPanels == null || slotPanels.Length != handSize) // Error si los slots no coinciden con el tamaño de mano.
        {
            Debug.LogError("CardManager: slotPanels incompleto.");
            return;
        }

        // Preparar contenedores para cada slot
        cardsBySlot = new List<CardData>[handSize]; // Inicializa el array.
        for (int i = 0; i < handSize; i++) cardsBySlot[i] = new List<CardData>(); // Inicializa cada lista individualmente.

        DistributeAllCards(); // Reparte todas las cartas.
    }

    //──────────────── DISTRIBUCIÓN ───────────────//

    private void DistributeAllCards() // Función que reparte todas las cartas entre los slots.
    {
        List<CardData> fullDeck = new List<CardData>(deckAsset.cards); // Crea una copia del mazo completo.
        int total = fullDeck.Count; // Número total de cartas.

        // Calcula cuántas cartas debe tener cada slot
        int baseCount = total / handSize; // Cantidad base de cartas por slot.
        int extraSlots = total % handSize; // Cantidad de slots que recibirán una carta extra.

        int index = 0; // Índice para recorrer el mazo.

        for (int slot = 0; slot < handSize; slot++) // Recorre cada slot.
        {
            int cardsInThisSlot = baseCount + (slot < extraSlots ? 1 : 0); // Calcula cuántas cartas van a este slot.

            for (int j = 0; j < cardsInThisSlot; j++) // Reparte cartas para este slot.
            {
                CardData card = fullDeck[index++]; // Toma la siguiente carta del mazo.
                cardsBySlot[slot].Add(card); // Añade la carta a la lista correspondiente.
                InstantiateCardUI(card, slot, j); // Instancia su UI.
            }
        }

        Debug.Log($"CardManager: repartidas {total} cartas entre {handSize} slots."); // Mensaje de depuración.
    }

    //──────────────── INSTANCIAR UI ──────────────//

    private void InstantiateCardUI(CardData data, int slot, int stackIndex) // Crea la carta en pantalla y la posiciona visualmente.
    {
        Transform parent = slotPanels[slot]; // Obtiene el contenedor visual del slot.
        if (!parent) { Debug.LogError($"Slot {slot} sin Transform."); return; } // Error si no hay panel.

        GameObject ui = Instantiate(cardUIPrefab, parent, false); // Crea una instancia del prefab en el slot.
        ui.name = data.cardName; // Renombra el objeto con el nombre de la carta.

        // Apilar ligeramente las cartas con un desplazamiento en Y
        float yOffset = -30f * stackIndex; // Desplazamiento vertical entre cartas.
        RectTransform rt = ui.GetComponent<RectTransform>(); // Obtiene su RectTransform.
        if (rt)
        {
            rt.anchorMin = Vector2.zero; // Ancla la carta a los bordes del panel.
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(0, yOffset); // Aplica desplazamiento inferior.
            rt.offsetMax = new Vector2(0, yOffset); // Aplica desplazamiento superior.
        }

        // Inicializa el componente CardUI si existe
        if (ui.TryGetComponent(out CardUI cui))
        {
            cui.Initialize(data); // Asocia los datos de la carta.
            cui.handSlotIndex = slot; // Guarda el índice del slot al que pertenece.
        }
    }

    //──────────────── DEBUG OPCIONAL ─────────────//

    [ContextMenu("Debug Hand Content")] // Permite ejecutar esta función desde el inspector de Unity.
    public void DebugHand() // Muestra por consola las cartas de cada slot.
    {
        for (int i = 0; i < handSize; i++) // Recorre todos los slots.
        {
            string list = string.Join(", ", cardsBySlot[i].ConvertAll(c => c.cardName)); // Convierte los nombres de las cartas a texto.
            Debug.Log($"slot_{i}: {list}"); // Muestra la lista de nombres en consola.
        }
    }
}
