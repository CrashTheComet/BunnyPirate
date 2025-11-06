using System.Collections.Generic;
using UnityEngine;

public class NotesTrack : MonoBehaviour
{
  bool _paused;

  [SerializeField] Transform[] _laneTransforms;
  [SerializeField] Note[] _trackNotes;

  [SerializeField] float _trackSpeed = 1;

  List<Note>[] _activeNotes;

  List<Note> _animatedNotes = new List<Note>();

  Vector3 trackHalfScale => transform.lossyScale / 2;

  NotePool _notePool;

  float noteTime = 0;

  void Awake()
  {
    GameObject templateObject = GameObject.Find("NoteTemplates");
    Note[] templateNotes = templateObject.GetComponentsInChildren<Note>();
    _trackNotes = new Note[templateNotes.Length];

    for (int i = 0; i < templateNotes.Length; i++)
    {
      _trackNotes[i] = templateNotes[i];
      templateNotes[i].NoteTypeIndex = i;
      templateNotes[i].gameObject.SetActive(false);
    }

    InitializeNotePool();

    _activeNotes = new List<Note>[_laneTransforms.Length];
    for (int i = 0; i < _activeNotes.Length; i++)
      _activeNotes[i] = new List<Note>();

    Inputs.onNoteStrike1 += () => { PassInput(0); };
    Inputs.onNoteStrike2 += () => { PassInput(1); };
    Inputs.onNoteStrike3 += () => { PassInput(2); };
  }

  void Update()
  {
    float deltaTime = Time.deltaTime;

    MoveNotesDown(deltaTime);
    Animate(deltaTime);
    noteTime += deltaTime;

    if (noteTime > 0.2f)
    {
      noteTime = 0;
      DropNote(Random.Range(0, 3));
    }
  }

  private void DropNote(int i)
  {
    if (!Utility.InRange(_laneTransforms, i))
    {
      Debug.LogError($"Cannot drop note! Index out of range!");
      Debug.LogError($"index i {i} max {_trackNotes.Length - 1} or {_laneTransforms.Length - 1}");
      return;
    }

    if (_notePool == null)
      InitializeNotePool();

    Note newNote = GetNewNote(i);
    newNote.transform.position = PositionOnTrack(i, 0);
    _activeNotes[i].Add(newNote);
  }

  private void DropNote(int i, int n)
  {
    if (!Utility.InRange(_laneTransforms, n) || !Utility.InRange(_trackNotes, i))
    {
      Debug.LogError("Cannot drop note! Index out of range!");
      Debug.LogError($"index i {i} max {_trackNotes.Length - 1}, index n {n} max {_laneTransforms.Length - 1}");
      return;
    }
    if (_notePool == null)
      InitializeNotePool();

    Note newNote = GetNewNote(n);
    newNote.transform.position = PositionOnTrack(i, 0);
    _activeNotes[n].Add(newNote);
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

    return note;
  }

  private void Recycle(Note note)
  {
    if (_notePool == null)
      InitializeNotePool();

    _notePool.Push(note);
  }

  private void MoveNotesDown(float deltaTime)
  {
    for (int i = 0; i < _activeNotes.Length; i++)
      for (int n = 0; n < _activeNotes[i].Count; n++)
      {
        Note currentNote = _activeNotes[i][n];
        currentNote.TrackPosition += deltaTime * _trackSpeed;
        currentNote.transform.position = PositionOnTrack(i, currentNote.TrackPosition);
        if (currentNote.TrackPosition > 1)
        {
          _activeNotes[i].Remove(currentNote);
          Recycle(currentNote);
        }
      }
  }

  private void PassInput(int i)
  {
    if (!Utility.InRange(_laneTransforms, i))
    {
      Debug.LogError("Cannot pass input! Index out of range!");
      Debug.LogError($"index i {i} max {_activeNotes.Length - 1}");
      return;
    }

    for (int n = 0; n < _activeNotes[i].Count; n++)
    {
      Note note = _activeNotes[i][n];
      if (note.TrackPosition > 0.75f && note.TrackPosition <= 0.9f)
      {
        _activeNotes[i].Remove(note);
        Spray(note);
      }
    }
  }

  private Vector3 PositionOnTrack(int i, float t)
  {
    if (!Utility.InRange(_laneTransforms, i))
    {
      Debug.LogError("Cannot find position! Index out of range!");
      Debug.LogError($"index i {i} max {_laneTransforms.Length - 1}");
      return Vector3.zero;
    }
    Vector3 topPosition = _laneTransforms[i].position + Vector3.up * trackHalfScale.y;
    return Vector3.Lerp(topPosition, _laneTransforms[i].position + Vector3.down * trackHalfScale.y, t);
  }

  public void Spray(Note note)
  {
    note.transform.SetParent(null);
    note.TrackPosition = 0;
    note.angularVelocity = Random.Range(-35f, 35f);
    note.SprayVelocity = new Vector2(
Random.Range(-3f, 3f),
Random.Range(3f, 6f)
    );
    _animatedNotes.Add(note);
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


      if (note.TrackPosition >= 1f)
      {
        _animatedNotes.Remove(note);
        Recycle(note);
      }

      note.TrackPosition += deltaTime;
    }
  }
}
