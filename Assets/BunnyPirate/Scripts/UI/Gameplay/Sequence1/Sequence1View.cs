using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Sequence1View : MonoBehaviour
{
  public GameMap _gameMap;

  [SerializeField] Button _showMapButton;
  [SerializeField] Button _cancelMapButton;
  [SerializeField] Button _confirmMapSpaceButton;

  [SerializeField] MapSpaceButton _mapButtonTemplate;
  List<MapSpaceButton> _mapButtons = new();

  [SerializeField] Transform _buttonParentTransform;

  void Awake()
  {
    _showMapButton.onClick.AddListener(() => { ShowGameMap(true); });
    _cancelMapButton.onClick.AddListener(() => { ShowGameMap(false); });
    _confirmMapSpaceButton.onClick.AddListener(() => { _gameMap.ConfirmMapSpace(); });

    _cancelMapButton.gameObject.SetActive(false);
    _showMapButton.gameObject.SetActive(true);
    _confirmMapSpaceButton.gameObject.SetActive(false);
  }

  public void ShowGameMap(bool show)
  {
    _gameMap.gameObject.SetActive(show);

    _cancelMapButton.gameObject.SetActive(show);
    _showMapButton.gameObject.SetActive(!show);
    _confirmMapSpaceButton.gameObject.SetActive(show);

    if (show)
      UpdateMapButtons();
  }

  public void UpdateMapButtons()
  {
    MapSpace[] mapSpaces = _gameMap.Spaces;
    for (int i = 0; i < mapSpaces.Length; i++)
    {
      if (i >= _mapButtons.Count)
      {
        MapSpaceButton b = Instantiate(_mapButtonTemplate.gameObject, _buttonParentTransform).GetComponent<MapSpaceButton>();
        b.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(
          mapSpaces[i].transform.position
        );
        _mapButtons.Add(b);
      }
    }

    for (int i = 0; i < _mapButtons.Count; i++)
    {
      if (i < mapSpaces.Length)
      {
        _mapButtons[i].gameObject.SetActive(true);
        _mapButtons[i].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(
          mapSpaces[i].transform.position
        );
        _mapButtons[i].Set(mapSpaces[i].spaceName, _gameMap);
      }
      else
      {
        _mapButtons[i].gameObject.SetActive(false);
      }
    }
  }
}
