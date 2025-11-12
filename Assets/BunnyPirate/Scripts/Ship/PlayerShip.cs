using System;
using UnityEngine;

public class PlayerShip : Singleton<PlayerShip>
{
  //static ref
  public static PlayerShip ship => instance;

  //health
  float _maxHealth = 100;
  public float Health { get; private set; }
  private float Health01 => Health / _maxHealth;

  //events
  public event Action OnDeath;

  //component references
  SpriteRenderer spriteRenderer;

  protected override void Awake()
  {
    base.Awake();

    spriteRenderer = GetComponent<SpriteRenderer>();

    Health = _maxHealth;
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
