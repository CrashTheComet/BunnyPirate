using UnityEngine;

public class GameplayUIView : MonoBehaviour
{
  [SerializeField] GameObject sequence1Panel;
  public void ShowSequence(int sequence)
  {
    CloseAllSequences();
    if (sequence == 1)
    {
      sequence1Panel.gameObject.SetActive(true);
    }
  }

  private void CloseAllSequences()
  {
    sequence1Panel.SetActive(false);
  }
}
