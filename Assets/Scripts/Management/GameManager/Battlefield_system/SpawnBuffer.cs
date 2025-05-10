using UnityEngine;

[System.Serializable]
public class SpawnBuffer
{
    public GameObject shipPrefab;
    public int totalAttack;
    public int totalHealth;
    public int crewCount;

    public void AddCard(CardData card)
    {
        if (shipPrefab == null)
            shipPrefab = card.shipPrefab;

        totalAttack += card.attack;
        totalHealth += card.defense;
        crewCount++;
    }

    public bool IsEmpty => crewCount == 0;

    public void Reset()
    {
        shipPrefab = null;
        totalAttack = 0;
        totalHealth = 0;
        crewCount = 0;
    }
}
