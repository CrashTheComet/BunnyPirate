using UnityEngine;

public class MapSpace : MonoBehaviour
{
  [SerializeField] SpaceData data;
  [SerializeField] GameObject _selectionIndicator;
  [SerializeField] GameObject _playerIndicator;

  string _spaceName;
  public string spaceName => _spaceName;
  Color _backgroundColor;
  public Color backgroundColor => _backgroundColor;

  PlayerShip _ship;
  public PlayerShip ship => _ship;

  GameMap _gameMap;
  public GameMap GameMap => _gameMap;

  [SerializeField] MapSpace[] connectedSpaces;

  public void Initialize(GameMap map)
  {
    _spaceName = data.AreaName;
    _backgroundColor = data.BackgroundColor;
    _gameMap = map;

    GetComponent<SpriteRenderer>().color = _backgroundColor;
  }

  public void EnterPlayer(PlayerShip ship)
  {
    _ship = ship;
  }

  public void ExitPlayer()
  {
    _ship = null;
  }

  public void ShowSelectionIndicator(bool show)
  {
    _selectionIndicator.SetActive(show);
  }

  public void ShowPlayerIndicator(bool show)
  {
    _playerIndicator.SetActive(show);
  }
}
