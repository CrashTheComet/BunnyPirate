using System;
using UnityEngine;

public class PlayerShip : MonoBehaviour
{
  float _maxHealth = 100;
  public float Health { get; private set; }

  private float Health01 => Health / _maxHealth;

  public event Action OnDeath;

  [SerializeField] NotesTrack notesTrack;

  SpriteRenderer spriteRenderer;

  void Awake()
  {
    spriteRenderer = GetComponent<SpriteRenderer>();

    Health = _maxHealth;

    notesTrack.OnBadInput += () => { Hurt(10f); };
    notesTrack.OnNoteMiss += () => { Hurt(10f); };
  }

  public void Hurt(float amount)
  {
    Health = Mathf.Clamp(Health - amount, 0, _maxHealth);
    UpdateVisualState();
    if (Health == 0f)
      OnDeath?.Invoke();
  }

  public void Heal(float amount)
  {
    Health = Mathf.Clamp(Health + amount, 0, _maxHealth);
    UpdateVisualState();
  }

  public void UpdateVisualState()
  {
    spriteRenderer.color = new Color(
Health01,
Health01,
Health01
    );
  }
}
