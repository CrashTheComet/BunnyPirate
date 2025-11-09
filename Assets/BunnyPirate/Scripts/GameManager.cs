using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

  public static void LoadScene(string scene)
  {
    SceneManager.LoadScene(scene);
  }

  //main menu scene

  //gameplay scene
  static PlayerShip _playerShip;
  static EnvironmentManager _environmentManager;
  static NotesTrack _notesTrack;
  static GameMap _gameMap;

  public static void Register(PlayerShip ship)
  {
    _playerShip = ship;
    if (instance != null)
      instance.TryInitialize();
  }
  public static void Register(EnvironmentManager environmentManager)
  {
    _environmentManager = environmentManager;
    if (instance != null)
      instance.TryInitialize();
  }
  public static void Register(NotesTrack notesTrack)
  {
    _notesTrack = notesTrack;
    if (instance != null)
      instance.TryInitialize();
  }
  public static void Register(GameMap gameMap)
  {
    _gameMap = gameMap;
    if (instance != null)
      instance.TryInitialize();
  }

  bool allSystemsReady =>
  _playerShip != null &&
  _environmentManager != null &&
  _notesTrack != null &&
  _gameMap != null;

  private void TryInitialize()
  {
    if (allSystemsReady)
      EnterSequence1();
  }

  //sequence 1:
  //  Use items, buy items, look at things, etc. ->
  //  click set sail button ->
  //  Go back or pick an area ->
  //  Confirm selection ->
  //  begin sequence 2.

  public void EnterSequence1()
  {
    _gameMap.gameObject.SetActive(false);
    _notesTrack.gameObject.SetActive(false);
    _gameMap.MovePlayerShipTo(_gameMap.GetInitialSpace(), _playerShip);
    _environmentManager.DisplaySpace(_gameMap.GetCurrentSpace());
  }
}
