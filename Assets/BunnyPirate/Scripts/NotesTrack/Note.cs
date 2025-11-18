using UnityEngine;

public class Note : MonoBehaviour
{
  [HideInInspector] public float AnimatedTime = 0;
  [HideInInspector] public int NoteTypeIndex = 0;
  [HideInInspector] public Vector2 SprayVelocity;
  [HideInInspector] public float angularVelocity;
  [HideInInspector] public bool RecycleNextFrame;
  [HideInInspector] public EventNote eventNote;
}
