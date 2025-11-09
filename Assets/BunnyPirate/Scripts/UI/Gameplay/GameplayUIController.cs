using UnityEngine;

public class GameplayUIController : MonoBehaviour
{
  void Awake()
  {
    InitializeGameplayUI();
    InitializeS1();

    GameManager.Register(this);
  }

  [SerializeField] GameplayUIView _gameplayUIView;

  GameplayUIModel _gameplayUIModel;
  GameplayUIPresenter _gameplayUIPresenter;

  private void InitializeGameplayUI()
  {
    _gameplayUIModel = new();
    _gameplayUIPresenter = new(_gameplayUIModel, _gameplayUIView);
  }

  [SerializeField] Sequence1View _sequence1View;
  Sequence1Model _sequence1Model;
  Sequence1Presenter _sequence1Presenter;

  private void InitializeS1()
  {
    _sequence1Model = new();
    _sequence1Presenter = new(_sequence1Model, _sequence1View);
  }
}
