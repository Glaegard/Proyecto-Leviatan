using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Estado de la partida")]
    public GameState currentGameState = GameState.Menu;

    [Header("Energía")]
    public int playerEnergy = 0;
    public int aiEnergy = 0;
    public int maxEnergy = 10;
    public float energyTickInterval = 2.2f;

    [Header("Cooldown (lanzamiento de barcos)")]
    public float marinerCooldown = 3f;
    public float[] lastPlayTimePerLane;

    [Header("Buffers de Spawn")]
    public SpawnBuffer[] playerSpawnBuffers;
    public SpawnBuffer[] enemySpawnBuffers;

    [Header("Referencias")]
    public CardManager cardManager;
    public UIManager uiManager;
    public PlayerController playerController;
    public AIController aiController;

    [Header("Eventos UI")]
    public UnityEvent<int> OnEnergyChanged;
    public UnityEvent OnBoardingHappened;
    public UnityEvent OnGameEnded;

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

        int lanes = LaneManager.Instance.laneCount;
        lastPlayTimePerLane = new float[lanes];
        for (int i = 0; i < lanes; i++) lastPlayTimePerLane[i] = -marinerCooldown;

        playerSpawnBuffers = new SpawnBuffer[lanes];
        enemySpawnBuffers = new SpawnBuffer[lanes];
        for (int i = 0; i < lanes; i++)
        {
            playerSpawnBuffers[i] = new SpawnBuffer();
            enemySpawnBuffers[i] = new SpawnBuffer();
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

            if (aiEnergy < maxEnergy)
                aiEnergy++;
        }
    }

    public bool PlayCard(CardData card, int lane, bool isPlayer)
    {
        if (!card || card.cardType != CardType.Marinero) return false;
        if (!LaneManager.Instance || lane < 0 || lane >= LaneManager.Instance.laneCount) return false;

        int energy = isPlayer ? playerEnergy : aiEnergy;
        if (energy < card.energyCost) return false;

        // Cooldown solo si se inicia un nuevo buffer
        SpawnBuffer[] buffers = isPlayer ? playerSpawnBuffers : enemySpawnBuffers;
        if (buffers[lane].IsEmpty && Time.time < lastPlayTimePerLane[lane] + marinerCooldown)
            return false;

        // Pagar energía
        if (isPlayer)
        {
            playerEnergy -= card.energyCost;
            OnEnergyChanged?.Invoke(playerEnergy);
        }
        else
        {
            aiEnergy -= card.energyCost;
        }

        // Agregar carta al buffer
        buffers[lane].AddCard(card);

        // Mostrar/actualizar barco en espera
        SpawnVisualManager.Instance?.ShowOrUpdateShipBuffer(isPlayer, lane, buffers[lane]);

        return true;
    }

    public bool LaunchShipFromLane(int lane, bool isPlayer)
    {
        SpawnBuffer[] buffers = isPlayer ? playerSpawnBuffers : enemySpawnBuffers;
        if (lane < 0 || lane >= buffers.Length) return false;
        if (buffers[lane].IsEmpty) return false;

        var buffer = buffers[lane];

        LaneManager.Instance.SpawnShip(
            buffer.shipPrefab,
            isPlayer,
            lane,
            buffer.totalAttack,
            buffer.totalHealth
        );

        // Eliminar barco en espera visual
        SpawnVisualManager.Instance?.RemoveVisualShip(isPlayer, lane);

        buffers[lane].Reset();
        lastPlayTimePerLane[lane] = Time.time;

        return true;
    }

    public bool PlayCardOnShip(CardData card, Ship target, bool isPlayer)
    {
        if (!card || !target) return false;

        int energy = isPlayer ? playerEnergy : aiEnergy;
        if (energy < card.energyCost) return false;

        if (isPlayer)
        {
            playerEnergy -= card.energyCost;
            OnEnergyChanged?.Invoke(playerEnergy);
        }
        else
        {
            aiEnergy -= card.energyCost;
        }

        switch (card.cardType)
        {
            case CardType.Marinero:
                ShipManager.Instance.AddCrew(target, card);
                break;
            case CardType.Maquinaria:
                ShipManager.Instance.AddEquipment(target, card);
                break;
            default:
                return false;
        }

        return true;
    }

    public void TriggerBoarding()
    {
        OnBoardingHappened?.Invoke();
        CheckVictoryCondition();
    }

    private void CheckVictoryCondition()
    {
        if (playerController && playerController.boardingCount >= 5)
            EndGame(true);
        if (aiController && aiController.boardingCount >= 5)
            EndGame(false);
    }

    public void EndGame(bool playerWon)
    {
        currentGameState = GameState.Ended;
        OnGameEnded?.Invoke();
        uiManager?.ShowGameResult(playerWon);
        Debug.Log($"Fin del juego — {(playerWon ? "¡GANA EL JUGADOR!" : "¡GANA LA IA!")}");

        Time.timeScale = 0f;
    }

    public void PauseGame() => currentGameState = GameState.Paused;
    public void ResumeGame() => currentGameState = GameState.Playing;
}
