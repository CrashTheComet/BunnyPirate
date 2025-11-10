using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
  [SerializeField] SpriteRenderer backgroundRenderer;

  void Awake()
  {
    GameManager.Register(this);
  }


  public void DisplaySpace(MapSpace space)
  {
    backgroundRenderer.color = space.backgroundColor;
  }

  public void StartTransition()
  {
    //start transition from player's current area to another.
  }

  //environment manager handles the game elements and ui when loading into a new area or transition.
  //
}
