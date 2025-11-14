using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NotesTrack : MonoBehaviour
{
  bool _paused;

  [SerializeField] Transform[] _laneTransforms;
  [SerializeField] Note[] _trackNotes;

  [SerializeField] Transform barTransform;

  [SerializeField] float _trackSpeed = 1;
  float playedTime = -2f;

  TrackEvent loadedEvent;

  List<Note> _scrollingNotes = new List<Note>();

  List<Note> _animatedNotes = new List<Note>();
  NotePool _notePool;

  public event Action OnNoteHit;
  public event Action OnBadInput;
  public event Action OnNoteMiss;

  void Awake()
  {
    //pull templates for instantiation
    GameObject templateObject = GameObject.Find("NoteTemplates");
    Note[] templateNotes = templateObject.GetComponentsInChildren<Note>();
    _trackNotes = new Note[templateNotes.Length];

    //configure and disable template objects
    for (int i = 0; i < templateNotes.Length; i++)
    {
      _trackNotes[i] = templateNotes[i];
      templateNotes[i].NoteTypeIndex = i;
      templateNotes[i].gameObject.SetActive(false);
    }

    InitializeNotePool();

    Inputs.onNoteStrike1 += () => { PassInput(0); };
    Inputs.onNoteStrike2 += () => { PassInput(1); };
    Inputs.onNoteStrike3 += () => { PassInput(2); };
  }

  void Update()
  {
    float deltaTime = Time.deltaTime;
    Animate(deltaTime);
    DisplayNotes(playedTime);
    playedTime += deltaTime;
  }

  private void InitializeNotePool()
  {
    GameObject pool = new GameObject();
    pool.name = "NotePool";
    _notePool = pool.AddComponent<NotePool>();
  }

  private Note GetNewNote(int n)
  {
    if (!Utility.InRange(_trackNotes, n))
    {
      Debug.LogError("Cannot get new note! Index out of range!");
      Debug.LogError($"index n {n} max {_trackNotes.Length - 1}");
      return null;
    }

    if (_notePool == null)
      InitializeNotePool();

    Note note = _notePool.Pull(n);
    if (note == null)
    {
      note = Instantiate(_trackNotes[n].gameObject).GetComponent<Note>();
      note.gameObject.SetActive(true);
    }

    _scrollingNotes.Add(note);

    note.RecycleNextFrame = false;
    return note;
  }

  private void RecycleNote(Note note)
  {
    Debug.Log("recycle");
    if (_notePool == null)
      InitializeNotePool();

    _notePool.Push(note);
  }

  private void RecycleMarkedNotes()
  {
    for (int i = 0; i < _scrollingNotes.Count; i++)
      if (_scrollingNotes[i].RecycleNextFrame)
      {
        RecycleNote(_scrollingNotes[i]);
        _scrollingNotes.Remove(_scrollingNotes[i]);
      }

    for (int i = 0; i < _animatedNotes.Count; i++)
      if (_animatedNotes[i].RecycleNextFrame)
      {
        RecycleNote(_animatedNotes[i]);
        _animatedNotes.Remove(_animatedNotes[i]);
      }
  }

  public void LoadEvent(TrackEvent trackEvent)
  {
    loadedEvent = trackEvent;
    DisplayNotes(0);
  }

  private void DisplayNotes(float time)
  {
    if (loadedEvent == null)
      return;

    RecycleMarkedNotes();

    List<EventNote> notes = loadedEvent.eventNotes;
    for (int i = 0; i < notes.Count; i++)
    {
      if (i < _scrollingNotes.Count)
      {
        _scrollingNotes[i].transform.position = new Vector3(
        _laneTransforms[notes[i].lane].transform.position.x,
        barTransform.position.y + notes[i].timeStamp + time * _trackSpeed * -1,
        0
              );
      }
      else
      {
        Note newNote = GetNewNote(notes[i].lane);
        newNote.eventNote = notes[i];
        newNote.transform.position = new Vector3(
                _laneTransforms[notes[i].lane].transform.position.x,
                barTransform.position.y + notes[i].timeStamp + time * _trackSpeed * -1,
                0
                      );
      }
    }

    Debug.Log(_scrollingNotes.Count);
  }

  private void PassInput(int lane)
  {
    for (int i = 0; i < _scrollingNotes.Count; i++)
    {
      if (Mathf.Abs(_scrollingNotes[i].transform.position.y - barTransform.position.y) < 0.125f)
      {
        loadedEvent.eventNotes.Remove(_scrollingNotes[i].eventNote);
        Spray(_scrollingNotes[i]);
      }
    }
  }

  public void Spray(Note note)
  {
    if (_scrollingNotes.Contains(note))
      _scrollingNotes.Remove(note);

    _animatedNotes.Add(note);

    note.transform.SetParent(null);
    note.AnimatedTime = 0;
    note.angularVelocity = Random.Range(-35f, 35f);
    note.SprayVelocity = new Vector2(
Random.Range(-3f, 3f),
Random.Range(3f, 6f)
    );
  }

  public void Animate(float deltaTime)
  {

    for (int i = 0; i < _animatedNotes.Count; i++)
    {
      Note note = _animatedNotes[i];
      note.transform.position += (Vector3)note.SprayVelocity * deltaTime;
      note.SprayVelocity.x = Mathf.MoveTowards(note.SprayVelocity.x, 0, deltaTime);
      note.SprayVelocity.y -= 9.81f * deltaTime;

      note.transform.Rotate(new Vector3(0, 0, note.angularVelocity * deltaTime));
      note.angularVelocity = Mathf.MoveTowards(note.angularVelocity, 0, deltaTime);


      if (note.AnimatedTime >= 1f)
      {
        note.RecycleNextFrame = true;
      }

      note.AnimatedTime += deltaTime;
    }
  }
}
