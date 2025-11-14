using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TrackEvent
{
  float _duration;
  float _bpm;

  List<EventNote> initialNotes;

  List<EventNote> _eventNotes;
  public List<EventNote> eventNotes => _eventNotes;

  public TrackEvent(float duration, float bpm, EventNote[] notes)
  {
    _duration = duration;
    _bpm = bpm;

    initialNotes = notes.ToList();
    _eventNotes = new(initialNotes);
  }
}

public class EventNote
{
  public int lane;
  public float timeStamp;
  public bool displayNote = true;

  public EventNote(int lane, float timeStamp)
  {
    this.lane = lane;
    this.timeStamp = timeStamp;
  }
}
