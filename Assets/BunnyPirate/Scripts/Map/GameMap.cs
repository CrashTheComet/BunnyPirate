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

  public void MovePlayerShipTo(MapSpace space)
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
        _spaces[i].EnterPlayer(PlayerShip.ship);
        _spaces[i].ShowPlayerIndicator(true);
        EnvironmentManager.DisplaySpace(_spaces[i]);
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

  public void ConfirmMapSpace()
  {
    if (_selectedMapSpace != null && _selectedMapSpace != GetCurrentSpace())
    GameManager.SwitchToSequence(1);
    MovePlayerShipTo(_selectedMapSpace);
  }
}