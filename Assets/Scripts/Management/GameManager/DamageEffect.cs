using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "Cards/Effects/DamageEffect")]
public class DamageEffect : CardEffect
{
    public int damageAmount = 5;

    public override void ApplyEffect(GameManager game, Ship targetShip = null, int targetLane = -1, bool isPlayer = true)
    {
        if (targetShip == null) return;
        targetShip.currentHealth -= damageAmount;
        targetShip.currentHealth = Mathf.Max(0, targetShip.currentHealth);
        targetShip.UpdateUI();
        if (targetShip.currentHealth == 0)
            LaneManager.Instance.DestroyShip(targetShip);
    }
}
