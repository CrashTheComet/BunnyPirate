using UnityEngine;

public class Sequence2Controller : SequenceController
{
  [SerializeField] NotesTrack _notesTrack;

  void Awake()
  {
    Order = 1;
    GameManager.Register(this);
  }

  void Start()
  {
    //Only disable objects in start() to make sure all objects have a chance to run Awake()
    _notesTrack.gameObject.SetActive(false);
  }
}
