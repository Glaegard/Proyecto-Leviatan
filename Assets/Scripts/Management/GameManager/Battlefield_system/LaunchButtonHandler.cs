using UnityEngine;

public class LaunchButtonHandler : MonoBehaviour
{
    public void LanzarLane0() => LaneManager.Instance.LaunchBufferedShip(0, true);
    public void LanzarLane1() => LaneManager.Instance.LaunchBufferedShip(1, true);
    public void LanzarLane2() => LaneManager.Instance.LaunchBufferedShip(2, true);
}
