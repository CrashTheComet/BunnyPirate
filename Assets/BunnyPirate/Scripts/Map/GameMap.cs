using System.Collections.Generic;
using UnityEngine;

public class GameMap : MonoBehaviour
{
  List<MapArea> mapAreas = new List<MapArea>();

  MapArea loadedArea;

}

public class MapArea
{
  public string areaName { get; private set; }
  public Color backgroundColor { get; private set; }
  public void Initialize(AreaData data)
  {
    areaName = data.AreaName;
    backgroundColor = data.BackgroundColor;
  }
}