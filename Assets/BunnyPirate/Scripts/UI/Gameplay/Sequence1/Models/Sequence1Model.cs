using System;
using UnityEngine;

public class Sequence1Controller : SequenceController
{
    // Needed for Sequence sorting
    [SerializeField] public int Order => 1;

    [SerializeField] Sequence1View _sequence1View;
    [SerializeField] GameMap _gameMap;

    protected void Awake()
    {
        _sequence1View.OnConfirmMapSpace += ConfirmMapSelection;

        CreateMap();

        SequenceAwake();
    }

    public override void SequenceAwake()
    {
        base.SequenceAwake();
        //
    }

    public override void EnterSequence()
    {
        base.EnterSequence();
        _sequence1View.EnterSequence();
    }

    public override void ExitSequence()
    {
        base.ExitSequence();
        _sequence1View.EnterSequence();
    }

    private void CreateMap()
    {

        if (_gameMap == null)
            _gameMap = FindFirstObjectByType<GameMap>();

        // THE CRUTCH IS A CRUTCH !!! - I do not know how to write in English that this code is temporary
        var _playerShip = FindFirstObjectByType<PlayerShip>();
        //

        _gameMap.MovePlayerShipTo(_gameMap.GetInitialSpace(), _playerShip);
    }

    private void ConfirmMapSelection()
    {
        _gameMap.Confirm();
    }
}

// I still don't know why this class is needed

//public class Sequence1Presenter
//{
//  Sequence1Controller _model;
//  ISequence1UI _view;

//  public Sequence1Presenter(Sequence1Controller model, ISequence1UI view)
//  {
//    _model = model;
//    _view = view;

//    _model.OnChange += UpdateView;

//    _view.OnShowMap += ShowMap;
//    _view.OnCancelMap += CancelMap;
//    _view.OnConfirmMapSpace += ConfirmMapSpace;
//  }

//  private void UpdateView()
//  {
//    _view.ShowGameMap(_model._mapOpened);
//  }

//  private void ShowMap()
//  {
//    _model.ShowGameMap(true);
//  }

//  private void CancelMap()
//  {
//    _model.ShowGameMap(false);
//  }

//  private void ConfirmMapSpace()
//  {
//    _model.ConfirmMapSelection();
//  }
//}
