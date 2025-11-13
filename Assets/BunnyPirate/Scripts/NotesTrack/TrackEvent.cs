using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackEvent
{
  float _duration;
  float _bpm;

  List<EventNote> _eventNotes;
  public EventNote[] AllNotes => _eventNotes.ToArray();

  public TrackEvent(float duration, float bpm, EventNote[] notes)
  {
    _duration = duration;
    _bpm = bpm;

    _eventNotes = notes.ToList();
  }

  public EventNote[] NotesInRange(float start, float end)
  {
    List<EventNote> rangeNotes = new List<EventNote>();
    for (int i = 0; i < _eventNotes.Count; i++)
    {
      float ts = _eventNotes[i].timeStamp;
      if (ts > start && ts < end)
        rangeNotes.Add(_eventNotes[i]);
    }

    return rangeNotes.ToArray();
  }
}

public struct EventNote
{
  public int lane;
  public float timeStamp;

  public EventNote(int lane, float timeStamp)
  {
    this.lane = lane;
    this.timeStamp = timeStamp;
  }
}
