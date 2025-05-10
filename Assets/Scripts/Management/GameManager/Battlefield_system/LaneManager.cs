using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona los carriles y el spawn de barcos.
/// Ahora permite múltiples barcos por carril.
/// </summary>
public class LaneManager : MonoBehaviour
{
    [Header("Configuración de Carriles")]
    public int laneCount = 3;
    public Transform[] playerLaneSpawns;
    public Transform[] enemyLaneSpawns;

    [Header("Movimiento")]
    public float moveInterval = 3f;
    public float moveDistance = 5f;

    public List<Ship>[] playerShips;
    public List<Ship>[] enemyShips;

    public static LaneManager Instance { get; private set; }

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
        if (playerLaneSpawns.Length != laneCount || enemyLaneSpawns.Length != laneCount)
        {
            Debug.LogWarning("LaneManager: la configuración de spawn points no coincide con laneCount.");
        }

        playerShips = new List<Ship>[laneCount];
        enemyShips = new List<Ship>[laneCount];
        for (int i = 0; i < laneCount; i++)
        {
            playerShips[i] = new List<Ship>();
            enemyShips[i] = new List<Ship>();
        }
    }

    /// <summary>
    /// Spawnea un barco con los valores dados, ya sea del jugador o la IA.
    /// </summary>
    public void SpawnShip(GameObject shipPrefab, bool isPlayer, int laneIndex, int attack, int health)
    {
        if (laneIndex < 0 || laneIndex >= laneCount)
        {
            Debug.LogError($"LaneManager: carril inválido ({laneIndex}).");
            return;
        }

        Transform spawn = isPlayer ? playerLaneSpawns[laneIndex] : enemyLaneSpawns[laneIndex];
        Transform target = isPlayer ? enemyLaneSpawns[laneIndex] : playerLaneSpawns[laneIndex];

        Quaternion rot = Quaternion.LookRotation(target.position - spawn.position);
        GameObject go = Instantiate(shipPrefab, spawn.position, rot);

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

            if (isPlayer) playerShips[laneIndex].Add(ship);
            else enemyShips[laneIndex].Add(ship);
        }
        else
        {
            Debug.LogError("LaneManager: el prefab no tiene componente Ship.");
            Destroy(go);
        }
    }

    /// <summary>
    /// Lanza el barco acumulado en el buffer del carril correspondiente.
    /// </summary>
    public void LaunchBufferedShip(int laneIndex, bool isPlayer)
    {
        GameManager.Instance.LaunchShipFromLane(laneIndex, isPlayer);
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
