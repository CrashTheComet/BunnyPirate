using System;
using Unity.VisualScripting;
using UnityEngine;

public interface IGameplayUI
{
  void ShowSequence(int sequence);
}

public class GameplayUIModel
{
  public int sequence = 0;

  public event Action OnChanged;

  public void ShowSequence(int sequence)
  {
    this.sequence = sequence;
    OnChanged?.Invoke();
  }
}

public class GameplayUIPresenter
{
  GameplayUIModel _model;
  IGameplayUI _view;

  public GameplayUIPresenter(GameplayUIModel model, IGameplayUI view)
  {
    _model = model;
    _view = view;

    _model.OnChanged += UpdateView;
  }

  private void UpdateView()
  {
    _view.ShowSequence(_model.sequence);
  }
}
