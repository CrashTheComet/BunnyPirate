using UnityEngine;

[CreateAssetMenu(fileName = "new AreaData", menuName = "BunnyPirate/AreaData")]
public class AreaData : ScriptableObject
{
  public string AreaName { get; private set; }
  public Color BackgroundColor { get; private set; }
}
