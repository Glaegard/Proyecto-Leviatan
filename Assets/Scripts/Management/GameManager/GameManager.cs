using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Estados posibles de la partida.
/// </summary>
public enum GameState { Menu, Playing, Paused, Ended }

/// <summary>
/// Controlador global de la partida.
/// Permanece entre escenas y se auto-inicializa según la escena actual.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Estado y Energía")]
    public GameState currentGameState = GameState.Menu;
    public int playerEnergy;
    public int aiEnergy;
    public int maxEnergy = 10;
    public float energyTickInterval = 2.2f;
    public UnityEvent<int> OnEnergyChanged;

    [Header("Buffers y Cooldown")]
    public float marinerCooldown = 3f;
    public float[] lastPlayTimePerLane;
    public SpawnBuffer[] playerSpawnBuffers;
    public SpawnBuffer[] enemySpawnBuffers;

    [Header("Abordajes y Victoria")]
    public int boardingsToWin = 5;
    public PlayerController playerController;
    public AIController aiController;
    public UnityEvent OnBoardingHappened;
    public UnityEvent<bool> OnGameEnded;

    [Header("UI y Cartas")]
    public UIManager uiManager;
    public CardManager cardManager;

    [Header("Raycast System")]
    [Tooltip("Arrastra aquí el GameObject que gestiona los raycasts de la UI (por ejemplo tu panel bloqueador)")]
    public GameObject raycastSystem;

    private void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Desactivar VSync y forzar 60 FPS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Suscribir al callback de carga de escenas
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            currentGameState = GameState.Playing;
            InitializeGameScene();
        }
        else
        {
            currentGameState = GameState.Menu;
        }
    }

    private void InitializeGameScene()
    {
        // 1) Referencias de componentes en la escena "Game"
        uiManager = FindObjectOfType<UIManager>();
        cardManager = FindObjectOfType<CardManager>();
        aiController = FindObjectOfType<AIController>();
        playerController = FindObjectOfType<PlayerController>();

        // 2) RaycastSystem: si no está asignado en Inspector, lo buscamos por nombre o tag
        if (raycastSystem == null)
        {
            raycastSystem = GameObject.FindWithTag("NewRaycastSystem");
        }

        // 3) Inicializar energía y notificar
        playerEnergy = aiEnergy = 0;
        OnEnergyChanged?.Invoke(playerEnergy);

        // 4) Buffers y cooldown por carril
        int lanes = LaneManager.Instance.laneCount;
        lastPlayTimePerLane = new float[lanes];
        playerSpawnBuffers = new SpawnBuffer[lanes];
        enemySpawnBuffers = new SpawnBuffer[lanes];
        for (int i = 0; i < lanes; i++)
        {
            lastPlayTimePerLane[i] = -marinerCooldown;
            playerSpawnBuffers[i] = new SpawnBuffer();
            enemySpawnBuffers[i] = new SpawnBuffer();
        }

        // 5) Eventos
        if (OnBoardingHappened == null) OnBoardingHappened = new UnityEvent();
        if (OnGameEnded == null) OnGameEnded = new UnityEvent<bool>();
        if (uiManager != null) OnGameEnded.AddListener(uiManager.ShowGameResult);

        // 6) Iniciar energía y IA
        StartCoroutine(EnergyRoutine());
        aiController?.StartAI();

        // 7) Música de juego
        AudioManager.Instance?.Play("music");
    }

    private IEnumerator EnergyRoutine()
    {
        while (currentGameState == GameState.Playing)
        {
            yield return new WaitForSeconds(energyTickInterval);
            if (playerEnergy < maxEnergy)
                OnEnergyChanged?.Invoke(++playerEnergy);
            if (aiEnergy < maxEnergy)
                aiEnergy++;
        }
    }

    /// <summary>
    /// Intenta jugar una carta (solo en Playing).
    /// </summary>
    public bool TryPlayCard(CardData card, int laneIndex, Ship targetShip, bool isPlayer)
    {
        if (currentGameState != GameState.Playing || card == null)
            return false;

        int energy = isPlayer ? playerEnergy : aiEnergy;
        if (energy < card.energyCost)
        {
            AudioManager.Instance?.Play("error");
            return false;
        }

        // Descontar energía
        if (isPlayer)
        {
            playerEnergy -= card.energyCost;
            OnEnergyChanged?.Invoke(playerEnergy);
        }
        else aiEnergy -= card.energyCost;

        // Marinero → buffer si no hay targetShip
        if (card.cardType == CardType.Marinero && targetShip == null)
        {
            var buffers = isPlayer ? playerSpawnBuffers : enemySpawnBuffers;

            if (buffers[laneIndex].IsEmpty &&
                Time.time < lastPlayTimePerLane[laneIndex] + marinerCooldown)
                return false;

            if (buffers[laneIndex].IsEmpty)
                lastPlayTimePerLane[laneIndex] = Time.time;

            buffers[laneIndex].AddCard(card);
            SpawnVisualManager.Instance?.ShowOrUpdateShipBuffer(isPlayer, laneIndex, buffers[laneIndex]);
            return true;
        }

        // Cartas sobre un barco
        if (targetShip != null)
        {
            switch (card.cardType)
            {
                case CardType.Marinero:
                case CardType.Capitan:
                    ShipManager.Instance.AddCrew(targetShip, card);
                    return true;
                case CardType.Artefacto:
                    ShipManager.Instance.AddEquipment(targetShip, card);
                    return true;
                case CardType.Maniobra:
                    card.effect?.ApplyEffect(this, targetShip, laneIndex, isPlayer);
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Lanza el barco desde el buffer (solo en Playing).
    /// </summary>
    public bool LaunchShipFromLane(int laneIndex, bool isPlayer)
    {
        if (currentGameState != GameState.Playing)
            return false;

        var buffers = isPlayer ? playerSpawnBuffers : enemySpawnBuffers;
        if (buffers[laneIndex].IsEmpty)
            return false;

        LaneManager.Instance.SpawnShip(
            buffers[laneIndex].shipPrefab,
            isPlayer,
            laneIndex,
            buffers[laneIndex].totalAttack,
            buffers[laneIndex].totalHealth
        );

        SpawnVisualManager.Instance?.RemoveVisualShip(isPlayer, laneIndex);
        buffers[laneIndex].Reset();
        lastPlayTimePerLane[laneIndex] = Time.time;
        return true;
    }

    /// <summary>
    /// Registra un abordaje y comprueba victoria (solo en Playing).
    /// </summary>
    public void TriggerBoarding(bool isPlayer)
    {
        if (currentGameState != GameState.Playing)
            return;

        if (isPlayer) playerController.boardingCount++;
        else aiController.boardingCount++;

        OnBoardingHappened?.Invoke();
        CheckVictoryCondition();
    }

    private void CheckVictoryCondition()
    {
        if (playerController != null && playerController.boardingCount >= boardingsToWin)
        {
            EndGame(true);
        }
        else if (aiController != null && aiController.boardingCount >= boardingsToWin)
        {
            EndGame(false);
        }
    }

    /// <summary>
    /// Finaliza la partida, desactiva raycastSystem y dispara evento de fin.
    /// </summary>
    public void EndGame(bool playerWon)
    {
        currentGameState = GameState.Ended;

        // Desactivar sistema de bloqueo de raycasts para los botones del panel de resultado
        if (raycastSystem != null)
            raycastSystem.SetActive(false);

        // Detener música y mostrar resultado
        AudioManager.Instance?.Stop("music");
        OnGameEnded?.Invoke(playerWon);
    }

    /// <summary>
    /// Pausa el juego (solo si estamos en Playing).
    /// </summary>
    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
            currentGameState = GameState.Paused;
    }

    /// <summary>
    /// Reanuda el juego (solo si estamos en Paused).
    /// </summary>
    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused)
            currentGameState = GameState.Playing;
    }
}
