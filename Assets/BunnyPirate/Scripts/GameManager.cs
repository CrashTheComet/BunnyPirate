using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

  List<SequenceController> _sequenceControllers = new();
  SequenceController _currentSequence;

  public static void LoadScene(string scene)
  {
    SceneManager.LoadScene(scene);
  }

  //sequence controllers handle their own initialization
  public static void Register(SequenceController controller)
  {
    if (!verify)
      return;

    instance._sequenceControllers.Add(controller);
    instance._sequenceControllers.Sort((a, b) => a.Order.CompareTo(b.Order));

    if (controller.Order == 0)
      SwitchToSequence(0);
  }

  public static void SwitchToSequence(int i)
  {
    if (!Utility.InRange(instance._sequenceControllers.Count, i))
      return;
    SequenceController sequence = instance._sequenceControllers[i];
    if (sequence == null)
      return;

    if (instance._currentSequence != null)
      instance._currentSequence.ExitSequence();

    instance._currentSequence = sequence;

    instance._currentSequence.EnterSequence();
  }
}
