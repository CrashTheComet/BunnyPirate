using UnityEngine;
using UnityEngine.UI;

public class MapSpaceButton : MonoBehaviour
{
  public string spaceName;

  Button button;

  void Awake()
  {
    button = GetComponent<Button>();
  }

  public void SetName(string name)
  {
    ClearEvents();

    spaceName = name;

    if (button == null)
      button = GetComponent<Button>();

    button.onClick.AddListener(() => { GameManager.TrySelectMapSpace(spaceName); });
  }

  private void ClearEvents()
  {
    if (button == null)
      button = GetComponent<Button>();

    button.onClick.RemoveAllListeners();
  }
}
