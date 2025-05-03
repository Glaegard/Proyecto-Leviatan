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
    public float energyTickInterval = 2.2f;

    [Header("Cooldown (jugadas de Marinero del jugador)")]
    public float marinerCooldown = 5f;        // ← NUEVO: 5 s entre jugadas en el mismo carril
    private float[] lastPlayTimePerLane;      // registra la última jugada por carril (sólo jugador)

    //──────────────────────── EVENTOS ──────────────────────//
    [Header("Eventos UI")]
    public UnityEvent<int> OnEnergyChanged;
    public UnityEvent OnBoardingHappened;
    public UnityEvent OnGameEnded;

    //──────────────────────── REFERENCIAS ──────────────────//
    [Header("Referencias")]
    public CardManager cardManager;
    public UIManager uiManager;
    public PlayerController playerController;
    public AIController aiController;

    //──────────────────────── SET-UP ───────────────────────//
    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!cardManager) cardManager = FindObjectOfType<CardManager>(true);
    }

    private void Start()
    {
        currentGameState = GameState.Playing;
        playerEnergy = aiEnergy = 0;

        if (LaneManager.Instance)
        {
            int lanes = LaneManager.Instance.laneCount;
            lastPlayTimePerLane = new float[lanes];
            for (int i = 0; i < lanes; i++) lastPlayTimePerLane[i] = -marinerCooldown;
        }

        StartCoroutine(EnergyRoutine());
        aiController?.StartAI();
    }

    private IEnumerator EnergyRoutine()
    {
        while (currentGameState == GameState.Playing)
        {
            yield return new WaitForSeconds(energyTickInterval);

            if (playerEnergy < maxEnergy)
            {
                playerEnergy++;
                OnEnergyChanged?.Invoke(playerEnergy);
            }
            if (aiEnergy < maxEnergy) aiEnergy++;
        }
    }

    //────────────────────── UTILIDADES ─────────────────────//
    private bool LaneIsFree(bool isPlayer, int lane)
    {
        if (!LaneManager.Instance) return false;
        return isPlayer
            ? LaneManager.Instance.playerShips[lane] == null
            : LaneManager.Instance.enemyShips[lane] == null;
    }

    private Ship GetOwnShipInLane(bool isPlayer, int lane)
    {
        if (!LaneManager.Instance) return null;
        return isPlayer
            ? LaneManager.Instance.playerShips[lane]
            : LaneManager.Instance.enemyShips[lane];
    }

    //──────────────────────── CARTAS ───────────────────────//
    /// <summary>
    /// Juega un Marinero: crea barco si el carril está libre
    /// o añade tripulación si ya hay barco propio.  
    /// Devuelve true solo si la acción se aplica y se descuenta energía.
    /// </summary>
    public bool PlayCard(CardData card, int lane, bool isPlayer)
    {
        if (!card || card.cardType != CardType.Marinero) return false;
        if (!LaneManager.Instance || lane < 0 || lane >= LaneManager.Instance.laneCount) return false;

        int energy = isPlayer ? playerEnergy : aiEnergy;
        if (energy < card.energyCost) return false;

        //── Cooldown SOLO para el jugador
        if (isPlayer && Time.time < lastPlayTimePerLane[lane] + marinerCooldown) return false;

        bool actionApplied = false;

        if (LaneIsFree(isPlayer, lane))
        {
            // Carril libre → crear barco
            LaneManager.Instance.SpawnShip(card.shipPrefab, isPlayer, lane, card.attack, card.defense);
            actionApplied = true;
        }
        else
        {
            // Carril ocupado → si el barco es propio, añadir tripulación
            Ship ownShip = GetOwnShipInLane(isPlayer, lane);
            if (ownShip)                       // si es NULL = barco enemigo, jugada inválida
            {
                ShipManager.Instance.AddCrew(ownShip, card);
                actionApplied = true;
            }
        }

        if (!actionApplied) return false;      // jugada inválida (p.ej. carril ocupado por enemigo)

        //── Consumir energía SOLO cuando se aplica la acción
        if (isPlayer)
        {
            playerEnergy -= card.energyCost;
            OnEnergyChanged?.Invoke(playerEnergy);
            lastPlayTimePerLane[lane] = Time.time;     // actualizar cooldown de ese carril
        }
        else
            aiEnergy -= card.energyCost;

        return true;
    }

    /// <summary>Aplica Maquinaria o Marinero sobre un barco existente (añade crew/equipo).</summary>
    public bool PlayCardOnShip(CardData card, Ship target, bool isPlayer)
    {
        if (!card || !target) return false;

        int energy = isPlayer ? playerEnergy : aiEnergy;
        if (energy < card.energyCost) return false;

        // Consumir energía
        if (isPlayer) { playerEnergy -= card.energyCost; OnEnergyChanged?.Invoke(playerEnergy); }
        else { aiEnergy -= card.energyCost; }

        switch (card.cardType)
        {
            case CardType.Marinero: ShipManager.Instance.AddCrew(target, card); break;
            case CardType.Maquinaria: ShipManager.Instance.AddEquipment(target, card); break;
            default: return false;
        }
        return true;
    }

    //───────────────── Abordajes / Fin de juego ────────────//
    public void TriggerBoarding()
    {
        OnBoardingHappened?.Invoke();
        CheckVictoryCondition();
    }

    private void CheckVictoryCondition()
    {
        if (playerController && playerController.boardingCount >= 5) EndGame(true);
        if (aiController && aiController.boardingCount >= 5) EndGame(false);
    }

    public void EndGame(bool playerWon)
    {
        currentGameState = GameState.Ended;
        OnGameEnded?.Invoke();
        uiManager?.ShowGameResult(playerWon);
        Debug.Log($"Fin del juego — {(playerWon ? "¡GANA EL JUGADOR!" : "¡GANA LA IA!")}");
    }

    //──────────────────────── PAUSA ────────────────────────//
    public void PauseGame() => currentGameState = GameState.Paused;
    public void ResumeGame() => currentGameState = GameState.Playing;
}
