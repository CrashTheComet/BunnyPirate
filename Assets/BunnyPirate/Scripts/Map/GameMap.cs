using System.Collections.Generic;
using UnityEngine;

public class GameMap : MonoBehaviour
{
  //areas and transitions contian and pass along references to the player's ship and other player data.
  //areas also contain references to shops, npcs, cutscenes, etc.
  //transitions are what contain references to events, tracks, more cutscenes, etc.
  //
  //initial area has a single transition to middle areas,
  //  middle areas have 2 or more transitions to other middle areas or an end area.
  //  end areas have no transitions.

  //Areas are handled statically, while the representations of those areas are handled non-statically.

  //areas and transitions are both examples of spaces? so MapArea : MapSpace and MapTransition : MapSpace
  [SerializeField] MapSpace[] _spaces;
  public MapSpace[] Spaces => _spaces;
  MapSpace _selectedMapSpace;
  [SerializeField] int initialSpace;

    void Awake()
    {
        foreach (MapSpace space in Spaces)
            space.Initialize(this);
        GameManager.Register(this);
    }

  public MapSpace GetInitialSpace()
  {
    return _spaces[initialSpace];
  }

  public MapSpace GetCurrentSpace()
  {
    for (int i = 0; i < _spaces.Length; i++)
    {
      if (_spaces[i].ship != null)
        return _spaces[i];
    }
    return null;
  }

    public bool CheckConnection(MapSpace newSpace)
    {
        MapSpace start = GetCurrentSpace();

        for (int i = 0; i < start.ConnectedSpaces.Length; i++)
        {
            if (start.ConnectedSpaces[i] == newSpace)
            {
                return true;
            }
        }

        return false;
    }

  public void MovePlayerShipTo(MapSpace space, PlayerShip ship)
  {
    MapSpace current = GetCurrentSpace();
    if (current != null)
    {
      current.ShowPlayerIndicator(false);
      current.ExitPlayer();
    }


    for (int i = 0; i < _spaces.Length; i++)
    {
      if (_spaces[i] == space)
      {
        _spaces[i].EnterPlayer(ship);
        _spaces[i].ShowPlayerIndicator(true);
        return;
      }
    }
  }

  public void SelectMapSpace(string name)
  {
    for (int i = 0; i < _spaces.Length; i++)
    {
      if (_spaces[i].spaceName == name)
      {
        if (_selectedMapSpace != null)
        {
          _selectedMapSpace.ShowSelectionIndicator(false);
        }
        _selectedMapSpace = _spaces[i];
        _selectedMapSpace.ShowSelectionIndicator(true);
      }
    }
  }

  public void Confirm()
  {
    if (_selectedMapSpace != null)
      GameManager.EnterSequence(1, _selectedMapSpace);
  }
}