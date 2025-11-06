using System.Collections.Generic;
using UnityEngine;

public class NotePool : MonoBehaviour
{
  List<Note> _notes = new();

  public Note Pull(int n)
  {
    for (int i = 0; i < _notes.Count; i++)
      if (_notes[i].NoteTypeIndex == n)
      {
        Note note = _notes[i];
        note.gameObject.SetActive(true);
        note.transform.SetParent(null);
        note.TrackPosition = 0;
        note.transform.rotation = Quaternion.Euler(Vector3.zero);
        _notes.Remove(_notes[i]);
        return note;
      }
    return null;
  }

  public void Push(Note note)
  {
    note.transform.SetParent(transform);
    note.gameObject.SetActive(false);
    _notes.Add(note);
  }
}
