using UnityEngine;
using UnityEngine.UI;

public class MapSpaceButton : MonoBehaviour
{
  [HideInInspector] public string _spaceName;

  Button _button;
  GameMap _map;

  void Awake()
  {
    _button = GetComponent<Button>();
  }

  public void Set(string name, GameMap map)
  {
    ClearEvents();

    _spaceName = name;
    _map = map;

    if (_button == null)
      _button = GetComponent<Button>();

    _button.onClick.AddListener(() => { _map.SelectMapSpace(_spaceName); });
  }

  private void ClearEvents()
  {
    if (_button == null)
      _button = GetComponent<Button>();

    _button.onClick.RemoveAllListeners();
  }
}
