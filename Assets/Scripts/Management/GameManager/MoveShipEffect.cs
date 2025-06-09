using UnityEngine;

[CreateAssetMenu(fileName = "MoveShipEffect", menuName = "Cards/Effects/MoveShipEffect")]
public class MoveShipEffect : CardEffect
{
    public int laneOffset = 1;

    public override void ApplyEffect(GameManager game, Ship targetShip = null, int targetLane = -1, bool isPlayer = true)
    {
        if (targetShip == null) return;
        int origin = targetShip.laneIndex;
        int dest = origin + laneOffset;
        if (dest < 0 || dest >= LaneManager.Instance.laneCount) return;
        if (!LaneManager.Instance.IsLaneEmpty(dest, targetShip.isPlayer)) return;
        LaneManager.Instance.TransferShipToLane(targetShip, dest);
    }
}
