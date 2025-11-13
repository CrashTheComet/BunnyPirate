using UnityEngine;

public class Sequence3Controller : SequenceController
{
  void Awake()
  {
    Order = 2;
    GameManager.Register(this);
  }
}
