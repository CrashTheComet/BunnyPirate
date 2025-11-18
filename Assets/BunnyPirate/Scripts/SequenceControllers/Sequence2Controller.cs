using UnityEngine;

public class Sequence2Controller : SequenceController
{
  [SerializeField] NotesTrack _notesTrack;

  [SerializeField] TrackEventGenerator trackEventGeneratorTemp;

  float playedTime = 0;

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

  void Update()
  {

  }

  public override void EnterSequence()
  {
    base.EnterSequence();
    PlayerShip.MoveToInstantly(Vector3.left * 2.5f);
    _notesTrack.gameObject.SetActive(true);
    _notesTrack.LoadEvent(trackEventGeneratorTemp.GetNew());
  }
}