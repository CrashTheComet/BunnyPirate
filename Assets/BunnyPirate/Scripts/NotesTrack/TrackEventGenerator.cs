using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrackEventGenerator", menuName = "BunnyPirate/Track Event Generator")]
public class TrackEventGenerator : ScriptableObject
{
  public int bpm;
  public TimeSignature timeSignature;
  public List<NoteLayer> noteLayers;

  public TrackEvent GetNew()
  {
    List<EventNote> notes = new();
    for (int i = 0; i < noteLayers.Count; i++)
      for (int n = 0; n < noteLayers[i].notes.Count; n++)
        notes.Add(new EventNote(noteLayers[i].notes[n]));
    return new TrackEvent(bpm, timeSignature, notes.ToArray());
  }
}

[Serializable]
public class NoteLayer
{
  public List<NoteDefinition> notes;
}

[Serializable]
public struct NoteDefinition
{
  public int lane;
  public NoteLength noteLength;
  public int measure;
  public int positionInMeasure;
}
