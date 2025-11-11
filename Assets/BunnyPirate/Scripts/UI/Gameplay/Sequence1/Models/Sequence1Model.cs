using System;
using UnityEngine;

public interface ISequence1UI
{
  void ShowGameMap(bool show);

  event Action OnShowMap;
  event Action OnCancelMap;
  event Action OnConfirmMapSpace;
}

public class SequenceController
{
  public bool _mapOpened = false;

  public event Action OnChange;

  public void ShowGameMap(bool show)
  {
    _mapOpened = show;
    OnChange?.Invoke();
  }

  public void ConfirmMapSelection()
  {
    GameManager.ConfirmMapSelection();
    OnChange?.Invoke();
  }
}

public class Sequence1Presenter
{
  SequenceController _model;
  ISequence1UI _view;

  public Sequence1Presenter(SequenceController model, ISequence1UI view)
  {
    _model = model;
    _view = view;

    _model.OnChange += UpdateView;

    _view.OnShowMap += ShowMap;
    _view.OnCancelMap += CancelMap;
    _view.OnConfirmMapSpace += ConfirmMapSpace;
  }

  private void UpdateView()
  {
    _view.ShowGameMap(_model._mapOpened);
  }

  private void ShowMap()
  {
    _model.ShowGameMap(true);
  }

  private void CancelMap()
  {
    _model.ShowGameMap(false);
  }

  private void ConfirmMapSpace()
  {
    _model.ConfirmMapSelection();
  }
}
