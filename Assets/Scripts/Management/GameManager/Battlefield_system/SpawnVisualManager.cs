// File: SpawnVisualManager.cs

using UnityEngine;

/// <summary>
/// Muestra en UI los barcos en buffer (vista previa) por carril.
/// </summary>
public class SpawnVisualManager : MonoBehaviour
{
    public static SpawnVisualManager Instance;

    [Header("Puntos de Spawn UI")]
    public Transform[] playerSpawnPoints;  // Posiciones en pantalla para preview del jugador
    public Transform[] enemySpawnPoints;   // Posiciones en pantalla para preview de la IA

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
        int laneCount = LaneManager.Instance.laneCount;
        playerVisualShips = new Ship[laneCount];
        enemyVisualShips = new Ship[laneCount];
    }

    /// <summary>
    /// Crea o actualiza la vista previa de barco en buffer.
    /// </summary>
    public void ShowOrUpdateShipBuffer(bool isPlayer, int laneIndex, SpawnBuffer buffer)
    {
        if (buffer == null || buffer.IsEmpty) return;
        if (laneIndex < 0 || laneIndex >= playerVisualShips.Length) return;

        Ship[] visualArray = isPlayer ? playerVisualShips : enemyVisualShips;
        Transform[] spawnPts = isPlayer ? playerSpawnPoints : enemySpawnPoints;

        // Si no existe preview, instanciar
        if (visualArray[laneIndex] == null)
        {
            GameObject go = Instantiate(
                buffer.shipPrefab,
                spawnPts[laneIndex].position,
                Quaternion.identity
            );
            Ship ship = go.GetComponent<Ship>();
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
            // Si ya existe, actualizar stats
            visualArray[laneIndex].SetStats(
                buffer.totalAttack,
                buffer.totalHealth,
                buffer.crewCount
            );
        }
    }

    /// <summary>
    /// Elimina la vista previa al lanzar el barco.
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
    /// Retorna la Ship de preview si existe.
    /// </summary>
    public Ship GetVisualShip(bool isPlayer, int laneIndex)
    {
        Ship[] visualArray = isPlayer ? playerVisualShips : enemyVisualShips;
        if (laneIndex < 0 || laneIndex >= visualArray.Length) return null;
        return visualArray[laneIndex];
    }
}
