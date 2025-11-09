using UnityEngine;

public class MapSpace : MonoBehaviour
{
  [SerializeField] SpaceData data;
  string areaName;
  Color _backgroundColor;
  public Color backgroundColor => _backgroundColor;

  PlayerShip _ship;
  public PlayerShip ship => _ship;

  [SerializeField] MapSpace[] connectedSpaces;

  void Awake()
  {
    areaName = data.AreaName;
    _backgroundColor = data.BackgroundColor;
  }

  public void EnterPlayer(PlayerShip ship)
  {
    _ship = ship;
  }

  public void ExitPlayer()
  {
    _ship = null;
  }
}
