using UnityEngine;

public class EnvironmentManager : Singleton<EnvironmentManager>
{
  [SerializeField] SpriteRenderer backgroundRenderer;

  protected override void Awake()
  {
    base.Awake();
  }

  public static void DisplaySpace(MapSpace space)
  {
    instance.backgroundRenderer.color = space.backgroundColor;
  }
}
