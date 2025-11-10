using UnityEngine;

public class MainMenuController : MonoBehaviour
{
  void Awake()
  {
    InitializeMainMenu();
  }

  public MainMenuModel _mainMenuModel { get; private set; }
  private MainMenuPresenter _mainMenuPresenter;

  [SerializeField] private MainMenuView _mainMenuView;

  private void InitializeMainMenu()
  {
    _mainMenuModel = new MainMenuModel();
    _mainMenuPresenter = new(_mainMenuModel, _mainMenuView);
  }
}
