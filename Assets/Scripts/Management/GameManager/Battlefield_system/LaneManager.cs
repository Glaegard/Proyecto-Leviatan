using System.Collections.Generic;
using UnityEngine;

public class LaneManager : MonoBehaviour
{
    public static LaneManager Instance { get; private set; }

    [Header("Configuración")]
    public int laneCount = 3;
    public Transform[] playerLaneSpawns;
    public Transform[] enemyLaneSpawns;
    public float moveInterval = 3f;
    public float moveDistance = 5f;

    [HideInInspector] public List<Ship>[] playerShips;
    [HideInInspector] public List<Ship>[] enemyShips;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        playerShips = new List<Ship>[laneCount];
        enemyShips = new List<Ship>[laneCount];
        for (int i = 0; i < laneCount; i++)
        {
            playerShips[i] = new List<Ship>();
            enemyShips[i] = new List<Ship>();
        }
    }

    public void SpawnShip(GameObject shipPrefab, bool isPlayer, int laneIndex, int attack, int health)
    {
        var spawn = isPlayer ? playerLaneSpawns[laneIndex] : enemyLaneSpawns[laneIndex];
        var target = isPlayer ? enemyLaneSpawns[laneIndex] : playerLaneSpawns[laneIndex];
        var rot = Quaternion.LookRotation(target.position - spawn.position);

        var go = Instantiate(shipPrefab, spawn.position, rot);
        var ship = go.GetComponent<Ship>();
        if (ship != null)
        {
            ship.Initialize(isPlayer, attack, health,
                            target.position, moveInterval, moveDistance, laneIndex);
            RegisterShip(ship, laneIndex, isPlayer);
        }
    }

    public void RegisterShip(Ship ship, int laneIndex, bool isPlayer)
    {
        if (isPlayer) playerShips[laneIndex].Add(ship);
        else enemyShips[laneIndex].Add(ship);
    }

    public void DestroyShip(Ship ship)
    {
        int lane = ship.laneIndex;
        if (ship.isPlayer) playerShips[lane].Remove(ship);
        else enemyShips[lane].Remove(ship);
        Destroy(ship.gameObject);
    }

    public bool IsLaneEmpty(int laneIndex, bool isPlayer)
    {
        return isPlayer
            ? playerShips[laneIndex].Count == 0
            : enemyShips[laneIndex].Count == 0;
    }

    public void TransferShipToLane(Ship ship, int newLane)
    {
        int old = ship.laneIndex;
        var listOld = ship.isPlayer ? playerShips[old] : enemyShips[old];
        listOld.Remove(ship);

        ship.laneIndex = newLane;
        var listNew = ship.isPlayer ? playerShips[newLane] : enemyShips[newLane];
        listNew.Add(ship);

        ship.transform.position = (ship.isPlayer
            ? playerLaneSpawns[newLane]
            : enemyLaneSpawns[newLane]
        ).position;
    }

    public void LaunchBufferedShip(int laneIndex, bool isPlayer)
    {
        GameManager.Instance.LaunchShipFromLane(laneIndex, isPlayer);
    }
}
