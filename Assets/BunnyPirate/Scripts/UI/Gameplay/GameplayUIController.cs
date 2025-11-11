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

  private void InitializeGameplayUI()
  {
    _gameplayUIModel = new();
  }

  [SerializeField] Sequence1View _sequence1View;
  SequenceController _sequence1Model;

  private void InitializeS1()
  {
    _sequence1Model = new();
  }

    public void OpenCloseGameMap(bool isOpen)
    {
        _sequence1Model.ShowGameMap(isOpen);
    }
}
