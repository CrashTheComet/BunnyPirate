using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TrackEvent
{
  public float Bpm { get; private set; }
  TimeSignature _timeSignature = TimeSignature.FourFour;
  float _signatureBeatCount =>
  _timeSignature == TimeSignature.FourFour ? 4 :
  _timeSignature == TimeSignature.ThreeFour ? 3 :
   6;

  List<EventNote> initialNotes;

  List<EventNote> _eventNotes;
  public List<EventNote> eventNotes => _eventNotes;

  private float _beatTime => 60 / Bpm;

  public TrackEvent(float bpm, TimeSignature timeSignature, EventNote[] notes)
  {
    Bpm = bpm;
    _timeSignature = timeSignature;

    SetTimeStamps(notes);

    initialNotes = notes.ToList();
    _eventNotes = new(initialNotes);
  }

  private void SetTimeStamps(EventNote[] notes)
  {
    foreach (EventNote note in notes)
    {
      note.timeStamp =
      _beatTime * _signatureBeatCount * note.measure +
      _beatTime * note.positionInMeasure * note.noteLengthMultiplier;
    }
  }
}

public class EventNote
{
  public int lane;
  public float timeStamp;
  public int positionInMeasure;
  public int measure;
  public NoteLength noteLength;

  public float noteLengthMultiplier =>
  noteLength == NoteLength.Full ? 4 :
  noteLength == NoteLength.Half ? 2 :
  noteLength == NoteLength.Fourth ? 1 :
  noteLength == NoteLength.Eigth ? 0.5f :
  noteLength == NoteLength.Sixteenth ? 0.25f :
  noteLength == NoteLength.ThirtySecond ? 0.125f : 0.0625f;


  public EventNote(int lane, NoteLength length, int measure, int positionInMeasure)
  {
    this.lane = lane - 1;
    noteLength = length;
    this.measure = measure - 1;
    this.positionInMeasure = positionInMeasure - 1;
  }

  public EventNote(NoteDefinition definition)
  {
    lane = definition.lane - 1;
    noteLength = definition.noteLength;
    measure = definition.measure - 1;
    positionInMeasure = definition.positionInMeasure - 1;
  }
}

public enum TimeSignature
{
  FourFour,
  ThreeFour,
  SixEight
}

public enum NoteLength
{
  Full,
  Half,
  Fourth,
  Eigth,
  Sixteenth,
  ThirtySecond,
  SixtyFourth
}
