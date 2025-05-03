using UnityEngine;

/// <summary>
/// Gestiona los carriles y el spawn de barcos.
/// Permite múltiples barcos por carril, pero mantiene un registro de barcos activos
/// para otras lógicas (combate, sink); el cooldown de jugar cartas lo controla GameManager.
/// </summary>
public class LaneManager : MonoBehaviour
{
    [Header("Configuración de Carriles")]
    public int laneCount = 3;
    public Transform[] playerLaneSpawns;  // Spawns para jugador
    public Transform[] enemyLaneSpawns;   // Spawns para IA

    [Header("Movimiento")]
    public float moveInterval = 3f;
    public float moveDistance = 5f;

    // Arrays para llevar el seguimiento de los barcos en cada carril
    [HideInInspector] public Ship[] playerShips;
    [HideInInspector] public Ship[] enemyShips;

    public static LaneManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Validar configuración de spawn points
        if (playerLaneSpawns.Length != laneCount ||
            enemyLaneSpawns.Length != laneCount)
        {
            Debug.LogWarning("LaneManager: la configuración de spawn points no coincide con laneCount.");
        }

        // Inicializar arrays de seguimiento de barcos
        playerShips = new Ship[laneCount];
        enemyShips = new Ship[laneCount];
    }

    /// <summary>
    /// Spawnea un barco en el carril indicado para jugador o IA.
    /// Registra la instancia en playerShips/enemyShips para cada carril.
    /// </summary>
    public void SpawnShip(GameObject shipPrefab, bool isPlayer, int laneIndex, int attack, int health)
    {
        // Validar índice
        if (laneIndex < 0 || laneIndex >= laneCount)
        {
            Debug.LogError($"LaneManager: carril inválido ({laneIndex}).");
            return;
        }

        // Prevenir dos barcos iniciales en un mismo carril
        if (isPlayer && playerShips[laneIndex] != null)
        {
            Debug.LogWarning($"LaneManager: carril {laneIndex} ya ocupado por un barco del jugador.");
            return;
        }
        if (!isPlayer && enemyShips[laneIndex] != null)
        {
            Debug.LogWarning($"LaneManager: carril {laneIndex} ya ocupado por un barco de la IA.");
            return;
        }

        // Determinar spawn y objetivo según bando
        Transform spawn = isPlayer
            ? playerLaneSpawns[laneIndex]
            : enemyLaneSpawns[laneIndex];
        Transform target = isPlayer
            ? enemyLaneSpawns[laneIndex]
            : playerLaneSpawns[laneIndex];

        // Instanciar y rotar hacia el objetivo
        Quaternion rot = Quaternion.LookRotation(target.position - spawn.position);
        GameObject go = Instantiate(shipPrefab, spawn.position, rot);

        // Inicializar componente Ship
        Ship ship = go.GetComponent<Ship>();
        if (ship != null)
        {
            ship.Initialize(
                playerTeam: isPlayer,
                baseAttack: attack,
                baseHealth: health,
                targetPos: target.position,
                moveInterval: moveInterval,
                moveDistance: moveDistance,
                laneIndex: laneIndex
            );

            // Registrar en el array correspondiente
            if (isPlayer) playerShips[laneIndex] = ship;
            else enemyShips[laneIndex] = ship;
        }
        else
        {
            Debug.LogError("LaneManager: el prefab no tiene componente Ship.");
            Destroy(go);
        }
    }

    private void OnDrawGizmos()
    {
        if (playerLaneSpawns == null || enemyLaneSpawns == null) return;
        int c = Mathf.Min(playerLaneSpawns.Length, enemyLaneSpawns.Length, laneCount);
        for (int i = 0; i < c; i++)
        {
            Transform p = playerLaneSpawns[i];
            Transform e = enemyLaneSpawns[i];
            if (p != null && e != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(p.position, p.position + (e.position - p.position).normalized * moveDistance);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(e.position, e.position + (p.position - e.position).normalized * moveDistance);
            }
        }
    }
}
