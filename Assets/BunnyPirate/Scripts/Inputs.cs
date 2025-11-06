using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inputs : Singleton<Inputs>
{
  public static event Action onNoteStrike1;
  private void OnNoteStrike1(InputValue val)
  {
    onNoteStrike1?.Invoke();
  }

  public static event Action onNoteStrike2;
  private void OnNoteStrike2(InputValue val)
  {
    onNoteStrike2?.Invoke();
  }

  public static event Action onNoteStrike3;
  private void OnNoteStrike3(InputValue val)
  {
    onNoteStrike3?.Invoke();
  }

  void OnDestroy()
  {
    onNoteStrike1 = null;
    onNoteStrike2 = null;
    onNoteStrike3 = null;
  }
}
