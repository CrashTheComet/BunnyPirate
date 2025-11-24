using UnityEngine;
using UnityEngine.U2D.Animation;

public class ShipAnimator : MonoBehaviour
{
  SpriteResolver spriteResolver;
  ShipAnimation _shipAnimation;

  private float _animTime = 0;
  private float _fps = 8;

  int _currentFrame = 0;
  int _maxFrame;
  string _currentAnimation = "Idle";



  void Awake()
  {
    spriteResolver = GetComponent<SpriteResolver>();
    _maxFrame = 5;
  }

  void Update()
  {
    Animate();
  }

  private void Animate()
  {
    _animTime += Time.deltaTime;
    if (_animTime > _fps / 60)
    {
      _animTime = 0;
      NextFrame();
    }
  }

  private void NextFrame()
  {
    _currentFrame++;
    if (_currentFrame > _maxFrame)
    {
      _currentFrame = 0;
      if (_shipAnimation != ShipAnimation.Idle)
      {
        _shipAnimation = ShipAnimation.Idle;
        _maxFrame = 5;
      }
    }

    spriteResolver.SetCategoryAndLabel(_currentAnimation, _currentAnimation + $"{_currentFrame + 1}");
  }



  private enum ShipAnimation
  {
    Idle,
    Attack1,
    Attack2
  }
}
