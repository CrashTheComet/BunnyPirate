using UnityEngine;

public class Sequence1Controller : SequenceController
{
  [SerializeField] Sequence1View _sequenceView;
  [SerializeField] GameMap _gameMap;

  void Awake()
  {
    Order = 0;
    GameManager.Register(this);

    _sequenceView._gameMap = _gameMap;
  }

  void Start()
  {
    //Only disable objects in start() to make sure all objects have a chance to run Awake()
    _gameMap.MovePlayerShipTo(_gameMap.GetInitialSpace());
    _sequenceView.ShowGameMap(false);
  }
}
