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

  [SerializeField] Button _mapButtonTemplate;
  List<Button> _mapButtons = new();

  void Awake()
  {
    _showMapButton.onClick.AddListener(() => { ShowGameMap(true); });
    _cancelMapButton.onClick.AddListener(() => { ShowGameMap(false); });

    _cancelMapButton.gameObject.SetActive(false);
    _showMapButton.gameObject.SetActive(true);
  }

  public void ShowGameMap(bool show)
  {
    _gameMapObject.SetActive(show);

    _cancelMapButton.gameObject.SetActive(show);
    _showMapButton.gameObject.SetActive(!show);
  }

  public void UpdateMapButtons()
  {
    MapSpace[] mapSpaces = GameManager.GetMapSpaces();
    for (int i = 0; i < mapSpaces.Length; i++)
    {
      if (i >= _mapButtons.Count)
      {
        Button b = Instantiate(_mapButtonTemplate.gameObject).GetComponent<Button>();
        b.gameObject.SetActive(true);
        b.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(
          mapSpaces[i].transform.position
        );
        _mapButtons.Add(b);
      }
    }
  }
}
