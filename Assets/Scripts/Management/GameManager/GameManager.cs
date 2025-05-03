using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Controlador principal del juego.</summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //──────────────────────── ESTADO ───────────────────────//
    [Header("Estado de la partida")]
    public GameState currentGameState = GameState.Menu;

    //──────────────────────── ENERGÍA ──────────────────────//
    [Header("Energía")]
    public int playerEnergy = 0;
    public int aiEnergy = 0;
    public int maxEnergy = 10;
    public float energyTickInterval = 2.2f; //esta a 2.2f de momento, se puede modificar desde el inspector cuando nos enfoquemos a gameplay

    [Header("Cooldown (jugadas de Marinero del jugador)")]
    public float marinerCooldown = 3f;        // 3s  de espera entre jugadas en el mismo carril para evitar solapamiento y bugs entre los barcos
    private float[] lastPlayTimePerLane;      // registra la última jugada del player por cada carril, para debuggin e implementaciones futuras de posibles nuevas mecanicas

    //──────────────────────── EVENTOS ──────────────────────//
    [Header("Eventos UI")]
    public UnityEvent<int> OnEnergyChanged;
    public UnityEvent OnBoardingHappened;
    public UnityEvent OnGameEnded;

    //──────────────────────── REFERENCIAS ──────────────────//
    [Header("Referencias")]
    public CardManager cardManager; // referencia al GameObject que contiene el script del cardManager
    public UIManager uiManager; // referencia al GameObject que contiene el script de UiManager
    public PlayerController playerController; // referencia al GameObject que contiene el script de PlayerController (por ahora innecesario pero ahi esta xd)
    public AIController aiController; // Referencia al GameObject que contiene el script de AiController

    //──────────────────────── SET-UP ───────────────────────//
    private void Awake()
    {
        // nos marcamos un singleton bien gucci para que no haya duplicaciones del GameManager y podamos llamar a funciones del GameManager desde otros script usando GameManager.Instance()
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Por defecto unity no permite destruir objetos con scripts declarados singletone, pero lo remarcamos en el awake() por si acaso por que #unity

        if (!cardManager) cardManager = FindObjectOfType<CardManager>(true); // condicional por si no se encuentra el cardmanager asignado manualmente desde el inspector, se asigna automaticamente con la funcion FindObjectOfType
    }

    private void Start()
    {
        currentGameState = GameState.Playing; // cuando se inicia partida se declara el currentGameState como Playing, se puede modificar a voluntad usando GameManager.Instance()
        playerEnergy = aiEnergy = 0; // inciamos la energia del juagdor e IA en 0

        if (LaneManager.Instance) // instanciamos el script LaneManager (Tambien declarado singleton -- ver LaneManager para mas info)
        {
            int lanes = LaneManager.Instance.laneCount; // creamos variable de enetero para establecer las lanes (el numero de lanes esta definido en el script de LaneManager)
            lastPlayTimePerLane = new float[lanes]; // registramos los movimientos en la variable lastPlayTimePerLane
            for (int i = 0; i < lanes; i++) lastPlayTimePerLane[i] = -marinerCooldown; // bucle que aplica la logica del cooldown
        }

        StartCoroutine(EnergyRoutine()); // inciiamos co-rutina en la funcion EnergyRoutine
        aiController?.StartAI(); // inciamos la IA
    }

    private IEnumerator EnergyRoutine() // Define una corrutina privada llamada EnergyRoutine que devuelve un IEnumerator.
    {
        while (currentGameState == GameState.Playing) // Mientras el estado actual del juego sea "Playing", se ejecuta el bucle.
        {
            yield return new WaitForSeconds(energyTickInterval); // Espera un intervalo de tiempo (energyTickInterval) antes de continuar con la ejecución.

            if (playerEnergy < maxEnergy) // Si la energía del jugador es menor que la energía máxima...
            {
                playerEnergy++; // ...aumenta la energía del jugador en 1 unidad.
                OnEnergyChanged?.Invoke(playerEnergy); // Llama al evento OnEnergyChanged (si hay suscriptores) y le pasa el nuevo valor de energía.
            }

            if (aiEnergy < maxEnergy) // Si la energía de la IA es menor que su energía máxima...
                aiEnergy++; // ...aumenta la energía de la IA en 1 unidad.
        }
    }

    //────────────────────── UTILIDADES ─────────────────────//

    private bool LaneIsFree(bool isPlayer, int lane) // Comprueba si un carril está libre para un jugador o la IA.
    {
        if (!LaneManager.Instance) return false; // Si no existe el LaneManager, devuelve false por seguridad.
        return isPlayer
            ? LaneManager.Instance.playerShips[lane] == null // Si es jugador, revisa si no hay barco en ese carril.
            : LaneManager.Instance.enemyShips[lane] == null; // Si es IA, revisa si su carril está libre.
    }

    private Ship GetOwnShipInLane(bool isPlayer, int lane) // Obtiene el barco propio en un carril específico.
    {
        if (!LaneManager.Instance) return null; // Si no existe el LaneManager, devuelve null.
        return isPlayer
            ? LaneManager.Instance.playerShips[lane] // Si es jugador, devuelve el barco del jugador.
            : LaneManager.Instance.enemyShips[lane]; // Si es IA, devuelve el barco de la IA.
    }

    //──────────────────────── CARTAS ───────────────────────//

    /// <summary>
    /// Juega un Marinero: crea barco si el carril está libre
    /// o añade tripulación si ya hay barco propio.  
    /// Devuelve true solo si la acción se aplica y se descuenta energía.
    /// </summary>
    public bool PlayCard(CardData card, int lane, bool isPlayer) // Lógica principal para jugar una carta tipo Marinero.
    {
        if (!card || card.cardType != CardType.Marinero) return false; // Si la carta no existe o no es Marinero, devuelve false.
        if (!LaneManager.Instance || lane < 0 || lane >= LaneManager.Instance.laneCount) return false; // Verifica que el carril es válido.

        int energy = isPlayer ? playerEnergy : aiEnergy; // Asigna la energía según quién juegue.
        if (energy < card.energyCost) return false; // Si no hay suficiente energía, cancela la acción.

        //── Cooldown SOLO para el jugador
        if (isPlayer && Time.time < lastPlayTimePerLane[lane] + marinerCooldown) return false; // Si el jugador está en cooldown, cancela.

        bool actionApplied = false; // Indica si la acción se ha aplicado correctamente.

        if (LaneIsFree(isPlayer, lane)) // Si el carril está libre...
        {
            // Carril libre → crear barco
            LaneManager.Instance.SpawnShip(card.shipPrefab, isPlayer, lane, card.attack, card.defense); // Crea un barco con los datos de la carta.
            actionApplied = true; // Marca la acción como aplicada.
        }
        else
        {
            // Carril ocupado → si el barco es propio, añadir tripulación
            Ship ownShip = GetOwnShipInLane(isPlayer, lane); // Obtiene el barco propio en el carril.
            if (ownShip) // Si existe (y por tanto no es enemigo)...
            {
                ShipManager.Instance.AddCrew(ownShip, card); // Añade la carta como tripulación al barco.
                actionApplied = true; // Marca la acción como aplicada.
            }
        }

        if (!actionApplied) return false; // Si no se hizo nada (p.ej. carril ocupado por enemigo), cancela.

        //── Consumir energía SOLO cuando se aplica la acción
        if (isPlayer)
        {
            playerEnergy -= card.energyCost; // Resta energía al jugador.
            OnEnergyChanged?.Invoke(playerEnergy); // Notifica el cambio de energía.
            lastPlayTimePerLane[lane] = Time.time; // Aplica cooldown para ese carril.
        }
        else
            aiEnergy -= card.energyCost; // Resta energía a la IA.

        return true; // La acción se ejecutó correctamente.
    }

    /// <summary>Aplica Maquinaria o Marinero sobre un barco existente (añade crew/equipo).</summary>
    public bool PlayCardOnShip(CardData card, Ship target, bool isPlayer) // Juega una carta directamente sobre un barco.
    {
        if (!card || !target) return false; // Si la carta o el objetivo no existen, cancela.

        int energy = isPlayer ? playerEnergy : aiEnergy; // Asigna la energía según quién juega.
        if (energy < card.energyCost) return false; // Si no hay energía suficiente, cancela.

        // Consumir energía
        if (isPlayer)
        {
            playerEnergy -= card.energyCost; // Resta energía al jugador.
            OnEnergyChanged?.Invoke(playerEnergy); // Notifica el cambio.
        }
        else
        {
            aiEnergy -= card.energyCost; // Resta energía a la IA.
        }

        switch (card.cardType) // Aplica el efecto según el tipo de carta.
        {
            case CardType.Marinero: ShipManager.Instance.AddCrew(target, card); break; // Añade tripulación.
            case CardType.Maquinaria: ShipManager.Instance.AddEquipment(target, card); break; // Añade equipo.
            default: return false; // Si no es una carta válida para esta función, cancela.
        }

        return true; // Acción realizada correctamente.
    }

    //───────────────── Abordajes / Fin de juego ────────────//

    public void TriggerBoarding() // Se llama cuando ocurre un abordaje.
    {
        OnBoardingHappened?.Invoke(); // Dispara el evento de abordaje.
        CheckVictoryCondition(); // Revisa si se ha ganado la partida.
    }

    private void CheckVictoryCondition() // Comprueba si alguien ha ganado.
    {
        if (playerController && playerController.boardingCount >= 5) EndGame(true); // Si el jugador ha hecho 5 abordajes, gana.
        if (aiController && aiController.boardingCount >= 5) EndGame(false); // Si la IA ha hecho 5 abordajes, gana.
    }

    public void EndGame(bool playerWon) // Finaliza la partida.
    {
        currentGameState = GameState.Ended; // Cambia el estado a "Terminado".
        OnGameEnded?.Invoke(); // Dispara el evento de fin de partida.
        uiManager?.ShowGameResult(playerWon); // Muestra el resultado en pantalla.
        Debug.Log($"Fin del juego — {(playerWon ? "¡GANA EL JUGADOR!" : "¡GANA LA IA!")}"); // Muestra un mensaje en la consola.
    }

    //──────────────────────── PAUSA ────────────────────────//

    public void PauseGame() => currentGameState = GameState.Paused; // Pone el juego en pausa.
    public void ResumeGame() => currentGameState = GameState.Playing; // Reanuda el juego.

}
