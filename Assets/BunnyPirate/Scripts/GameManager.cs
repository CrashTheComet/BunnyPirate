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
  static GameplayUIController _gameplayUI;

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
  public static void Register(GameplayUIController gameplayUI)
  {
    _gameplayUI = gameplayUI;
    if (instance != null)
      instance.TryInitialize();
  }

  bool allSystemsReady =>
  _playerShip != null &&
  _environmentManager != null &&
  _notesTrack != null &&
  _gameMap != null &&
  _gameplayUI != null;

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

  public static void EnterSequence1()
  {
    _gameMap.gameObject.SetActive(false);
    _notesTrack.gameObject.SetActive(false);
    _gameMap.MovePlayerShipTo(_gameMap.GetInitialSpace(), _playerShip);
    _environmentManager.DisplaySpace(_gameMap.GetCurrentSpace());
  }

  public static MapSpace[] GetMapSpaces() => _gameMap.Spaces;

  public static void TrySelectMapSpace(string name)
  {
    _gameMap.SelectMapSpace(name);
  }

  public static void ConfirmMapSelection()
  {
    _gameMap.Confirm();
  }

  //sequence 2:
  //disable unneeded objects from sequence 1
  //move the player ship (just a white square for now) to the side and start up the notes track, which plays a series of rhythym events.
  //I'll comment the best I can on how the notes track works, but the main thing will be the DropNote function.
  //modify EnterSequence2 as needed.
  //I'll work on making the map selection system work more intentionally later. For now you just 
  //pick one of the blue circles and click confirm, which will fire off the EnterSequence2 function.

  public static void EnterSequence2(string destinationName)
  {
    Debug.Log($"EnterSequence2(distinationName: {destinationName})");
  }
}
