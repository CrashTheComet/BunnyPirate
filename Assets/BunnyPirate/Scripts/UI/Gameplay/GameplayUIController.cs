using UnityEngine;

public class GameplayUIController : MonoBehaviour
{
    void Awake()
    {
        InitializeGameplayUI();
        //InitializeS1();

        GameManager.Register(this);
    }

    [SerializeField] GameplayUIView _gameplayUIView;

    GameplayUIModel _gameplayUIModel;

    private void InitializeGameplayUI()
    {
        _gameplayUIModel = new();
    }

    //Sequence1Controller _sequence1Model;

    //private void InitializeS1()
    //{
    //    _sequence1Model = Sequence1Controller.instance;
    //}

    //public void OpenCloseGameMap(bool isOpen)
    //{
    //    _sequence1Model.ShowGameMap(isOpen);
    //}
}
