using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Sequence1View : MonoBehaviour, ISequence1UI
{
  [SerializeField] GameObject _gameMapObject;

  [SerializeField] Button _showMapButton;
  public event Action OnShowMap;
  [SerializeField] Button _cancelMapButton;
  public event Action OnCancelMap;
  [SerializeField] Button _confirmMapSpaceButton;
  public event Action OnConfirmMapSpace;

  [SerializeField] MapSpaceButton _mapButtonTemplate;
  List<MapSpaceButton> _mapButtons = new();

  [SerializeField] Transform _buttonParentTransform;

  void Awake()
  {
    _showMapButton.onClick.AddListener(() => { OnShowMap?.Invoke(); });
    _cancelMapButton.onClick.AddListener(() => { OnCancelMap?.Invoke(); });
    _confirmMapSpaceButton.onClick.AddListener(() => { OnConfirmMapSpace?.Invoke(); });

    _cancelMapButton.gameObject.SetActive(false);
    _showMapButton.gameObject.SetActive(true);
    _confirmMapSpaceButton.gameObject.SetActive(false);
  }

  public void ShowGameMap(bool show)
  {
    _gameMapObject.SetActive(show);

    _cancelMapButton.gameObject.SetActive(show);
    _showMapButton.gameObject.SetActive(!show);
    _confirmMapSpaceButton.gameObject.SetActive(show);

    if (show)
      UpdateMapButtons();
  }

  public void UpdateMapButtons()
  {
    MapSpace[] mapSpaces = GameManager.GetMapSpaces();
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
        _mapButtons[i].SetName(mapSpaces[i].spaceName);
      }
      else
      {
        _mapButtons[i].gameObject.SetActive(false);
      }
    }
  }
}
