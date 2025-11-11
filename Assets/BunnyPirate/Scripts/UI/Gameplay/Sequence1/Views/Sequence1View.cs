using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct gameUI
{
    [SerializeField] private string name;
    [SerializeField] private List<GameObject> GameObjects;

    public string Name { get => name; }
    public void SetActive(bool isActive)
    {
        foreach (var item in GameObjects)
        {
            item.SetActive(isActive);
        }
    }
}

/// <summary>
/// There are purely interfaces inside Sequence, 
/// but you can try to cram objects according to the type of card, 
/// although it is doubtful
/// </summary>
[Serializable]
public class gameUIList
{
    [SerializeField] private List<gameUI> _gameUI;

    public int FindGameUIByName(string name)
    {
        for (int i = 0; i < _gameUI.Count; i++)
            if (_gameUI[i].Name == name)
                return i;
        return -1;
    }

    public void ActiveByNum(int num)
    {
        for (int i = 0; i < _gameUI.Count; i++)
            _gameUI[i].SetActive(i == num);
    }
}

[Serializable]
public class Sequence1View : MonoBehaviour
{
    [SerializeField] GameMap _gameMap;
    [SerializeField] private gameUIList _gameUIList;

    [SerializeField] MapSpaceButton _mapButtonTemplate;
    List<MapSpaceButton> _mapButtons = new();

    public event Action OnConfirmMapSpace;
    [SerializeField] Transform _buttonParentTransform;

    // The local version of Awake
    public void EnterSequence()
    {
        ShowGameLobby();
    }

    public void ShowGameLobby()
    {
        _gameMap.gameObject.SetActive(false);

        int n = _gameUIList.FindGameUIByName("GameLobby");

        if (n == -1)
            Debug.LogError("Wrong Name Of UI");
        else
            _gameUIList.ActiveByNum(n);
    }

    public void ShowGameMap()
    {
        _gameMap.gameObject.SetActive(true);

        int n = _gameUIList.FindGameUIByName("GameMap");

        if (n == -1)
            Debug.LogError("Wrong Name Of UI");
        else
            _gameUIList.ActiveByNum(n);

        UpdateMapSpaces();
    }

    public void OnConfirm()
    {
        OnConfirmMapSpace?.Invoke();
    }

    // I didn't understand this code very well, but I like it conceptually
    // I think I broke the MapSpace operation, but I'm fixing it.
    public void UpdateMapSpaces()
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
                _mapButtons[i].SetName(mapSpaces[i].spaceName);
            }
            else
            {
                _mapButtons[i].gameObject.SetActive(false);
            }
        }
    }
}
