using UnityEngine;

/// <summary>
/// Controla los barcos en espera visibles en el spawn point.
/// </summary>
public class SpawnVisualManager : MonoBehaviour
{
    public static SpawnVisualManager Instance;

    [Header("Referencias de spawn")]
    public Transform[] playerSpawnPoints;
    public Transform[] enemySpawnPoints;

    // Barcos visibles en espera por carril
    private Ship[] playerVisualShips;
    private Ship[] enemyVisualShips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Asegurarnos de que LaneManager ya está inicializado
        int laneCount = LaneManager.Instance.laneCount;
        playerVisualShips = new Ship[laneCount];
        enemyVisualShips = new Ship[laneCount];
    }

    /// <summary>
    /// Crea o actualiza el barco en espera visual para el buffer dado.
    /// </summary>
    public void ShowOrUpdateShipBuffer(bool isPlayer, int laneIndex, SpawnBuffer buffer)
    {
        if (buffer == null
            || laneIndex < 0
            || laneIndex >= LaneManager.Instance.laneCount
            || buffer.IsEmpty)
        {
            return;
        }

        Ship[] visualArray = isPlayer ? playerVisualShips : enemyVisualShips;
        Transform[] spawnPts = isPlayer ? playerSpawnPoints : enemySpawnPoints;

        // Si no existe aún, instanciar un barco inmóvil
        if (visualArray[laneIndex] == null)
        {
            GameObject shipGO = Instantiate(
                buffer.shipPrefab,
                spawnPts[laneIndex].position,
                Quaternion.identity
            );
            Ship ship = shipGO.GetComponent<Ship>();
            ship.InitializePreview(
                playerTeam: isPlayer,
                attack: buffer.totalAttack,
                health: buffer.totalHealth,
                lane: laneIndex
            );
            visualArray[laneIndex] = ship;
        }
        else
        {
            // Si ya existe, solo actualizamos stats
            visualArray[laneIndex].SetStats(
                buffer.totalAttack,
                buffer.totalHealth,
                buffer.crewCount
            );
        }
    }

    /// <summary>
    /// Destruye el barco en espera visual de ese carril (por ejemplo al lanzarlo).
    /// </summary>
    public void RemoveVisualShip(bool isPlayer, int laneIndex)
    {
        Ship[] visualArray = isPlayer ? playerVisualShips : enemyVisualShips;
        if (laneIndex < 0 || laneIndex >= visualArray.Length) return;

        if (visualArray[laneIndex] != null)
        {
            Destroy(visualArray[laneIndex].gameObject);
            visualArray[laneIndex] = null;
        }
    }

    /// <summary>
    /// Devuelve el Ship visual en espera (o null si no hay).
    /// </summary>
    public Ship GetVisualShip(bool isPlayer, int laneIndex)
    {
        Ship[] visualArray = isPlayer ? playerVisualShips : enemyVisualShips;
        if (laneIndex < 0 || laneIndex >= visualArray.Length) return null;
        return visualArray[laneIndex];
    }
}
