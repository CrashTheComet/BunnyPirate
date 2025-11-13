using System;
using UnityEngine;

public class PlayerShip : Singleton<PlayerShip>
{
  //static ref
  public static PlayerShip ship => instance;


  float _maxHealth = 100;
  public float Health { get; private set; }
  private float Health01 => Health / _maxHealth;

  Vector3 _targetPosition;
  bool _targetReached;

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

  void Update()
  {
    if (!_targetReached)
    {
      transform.position = Vector3.MoveTowards(transform.position, _targetPosition, Time.deltaTime * 15f);
      if (Vector3.Distance(transform.position, _targetPosition) < 0.05f)
        _targetReached = true;
    }
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

  public static void MoveTo(Vector3 position)
  {
    if (!verify)
      return;
    instance._targetPosition = position;
    instance._targetReached = false;
  }

  public static void MoveToInstantly(Vector3 _position)
  {
    if (!verify)
      return;
    instance._targetPosition = _position;
    instance.transform.position = _position;
    instance._targetReached = true;
  }
}
